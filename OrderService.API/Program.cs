using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OrderService.API.WorkerService;
using OrderService.API.WorkerService.Business;
using OrderService.API.WorkerService.Event.Consumer.Projection;
using OrderService.API.WorkerService.Event.Publisher;
using OrderService.API.WorkerService.Reconciliation;
using OrderService.Application.Handler.CreateOrder;
using OrderService.Application.Interface;
using OrderService.Application.Interface.Cache;
using OrderService.Application.Interface.Metrics;
using OrderService.Application.Interface.RateLimit;
using OrderService.Application.Interface.Repository;
using OrderService.Infrastructure.Cache;
using OrderService.Infrastructure.Configuration.Kafka;
using OrderService.Infrastructure.Observability;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Persistence.Repository;
using OrderService.Infrastructure.Pressure;
using OrderService.Infrastructure.RateLimit;
using StackExchange.Redis;

namespace OrderService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //Repository
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
            builder.Services.AddScoped<IOutboxEventRepository, OutboxRepository>();
            builder.Services.AddScoped<IOrderProjectionRepository, OrderProjectionRepository>();

            builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            builder.Services.AddScoped<IPressureGate, PressureGate>();
            builder.Services.AddScoped<IOrderSummaryCache, InMemoryOrderSummaryCache>();
            
            
            //Uses singleton bcs its process-wide, single. not per request, but per system
            builder.Services.AddSingleton<IOrderMetric, OTelOrderMetricRecorder>();
            builder.Services.AddSingleton<IPressureMetric, OTelPressureMetricRecorder>();
            builder.Services.AddSingleton<IConcurrencyMetric, OTelConcurrencyMetricRecorder>();
            builder.Services.AddSingleton<IConcurrencyLimiter>(sp =>
            {
                var metric = sp.GetRequiredService<IConcurrencyMetric>();
                var capacity = 1; // config later
                return new ConcurrencyLimiter(capacity, metric);
            });

            var useRedis = builder.Configuration.GetValue<bool>("Features:UseRedis");
            if (useRedis)
            {
                builder.Services.AddScoped<ISharedCounterCache, RedisSharedCounterCache>();
                builder.Services.AddSingleton<IRateLimiter, RedisRateLimiter>();

                builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    //var configuration = builder.Configuration.GetConnectionString("Redis");
                    var configuration = Environment.GetEnvironmentVariable("REDIS_CONNECTION");

                    //If redis isnt up/connected, abort connection
                    var options = ConfigurationOptions.Parse(configuration);
                    options.AbortOnConnectFail = false;

                    return ConnectionMultiplexer.Connect(options);
                });
            }

            builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));

            //Background Worker
            //builder.Services.AddHostedService<OrderCompletionWorker>();
            //builder.Services.AddHostedService<OrderExpirationWorker>();
            //builder.Services.AddHostedService<OutboxWorker>();
            //builder.Services.AddHostedService<OrderReconciliationWorker>();
            builder.Services.AddHostedService<EventPublishWorker>();
            builder.Services.AddHostedService<OrderProjectionConsumerWorker>();


            //Conn strings
            builder.Services.AddDbContext<OrderDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

            //MediatR
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
            });

            builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter("OrderService.Metrics")
                    .AddMeter("OrderService.Pressure")
                    .AddMeter("OrderService.Concurrency")
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();
            });

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.MapPrometheusScrapingEndpoint();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            var hostedServices = app.Services.GetServices<IHostedService>();
            Console.WriteLine($"HostedService count: {hostedServices.Count()}");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                const int maxRetries = 10;

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        db.Database.Migrate();
                        Console.WriteLine("Database migration successful.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Migration attempt {attempt} failed: {ex.Message}");

                        if (attempt == maxRetries)
                            throw;

                        Thread.Sleep(3000);
                    }
                }
            }

            app.Run();
        }
    }
}

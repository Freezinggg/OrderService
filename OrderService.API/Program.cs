using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OrderService.API.WorkerService;
using OrderService.Application.Handler.CreateOrder;
using OrderService.Application.Interface;
using OrderService.Application.Interface.Cache;
using OrderService.Infrastructure.Cache;
using OrderService.Infrastructure.Observability;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Pressure;

namespace OrderService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
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


            //Background Worker
            builder.Services.AddHostedService<OrderCompletionWorker>();
            builder.Services.AddHostedService<OrderExpirationWorker>();

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

            app.Run();
        }
    }
}

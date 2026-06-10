using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace OrderService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KafkaTestController : ControllerBase
    {
        [HttpPost("kafka")]
        public async Task<IActionResult> Kafka_Test()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "kafka:9092"
            };

            var producer = new ProducerBuilder<Null, string>(config).Build();

            var evt = new
            {
                EventType = "OrderCreated",
                OrderId = 999
            };

            //Message
            await producer.ProduceAsync(
            "order-events",
            new Message<Null, string> { Value = JsonConvert.SerializeObject(evt) });

            return Ok();
        }
    }
}

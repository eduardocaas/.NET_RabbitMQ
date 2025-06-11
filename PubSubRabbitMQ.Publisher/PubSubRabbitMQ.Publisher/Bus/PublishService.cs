using PubSubRabbitMQ.Publisher.RabbitMQ;
using RabbitMQ.Client;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace PubSubRabbitMQ.Publisher.Bus
{
    public class PublishService : IPublishService
    {
        private string _exchange = string.Empty;
        private IChannel _channel;

        public PublishService(IRabbitMqService rabbitMqService, string exchange)
        {
            _exchange = exchange;
            //Task.Run(() => InitializeAsync(rabbitMqService)).ConfigureAwait(false);
            InitializeAsync(rabbitMqService).GetAwaiter().GetResult();
        }

        public async Task InitializeAsync(IRabbitMqService rabbitMqService) 
            => _channel = await rabbitMqService.CreateChannel();
        

        public void Publish<T>(string routingKey, T message)
        {     
            var json = JsonSerializer.Serialize(message);
            var byteArray = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublishAsync(_exchange, routingKey, byteArray);
        }
    }
}

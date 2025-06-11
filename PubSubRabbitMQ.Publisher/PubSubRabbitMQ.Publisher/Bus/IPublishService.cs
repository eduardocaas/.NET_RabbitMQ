using PubSubRabbitMQ.Publisher.RabbitMQ;

namespace PubSubRabbitMQ.Publisher.Bus
{
    public interface IPublishService
    {
        protected Task InitializeAsync(IRabbitMqService rabbitMqService);
        void Publish<T>(string routingKey, T message);
    }
}

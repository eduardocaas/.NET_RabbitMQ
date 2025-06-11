using RabbitMQ.Client;

namespace PubSubRabbitMQ.Publisher.RabbitMQ
{
    public interface IRabbitMqService
    {
        Task<IChannel> CreateChannel();
    }
}

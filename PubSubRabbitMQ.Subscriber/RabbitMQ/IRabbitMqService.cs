using RabbitMQ.Client;

namespace PubSubRabbitMQ.Subscriber.RabbitMQ
{
    public interface IRabbitMqService
    {
        Task<IConnection> CreateConnection();
        Task<IChannel> CreateChannel();
    }
}

using RabbitMQ.Client;

namespace PubSubRabbitMQ.Subscriber.RabbitMQ
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly IConnection _connection;

        public RabbitMqService(IConnection connection)
        {
            this._connection = this.CreateConnection().GetAwaiter().GetResult();
        }

        public async Task<IConnection> CreateConnection()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                ClientProvidedName = "app-consumer"
            };

            var connection = await connectionFactory.CreateConnectionAsync();
            return connection;
        }

        public Task<IChannel> CreateChannel()
            => _connection.CreateChannelAsync();
    }
}

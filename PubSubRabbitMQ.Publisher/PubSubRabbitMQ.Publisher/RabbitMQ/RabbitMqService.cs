using RabbitMQ.Client;

namespace PubSubRabbitMQ.Publisher.RabbitMQ
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly IConnection _connection;

        public RabbitMqService()
        {
            _connection = CreateConnection().GetAwaiter().GetResult();
        }

        public async Task<IConnection> CreateConnection()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                ClientProvidedName = "customer-app"
            };

            var connection =  await connectionFactory.CreateConnectionAsync();
            return connection;
        }

        public async Task<IChannel> CreateChannel()
            => await _connection.CreateChannelAsync();
    }
}

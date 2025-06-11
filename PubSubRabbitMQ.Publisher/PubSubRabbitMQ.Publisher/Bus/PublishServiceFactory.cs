using PubSubRabbitMQ.Publisher.RabbitMQ;

namespace PubSubRabbitMQ.Publisher.Bus
{
    public class PublishServiceFactory
    {
        private readonly IRabbitMqService _rabbitMqService;

        public PublishServiceFactory(IRabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        public IPublishService Create(string exchange)
        {
            return new PublishService(_rabbitMqService, exchange);
        }
    }
}

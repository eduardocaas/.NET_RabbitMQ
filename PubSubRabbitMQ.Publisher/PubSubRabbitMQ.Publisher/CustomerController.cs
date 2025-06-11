using System.Text;
using System.Text.Json;
using PubSubRabbitMQ.Publisher.Bus;
using PubSubRabbitMQ.Publisher.RabbitMQ;

namespace PubSubRabbitMQ.Publisher
{
    public class CustomerController
    {
        const string RK_CREATED = "customer.created";
        const string EX_CUSTOMER = "customers";

        private readonly IPublishService _publishService;

        public CustomerController(PublishServiceFactory publishFactory)
        {
            _publishService = publishFactory.Create(EX_CUSTOMER);
        }

        public void Save(in Tuple<int, string> customer)
        {
            _publishService.Publish(RK_CREATED, customer);
        }
    }
}

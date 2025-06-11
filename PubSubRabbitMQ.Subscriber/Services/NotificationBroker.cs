namespace PubSubRabbitMQ.Subscriber.Services
{
    public class NotificationBroker
    {
        public NotificationBroker()
        {
            
        }

        public event Action<Object> NotificationHandler;

        public void Send(object notification)
        {
            Console.WriteLine("Message sent");
            NotificationHandler?.Invoke(notification);
        }
    }
}

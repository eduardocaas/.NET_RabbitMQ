namespace PubSubRabbitMQ.Subscriber.Services
{
    public interface INotificationService
    {
        void SendNotification(object notification);
    }
}

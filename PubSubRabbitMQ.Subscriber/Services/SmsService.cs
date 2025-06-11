namespace PubSubRabbitMQ.Subscriber.Services
{
    public class SmsService : INotificationService
    {
        public void SendNotification(object notification)
        {
            Console.WriteLine("Send SMS");
        }
    }
}

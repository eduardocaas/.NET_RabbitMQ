namespace PubSubRabbitMQ.Subscriber.Services
{
    public class EmailService : INotificationService
    {
        public void SendNotification(object notification)
        {
            Console.WriteLine("Send EMAIL");
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PubSubRabbitMQ.Subscriber.Models;
using PubSubRabbitMQ.Subscriber.RabbitMQ;
using PubSubRabbitMQ.Subscriber.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PubSubRabbitMQ.Subscriber.Subscribers
{
    public class CustomerCreatedSubscriber : IHostedService
    {
        const string QE_CUSTOMER_CREATED = "customer-created";

        private readonly IChannel _channel;
        public IServiceProvider Services { get; }

        public CustomerCreatedSubscriber(
            IServiceProvider services, 
            IRabbitMqService rabbitMqService)
        {
            Services = services;
            _channel = rabbitMqService.CreateChannel().GetAwaiter().GetResult();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                try
                {
                    Console.WriteLine("[x] Received message ID: {0}", eventArgs.BasicProperties.MessageId);

                    var content = eventArgs.Body.ToArray();
                    var contentString = Encoding.UTF8.GetString(content);

                    var message = JsonSerializer.Deserialize<CustomerCreated>(contentString);

                    // Envia para serviços de notificações
                    SendNotifications(message);

                    // Confirma mensagem
                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    var headers = eventArgs.BasicProperties.Headers;
                    int count = 0;

                    // Se headers for nulo, cria novo e já coloca x-death-count igual 1
                    if (headers == null)
                    {
                        headers = new Dictionary<string, object?>();
                        headers["x-death-count"] = 1;
                    }
                    else
                    {
                        // Atualiza valor do x-death-count
                        if (headers.ContainsKey("x-death-count"))
                        {
                            count = Convert.ToInt16(headers["x-death-count"]);
                            headers["x-death-count"] = ++count;
                        }
                    }

                    Console.WriteLine($"\n{ex.Message} ||| Actual x-death-count value: {headers["x-death-count"]}");

                    // Se for maior que 3 nega mensagem e requeue, mensagem vai para DLX que manda para a DLQ
                    if (count > 3)
                    {
                        Console.WriteLine("| --- Nack message and no requeue (exceed x-death-count) ||| SENDING TO DLX\n");
                        await _channel.BasicNackAsync(eventArgs.DeliveryTag, false, false);
                    }
                    else
                    {
                        // Propriedades da mensagem são imutáveis, sendo necessário criar nova mensagem com propriedades novas
                        var newProperties = new BasicProperties();
                        newProperties.Headers = headers;

                        // TODO: Ver se MessageId = 0 ou Nulo, se for, criar novo GUID
                        newProperties.MessageId = eventArgs.BasicProperties.MessageId;

                        // Envia mensagem nova com valor atualizado do header
                        await _channel.BasicPublishAsync(
                            exchange: eventArgs.Exchange,
                            routingKey: eventArgs.RoutingKey,
                            mandatory: false,
                            basicProperties: newProperties,
                            body: eventArgs.Body.ToArray());

                        // Confirma a mensagem anterior
                        await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);
                    } 
                }
            };

            _channel.BasicConsumeAsync(queue: QE_CUSTOMER_CREATED, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public void SendNotifications(CustomerCreated customer)
        {
            using (var scope = Services.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<NotificationBroker>();
                service.Send(customer);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

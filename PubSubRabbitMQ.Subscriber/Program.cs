using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PubSubRabbitMQ.Subscriber.RabbitMQ;
using PubSubRabbitMQ.Subscriber.Services;
using PubSubRabbitMQ.Subscriber.Subscribers;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<NotificationBroker>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddHostedService<CustomerCreatedSubscriber>();

IHost host = builder.Build();
host.Run();

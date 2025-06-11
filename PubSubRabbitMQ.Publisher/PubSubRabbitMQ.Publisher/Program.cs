using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PubSubRabbitMQ.Publisher.Bus;
using PubSubRabbitMQ.Publisher.RabbitMQ;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddScoped<PublishServiceFactory>();

using IHost host = builder.Build();

await host.RunAsync();
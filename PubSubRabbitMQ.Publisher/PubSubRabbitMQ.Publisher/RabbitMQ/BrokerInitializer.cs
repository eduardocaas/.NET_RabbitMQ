using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace PubSubRabbitMQ.Publisher.RabbitMQ
{
    public class BrokerInitializer
    {
        private IChannel? _channel;

        public BrokerInitializer(IRabbitMqService rabbitMqService)
        {
            InitializeAsync(rabbitMqService).GetAwaiter().GetResult();
        }

        public async Task InitializeAsync(IRabbitMqService rabbitMqService)
        {
            _channel = await rabbitMqService.CreateChannel();

            // LOGS
            InitializeLog();
            InitializeLogDeadLetter();

            // CUSTOMER
            InitializeCustomer();
            InitializeCustomerCreated();
            InitializeCustomerDeadLetter();
        }

        // Método auxiliar para criar Queue
        async void DeclareQueueIfNotExists(string queueName, IDictionary<string, Object?> args)
        {
            try
            {
                await _channel.QueueDeclarePassiveAsync(queueName);
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason.ReplyCode == 404)
            {
                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: args,
                    noWait: false);
            }
        }

        // Método auxiliar para criar Exchange
        async void DeclareExchangeIfNotExists(string exchangeName, string exchangeType)
        {
            try
            {
                await _channel.ExchangeDeclarePassiveAsync(exchangeName);
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason.ReplyCode == 404)
            {
                await _channel.ExchangeDeclareAsync(
                    exchange: exchangeName,
                    type: exchangeType,
                    durable: true,
                    autoDelete: false,
                    noWait: false);
            }
        }

        private void InitializeLog()
        {
            // Cria Exchange -> Topic
            DeclareExchangeIfNotExists("EX_LOG", ExchangeType.Topic);


            // Argumentos para TTL e DLX
            IDictionary<string, Object?> qeLogArgs = new Dictionary<string, Object?>
            {
                { "x-message-ttl", 10000 },
                { "x-dead-letter-exchange", "DLX_LOG" }
            };

            // Cria queues para Log
            DeclareQueueIfNotExists("QE_LOG_ALL", qeLogArgs);
            DeclareQueueIfNotExists("QE_LOG_CUSTOMER", qeLogArgs);

            // Bindings -> tipo pattern (topic)
            _channel.QueueBindAsync(
                queue: "QE_LOG_ALL",
                exchange: "EX_LOG",
                routingKey: "log.#",
                noWait: false);

            _channel.QueueBindAsync(
                queue: "QE_LOG_CUSTOMER",
                exchange: "EX_LOG",
                routingKey: "log.*.customer",
                noWait: false);
        }

        [Obsolete]
        private void OLD_InitalizeLog()
        {
            // Cria Exchange -> Topic
            _channel.ExchangeDeclareAsync(
                exchange: "EX_LOG",
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                noWait: false);

            // Argumentos para TTL e DLX
            IDictionary<string, Object?> qeLogArgs = new Dictionary<string, Object?>
            {
                { "x-message-ttl", 10000 },
                { "x-dead-letter-exchange", "DLX_LOG" }
            };

            // Queues de Log
            _channel.QueueDeclareAsync(
                queue: "QE_LOG_ALL",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: qeLogArgs,
                noWait: false);

            _channel.QueueDeclareAsync(queue: "QE_LOG_CUSTOMER",
                  durable: true,
                  exclusive: false,
                  autoDelete: false,
                  arguments: qeLogArgs,
                  noWait: false);

            // Bindings -> tipo pattern (topic)
            _channel.QueueBindAsync(
                queue: "QE_LOG_ALL",
                exchange: "EX_LOG",
                routingKey: "log.#",
                noWait: false);

            _channel.QueueBindAsync(
                queue: "QE_LOG_CUSTOMER",
                exchange: "EX_LOG",
                routingKey: "log.*.customer",
                noWait: false);
        }

        private void InitializeLogDeadLetter()
        {

            // Cria Exchange DLX -> Headers
            DeclareExchangeIfNotExists("DLX_LOG", ExchangeType.Headers);

            // DLQ de Expired
            DeclareQueueIfNotExists("DLQ_LOG_EXPIRED", new Dictionary<string, Object?>());

            // DLQ de Rejected
            DeclareQueueIfNotExists("DLQ_LOG_REJECTED", new Dictionary<string, Object?>());

            // Args de binding (Headers) - para expired
            IDictionary<string, Object?> bindLogExpiredArgs = new Dictionary<string, Object?>
            {
                { "x-first-death-reason", "expired" },
                { "x-match", "all-with-x" }
            };

            // Bind de Expired
            _channel.QueueBindAsync(
                queue: "DLQ_LOG_EXPIRED",
                exchange: "DLX_LOG",
                routingKey: string.Empty,
                arguments: bindLogExpiredArgs,
                noWait: false);

            // Args de binding (Headers) - para rejected
            IDictionary<string, Object?> bindLogRejectedArgs = new Dictionary<string, Object?>
            {
                { "x-first-death-reason", "rejected" },
                { "x-match", "all-with-x" }
            };

            // Bind de Reject
            _channel.QueueBindAsync(
                queue: "DLQ_LOG_REJECTED",
                exchange: "DLX_LOG",
                routingKey: string.Empty,
                arguments: bindLogRejectedArgs,
                noWait: false);
        }

        [Obsolete]
        private void OLD_InitializeLogDeadLetter()
        {
            // Cria Exchange DLX -> Headers
            _channel.ExchangeDeclareAsync(
               exchange: "DLX_LOG",
               type: ExchangeType.Headers,
               durable: true,
               autoDelete: false,
               noWait: false);

            // DLQ de Expired
            _channel.QueueDeclareAsync(
               queue: "DLQ_LOG_EXPIRED",
               durable: true,
               exclusive: false,
               autoDelete: false,
               noWait: false);

            // DLQ de Rejected
            _channel.QueueDeclareAsync(queue: "DLQ_LOG_REJECTED",
                durable: true,
                exclusive: false,
                autoDelete: false,
                noWait: false);

            // Args de binding (Headers) - para expired
            IDictionary<string, Object?> bindLogExpiredArgs = new Dictionary<string, Object?>
            {
                { "x-first-death-reason", "expired" },
                { "x-match", "all-with-x" }
            };

            // Bind de Expired
            _channel.QueueBindAsync(
                queue: "DLQ_LOG_EXPIRED",
                exchange: "DLX_LOG",
                routingKey: string.Empty,
                arguments: bindLogExpiredArgs,
                noWait: false);

            // Args de binding (Headers) - para rejected
            IDictionary<string, Object?> bindLogRejectedArgs = new Dictionary<string, Object?>
            {
                { "x-first-death-reason", "rejected" },
                { "x-match", "all-with-x" }
            };

            // Bind de Reject
            _channel.QueueBindAsync(
                queue: "DLQ_LOG_REJECTED",
                exchange: "DLX_LOG",
                routingKey: string.Empty,
                arguments: bindLogRejectedArgs,
                noWait: false);
        }

        private void InitializeCustomer()
        {
            // Exchange para Customer -> Direct
            DeclareExchangeIfNotExists("EX_CUSTOMER", ExchangeType.Direct);
        }

        [Obsolete]
        private void OLD_InitializeCustomer()
        {
            // Exchange para Customer -> Direct
            _channel.ExchangeDeclareAsync(
                exchange: "EX_CUSTOMER",
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                noWait: false);
        }

        private void InitializeCustomerCreated()
        {
            // Argumentos para criação de queue -> incluindo TTL e DLX
            IDictionary<string, Object?> qeCustomerArgs = new Dictionary<string, Object?>
            {
                { "x-message-ttl", 10000 },
                { "x-dead-letter-exchange", "DLX_CUSTOMER" }
            };

            // Cria queue para Customer (created)
            DeclareQueueIfNotExists("QE_CUSTOMER_CREATED", qeCustomerArgs);

            // Criação de Binding para Customer (created) 
            _channel.QueueBindAsync(
                queue: "QE_CUSTOMER_CREATED",
                exchange: "EX_CUSTOMER",
                routingKey: "customer.created");
        }

        [Obsolete]
        private void OLD_InitializeCustomerCreated()
        {
            // Argumentos para criação de queue -> incluindo TTL e DLX
            IDictionary<string, Object?> qeCustomerArgs = new Dictionary<string, Object?>
            {
                { "x-message-ttl", 10000 },
                { "x-dead-letter-exchange", "DLX_CUSTOMER" }
            };

            // Cria queue para Customer (created)
            _channel.QueueDeclareAsync(
                queue: "QE_CUSTOMER_CREATED",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: qeCustomerArgs,
                noWait: false);

            // Criação de Binding para Customer (created) 
            _channel.QueueBindAsync(
                queue: "QE_CUSTOMER_CREATED",
                exchange: "EX_CUSTOMER",
                routingKey: "customer.created");
        }

        private void InitializeCustomerDeadLetter()
        {
            // Cria DLQ para Customer -> expired
            DeclareQueueIfNotExists("DLQ_CUSTOMER_EXPIRED", new Dictionary<string, Object?>());

            // Cria DLQ para Customer -> rejected
            DeclareQueueIfNotExists("DLQ_CUSTOMER_REJECTED", new Dictionary<string, Object?>());

            // Cria DLX para customer -> Headers
            DeclareExchangeIfNotExists("DLX_CUSTOMER", ExchangeType.Headers);

            // Argumentos para Binding -> expired (headers)
            IDictionary<string, Object?> bindCustomerExpiredArgs = new Dictionary<string, Object?>
            {
                { "x-first-death-reason", "expired" },
                { "x-match", "all-with-x" }
            };

            // Binding para Expired
            _channel.QueueBindAsync(
                queue: "DLQ_CUSTOMER_EXPIRED",
                exchange: "DLX_CUSTOMER",
                routingKey: string.Empty,
                arguments: bindCustomerExpiredArgs,
                noWait: false);

            // Argumentos para Binding -> rejected (headers)
            IDictionary<string, Object?> bindCustomerRejectedArgs = new Dictionary<string, Object?>
            {
                { "x-first-death-reason", "rejected" },
                { "x-match", "all-with-x" }
            };

            // Binding para Rejected
            _channel.QueueBindAsync(
                queue: "DLQ_CUSTOMER_REJECTED",
                exchange: "DLX_CUSTOMER",
                routingKey: string.Empty,
                arguments: bindCustomerRejectedArgs,
                noWait: false);
        }

        [Obsolete]
        private void OLD_InitializeCustomerDeadLetter()
        {
            // Cria DLQ para Customer -> expired
            _channel.QueueDeclareAsync(
               queue: "DLQ_CUSTOMER_EXPIRED",
               durable: true,
               exclusive: false,
               autoDelete: false,
               noWait: false);

            // Cria DLQ para Customer -> rejected
            _channel.QueueDeclareAsync(
              queue: "DLQ_CUSTOMER_REJECTED",
              durable: true,
              exclusive: false,
              autoDelete: false,
              noWait: false);

            // Cria DLX para customer -> Headers
            _channel.ExchangeDeclareAsync(
                exchange: "DLX_CUSTOMER",
                type: ExchangeType.Headers,
                durable: true,
                autoDelete: false,
                noWait: false);

            // Argumentos para Binding -> expired (headers)
            IDictionary<string, Object?> bindCustomerExpiredArgs = new Dictionary<string, Object?>
            {
                { "x-first-death-reason", "expired" },
                { "x-match", "all-with-x" }
            };

            // Binding para Expired
            _channel.QueueBindAsync(
                queue: "DLQ_CUSTOMER_EXPIRED",
                exchange: "DLX_CUSTOMER",
                routingKey: string.Empty,
                arguments: bindCustomerExpiredArgs,
                noWait: false);

            // Argumentos para Binding -> rejected (headers)
            IDictionary<string, Object?> bindCustomerRejectedArgs = new Dictionary<string, Object?>
            {
                { "x-first-death-reason", "rejected" },
                { "x-match", "all-with-x" }
            };

            // Binding para Rejected
            _channel.QueueBindAsync(
                queue: "DLQ_CUSTOMER_REJECTED",
                exchange: "DLX_CUSTOMER",
                routingKey: string.Empty,
                arguments: bindCustomerRejectedArgs,
                noWait: false);
        }
    }
}

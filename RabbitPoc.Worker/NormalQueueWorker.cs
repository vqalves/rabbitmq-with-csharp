using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClassLibrary1.Exchanges;
using ClassLibrary1.Messages;
using ClassLibrary1.Queues;
using ClassLibrary1.Structure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitPoc.Worker
{
    public class NormalQueueWorker : BackgroundService
    {
        /*
        private readonly NotificacaoBusiness notificacaoBusiness;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            this.notificacaoBusiness = new NotificacaoBusiness();
            _logger = logger;
        }

        public void Show(string message)
        {
            var time = DateTimeOffset.Now;
            _logger.LogInformation($"{time} -> {message}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscription = this.notificacaoBusiness.Subscribe(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await subscription.NextMessageAsync();
                await Task.Delay(1000);
                Show(message);
            }
        }
        */

        private readonly ILogger Logger;
        private readonly ConnectionFactory ConnectionFactory;
        private readonly Guid WorkerId;
        private ProcessMessageDictionary ProcessMessageDictionary;

        private IConnection Connection;
        private IModel Channel;

        public NormalQueueWorker(ConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
        {
            this.WorkerId = Guid.NewGuid();

            this.ConnectionFactory = connectionFactory;

            this.ProcessMessageDictionary = new ProcessMessageDictionary();
            this.ProcessMessageDictionary.Register<ExampleMessage1>(ExampleMessage1.TypeID, Work);
            this.ProcessMessageDictionary.Register<ExampleMessage2>(ExampleMessage2.TypeID, Work);

            this.Logger = loggerFactory.CreateLogger<NormalQueueWorker>();
            this.Logger.LogInformation($"Created {WorkerId.ToString("n")}");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                this.Connection = ConnectionFactory.CreateConnection();
                this.Channel = Connection.CreateModel();

                MainQueue.DeclareQueue(Channel);
            }

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var consumer = new EventingBasicConsumer(Channel);
                consumer.Received += async (ch, ea) =>
                {
                    var message = MessageMethods.ToJson(ea.Body);
                    var executionResult = await ProcessMessageDictionary.Execute(message);

                    if (executionResult.Processed)
                    {
                        // If it's invoked by a RPC, enqueue the response to the specified queue
                        if(!string.IsNullOrWhiteSpace(ea.BasicProperties?.ReplyTo))
                        {
                            var messageProperties = Channel.CreateBasicProperties();
                            messageProperties.CorrelationId = ea.BasicProperties.CorrelationId;

                            Channel.BasicPublish(exchange: DefaultExchange.ExchangeName, routingKey: ea.BasicProperties.ReplyTo, basicProperties: messageProperties, body: executionResult.Result);
                        }

                        // Acknowledge the message was processed
                        Channel.BasicAck(ea.DeliveryTag, false);
                    }
                };

                Channel.BasicConsume(MainQueue.QueueName, false, consumer);
            }

            return Task.CompletedTask;
        }

        private async Task<byte[]> Work(ExampleMessage1 message)
        {
            Logger.LogInformation($"Work1: {message.Name} / {message.Date} - @{DateTime.Now}");

            var result = new ExampleResponse1()
            {
                ProcessedDate = DateTime.Now,
                ProcessedMessage = message
            };

            var resultBody = MessageMethods.ToByte(ExampleResponse1.TypeID, result);
            return await Task.FromResult(resultBody);
        }

        private async Task<byte[]> Work(ExampleMessage2 message)
        {
            Logger.LogInformation($"Work2: {message.Guid}");
            return await Task.FromResult<byte[]>(null);
        }

        /*
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if(!cancellationToken.IsCancellationRequested)
            {
                var factory = new ConnectionFactory { HostName = "localhost" };

                this._connection = factory.CreateConnection();
                this._channel = _connection.CreateModel();
                this._channel.QueueDeclare(queue: "test", durable: true, exclusive: false, autoDelete: false, arguments: null);
            }

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (ch, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation($"{_id.ToString("n")} -> {message} Processed: {DateTime.Now.ToString("mm:ss.fff")}");

                    // handle the received message  
                    _channel.BasicAck(ea.DeliveryTag, false);
                };

                _channel.BasicConsume("test", false, consumer);
            }

            return Task.CompletedTask;
        }
        */

        public override void Dispose()
        {
            Channel.Close();
            Connection.Close();
            base.Dispose();
        }
    }
}
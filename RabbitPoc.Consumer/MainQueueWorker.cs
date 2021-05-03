using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitPoc.MQCommon.Exchanges;
using RabbitPoc.MQCommon.Messages;
using RabbitPoc.MQCommon.Queues;
using RabbitPoc.MQCommon.Structure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitPoc.Consumer
{
    public class MainQueueWorker : BackgroundService
    {
        private readonly Guid WorkerId;
        private ProcessMessageDictionary ProcessMessageDictionary;

        private readonly ConnectionFactory ConnectionFactory;
        private readonly ILogger Logger;

        private IConnection Connection;
        private IModel Channel;


        public MainQueueWorker(ConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
        {
            this.WorkerId = Guid.NewGuid();

            this.ConnectionFactory = connectionFactory;

            this.ProcessMessageDictionary = new ProcessMessageDictionary();
            this.ProcessMessageDictionary.Register<ExampleMessage1>(ExampleMessage1.TypeID, Work);
            this.ProcessMessageDictionary.Register<ExampleMessage2>(ExampleMessage2.TypeID, Work);

            this.Logger = loggerFactory.CreateLogger<MainQueueWorker>();
            this.Logger.LogInformation($"Created {WorkerId.ToString("n")}");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            // Start connection with RabbitMQ and keep alive while the worker is up
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

                var consumer = new AsyncEventingBasicConsumer(Channel);
                consumer.Received += async (ch, ea) =>
                {
                    var message = WrappedMessage.ToJson(ea.Body);
                    var executionResult = await ProcessMessageDictionary.Execute(message);

                    if (executionResult.Processed)
                    {
                        // If it's invoked as an RPC, enqueue the response to the specified queue
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
            Logger.LogInformation($"Work1: {message.SentMessage} / {message.Date} - @{DateTime.Now}");

            var result = new ExampleResponse1()
            {
                ProcessedDate = DateTime.Now,
                ProcessedMessage = message
            };

            var resultBody = WrappedMessage.Wrap(ExampleResponse1.TypeID, result).ToBytes();
            return await Task.FromResult(resultBody);
        }

        private async Task<byte[]> Work(ExampleMessage2 message)
        {
            Logger.LogInformation($"Work2: {message.Guid}");
            return await Task.FromResult<byte[]>(null);
        }

        public override void Dispose()
        {
            Channel.Close();
            Connection.Close();
            base.Dispose();
        }
    }
}
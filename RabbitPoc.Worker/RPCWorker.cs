using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitPoc.Worker
{
    public class RPCWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private Guid _id;

        public RPCWorker(ILoggerFactory loggerFactory)
        {
            this._id = Guid.NewGuid();
            this._logger = loggerFactory.CreateLogger<RPCWorker>();

            _logger.LogInformation($"Created {_id.ToString("n")}");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if(!cancellationToken.IsCancellationRequested)
            {
                var factory = new ConnectionFactory { HostName = "localhost" };

                this._connection = factory.CreateConnection();
                this._channel = _connection.CreateModel();
                this._channel.QueueDeclare(queue: "exemplo_rpc", durable: true, exclusive: false, autoDelete: false, arguments: null);

                this._channel.BasicQos(0, 1, false);
            }

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var consumer = new EventingBasicConsumer(_channel);
                _channel.BasicConsume(queue: "exemplo_rpc", autoAck: false, consumer: consumer);

                consumer.Received += (model, ea) =>
                {
                    string response = null;

                    var body = ea.Body.ToArray();
                    var props = ea.BasicProperties;
                    var replyProps = _channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        response = $"{_id.ToString("n")} -> {message} Processed: {DateTime.Now.ToString("MM:ss.fff")}";

                        _logger.LogInformation(response);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(" [.] " + e.Message);
                        response = "<Exception>";
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        _channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                };
            }

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitPoc.Worker
{
    public class NotificacaoBusiness
    {
        private readonly ConnectionFactory factory;

        public NotificacaoBusiness()
        {
            this.factory = new ConnectionFactory() { HostName = "localhost" };
        }

        public Subscription Subscribe(CancellationToken stoppingToken) => new Subscription(factory, stoppingToken);

        public class Subscription : IDisposable
        {
            private IConnection connection;
            private IModel channel;
            private bool disposedValue;

            private TaskCompletionSource<string> currentTaskSource;
            private Task<string> currentTask;

            private CancellationToken stoppingToken;
            private CancellationTokenRegistration? unsafeRegisterCancellationToken;

            public Subscription(ConnectionFactory factory, CancellationToken stoppingToken)
            {
                this.stoppingToken = stoppingToken;
                this.unsafeRegisterCancellationToken = stoppingToken.UnsafeRegister(Cancel, null);

                NewEmptyTask();

                this.connection = factory.CreateConnection();
                this.channel = connection.CreateModel();
                this.channel.QueueDeclare(queue: "test", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += HandleMessageReceived;

                this.channel.BasicConsume(queue: "test", autoAck: true, consumer: consumer);
            }

            private void HandleMessageReceived(object? model, BasicDeliverEventArgs args)
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var oldTaskSource = currentTaskSource;
                NewEmptyTask();
                oldTaskSource.SetResult(message);
            }

            private void NewEmptyTask()
            {
                this.currentTaskSource = new TaskCompletionSource<string>(TaskCreationOptions.AttachedToParent);
                this.currentTask = currentTaskSource.Task;
            }

            private void Cancel(object? args)
            {
                this.currentTaskSource.SetCanceled();
            }

            public async Task<string> NextMessageAsync()
            {
                if (stoppingToken.IsCancellationRequested)
                    return await Task.FromCanceled<string>(stoppingToken);

                return await currentTask;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        if(connection != null)
                        {
                            connection.Dispose();
                            connection = null;
                        }

                        if(channel != null)
                        {
                            channel.Dispose();
                            channel = null;
                        }

                        if(unsafeRegisterCancellationToken.HasValue)
                        {
                            unsafeRegisterCancellationToken.Value.Dispose();
                            unsafeRegisterCancellationToken = null;
                        }
                    }

                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}

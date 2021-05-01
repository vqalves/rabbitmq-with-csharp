using RabbitPoc.MQCommon.Queues;
using RabbitMQ.Client;
using System;

namespace RabbitPoc.MQCommon.Structure
{
    public class ConnectionHolder : IDisposable
    {
        private ConnectionFactory connectionFactory;

        private IConnection _connection;
        protected IConnection Connection 
        {  
            get => _connection ?? (_connection = connectionFactory.CreateConnection());
            set => _connection = value;
        }

        private IModel _channel;
        protected IModel Channel 
        { 
            get => _channel ?? (_channel = Connection.CreateModel());
            set => _channel = value;
        }

        public ConnectionHolder(ConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        #region Dispose pattern
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(Connection != null)
                    {
                        Connection.Dispose();
                        Connection = null;
                    }

                    if(Channel != null)
                    {
                        Channel.Dispose();
                        Channel = null;
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
        #endregion
    }
}

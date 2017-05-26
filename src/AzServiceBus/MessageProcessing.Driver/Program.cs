using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessing.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new MessageSenderConfig();
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            string sharedAccessKey = "";
            string keyName = "";
            string queuePath = "CreateCart";
            string namespaceAddress = "";
            var senderFactory = MessagingFactory.Create(
                namespaceAddress,
                new MessagingFactorySettings
                {
                    OperationTimeout = TimeSpan.FromSeconds(20),
                    TransportType = (TransportType)Enum.Parse(typeof(TransportType),"Amqp",true),
                    TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(keyName, sharedAccessKey)
                });
            var sender = await senderFactory.CreateMessageSenderAsync(queuePath);
        }
    }

    public class MessageSenderConfig
    {
        public string NamespaceAddress { get; set; }
        public string KeyName { get; set; }
        public string SharedAccessKey { get; set; }
        public string Path { get; set; }
        public string TransportType { get; set; } = "Amqp";
        public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(20);
    }
}

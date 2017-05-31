namespace MessageProcessing
{
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using System;
    using System.Threading.Tasks;

    public static class MessageSenderFactory
    {
        public static async Task<Microsoft.ServiceBus.Messaging.MessageSender> CreateMessageSenderAsync(MessageSenderConfig messageSenderConfig)
        {
            var senderFactory = await MessagingFactory.CreateAsync(
                messageSenderConfig.NamespaceAddress,
                new MessagingFactorySettings
                {
                    OperationTimeout = messageSenderConfig.OperationTimeout,
                    TransportType = (TransportType)Enum.Parse(typeof(TransportType), messageSenderConfig.TransportType, true),
                    TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(messageSenderConfig.KeyName, messageSenderConfig.SharedAccessKey)
                }).ConfigureAwait(false);
            return await senderFactory.CreateMessageSenderAsync(messageSenderConfig.Path).ConfigureAwait(false);
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

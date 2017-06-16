using MessageProcessing.Driver.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace MessageProcessing.Driver
{
    class Program
    {
        static MessageSender sender;
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            MessageSenderConfig messageSenderConfig = JsonConvert.DeserializeObject<MessageSenderConfig>(ConfigurationManager.AppSettings["messageSenderConfig"]);
            sender = await MessageSender.CreateAsync(messageSenderConfig,new NServiceBusPropertyProvider()).ConfigureAwait(false);
            CreateCart createCart = new CreateCart
            {
                CartId = "cart-id-1",
                CustomerId = "customer-id-1",
                Timestamp = DateTime.UtcNow
            };
            var message = new Message<CreateCart>
            {
                Id = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                //Label = typeof(CreateCart).Name,
                TimeToLive = TimeSpan.FromMinutes(2),
                Body = createCart
            };
            message.Properties["MessageType"] = "CreateCart";
            await sender.SendMessageAsync(message).ConfigureAwait(false);
            await sender.CloseAsync().ConfigureAwait(false);
        }
    }

    public class NServiceBusPropertyProvider : IPropertyProvider
    {
        private const string NsbMessageId = "NServiceBus.MessageId";
        private const string NsbCorrelationId = "NServiceBus.CorrelationId";
        private const string NsbVersion = "NServiceBus.Version";
        private const string NsbMessageIntent = "NServiceBus.MessageIntent";
        private const string NsbOriginatingEndpoint = "NServiceBus.OriginatingEndpoint";
        private const string NsbContentType = "NServiceBus.ContentType";
        private const string NsbEnclosedTypes = "NServiceBus.EnclosedMessageTypes";
        private const string NsbOriginatingMachine = "NServiceBus.OriginatingMachine";
        private const string NsbContentTypeValue = "application/json";
        private const string NsbVersionValue = "4.7.6";
        private KeyValuePair<string, object> messageIntent = new KeyValuePair<string, object>(NsbMessageIntent, "Send");
        private KeyValuePair<string, object> messageVersion = new KeyValuePair<string, object>(NsbVersion, NsbVersionValue);
        private KeyValuePair<string, object> originatingMachine = new KeyValuePair<string, object>(NsbOriginatingMachine, Environment.MachineName);
        private KeyValuePair<string, object> originatingEndpoint = new KeyValuePair<string, object>(NsbOriginatingEndpoint, "");
        private KeyValuePair<string, object> contentType = new KeyValuePair<string, object>(NsbContentType, NsbContentTypeValue);

        public IEnumerable<KeyValuePair<string, object>> GetProperties<T>(Message<T> message)
        {
            yield return new KeyValuePair<string, object>(NsbMessageId, message.Id);
            yield return new KeyValuePair<string, object>(NsbCorrelationId, message.CorrelationId);
            yield return new KeyValuePair<string, object>(NsbEnclosedTypes, message.Body.GetType().AssemblyQualifiedName);
            yield return messageIntent;
            yield return messageVersion;
            yield return originatingMachine;
            yield return originatingEndpoint;
            yield return contentType;
        }
    }
}

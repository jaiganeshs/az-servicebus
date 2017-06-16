using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessing
{
    public interface IPropertyProvider
    {
        IEnumerable<KeyValuePair<string, object>> GetProperties<T>(Message<T> message);
    }
    public class MessageSender
    {
        private Microsoft.ServiceBus.Messaging.MessageSender internalSender;
        private IPropertyProvider[] propertyProviders;
        private IBrokeredMessageBuilder brokeredMessageBuilder;

        internal MessageSender(Microsoft.ServiceBus.Messaging.MessageSender internalSender, IPropertyProvider[] propertyProviders, IBrokeredMessageBuilder brokeredMessageBuilder)
        {
            this.internalSender = internalSender;
            this.propertyProviders = propertyProviders ?? Array.Empty<IPropertyProvider>();
            this.brokeredMessageBuilder = brokeredMessageBuilder;
        }

        public async Task SendMessageAsync<T>(T messagePayload) where T:class
        {
            await SendMessageAsync(new Message<T>() { Body = messagePayload }).ConfigureAwait(false);
        }

        public async Task SendMessageAsync<T>(Message<T> message) where T:class
        {
            var brokeredMessage = this.brokeredMessageBuilder.Build(message);
            brokeredMessage.MessageId = message.Id;
            brokeredMessage.CorrelationId = message.CorrelationId;
            brokeredMessage.Label = message.Label;
            brokeredMessage.TimeToLive = message.TimeToLive;
            foreach (var propertyProvider in propertyProviders)
            {
                var properties = propertyProvider.GetProperties(message);
                SetProperties(brokeredMessage, properties);
            }
            SetProperties(brokeredMessage, message.Properties);
            await internalSender.SendAsync(brokeredMessage).ConfigureAwait(false);
        }

        private void SetProperties(BrokeredMessage brokeredMessage, IEnumerable<KeyValuePair<string, object>> properties)
        {
            foreach (var property in properties)
            {
                brokeredMessage.Properties[property.Key] = property.Value;
            }
        }

        public async Task CloseAsync()
        {
            await internalSender.CloseAsync().ConfigureAwait(false);
        }

        public static async Task<MessageSender> CreateAsync(MessageSenderConfig messageSenderConfig, params IPropertyProvider[] propertyProviders)
        {
            var internalMessageSender = await MessageSenderFactory.CreateMessageSenderAsync(messageSenderConfig).ConfigureAwait(false);
            return new MessageSender(internalMessageSender,propertyProviders, new BrokeredMessageBuilder());
        }
    }

    public class Message<T>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CorrelationId { get; set; }
        public string Label { get; set; } = typeof(T).Name;
        public TimeSpan TimeToLive { get; set; } = TimeSpan.MaxValue;
        public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();
        public T Body { get; set; }
    }

    public interface IBrokeredMessageBuilder
    {
        BrokeredMessage Build<T>(Message<T> message);
    }

    public class BrokeredMessageBuilder : IBrokeredMessageBuilder
    {
        public BrokeredMessage Build<T>(Message<T> message)
        {
            var json = JsonConvert.SerializeObject(message.Body);
            var utf8Bytes = Encoding.UTF8.GetBytes(json);
            var brokeredMessage = new BrokeredMessage(new MemoryStream(utf8Bytes))
            {
                ContentType = "application/json",
            };
            brokeredMessage.Properties["$.PayloadBodyFormat"] = "Stream";
            brokeredMessage.Properties["$.EnclosedTypes"] = typeof(T).AssemblyQualifiedName;
            return brokeredMessage;
        }
    }
}

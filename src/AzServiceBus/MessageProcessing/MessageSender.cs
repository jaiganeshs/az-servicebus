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
    public class MessageSender
    {
        private Microsoft.ServiceBus.Messaging.MessageSender internalSender;

        internal MessageSender(Microsoft.ServiceBus.Messaging.MessageSender internalSender) => this.internalSender = internalSender;

        public async Task SendMessageAsync<T>(Message<T> message)
        {
            var json = JsonConvert.SerializeObject(message.Body);
            var utf8Bytes = Encoding.UTF8.GetBytes(json);
            var brokeredMessage = new BrokeredMessage(new MemoryStream(utf8Bytes))
            {
                ContentType = "application/json",
                MessageId = message.Id,
                CorrelationId = message.CorrelationId,
                Label = message.Label,
                TimeToLive = message.TimeToLive
            };
            brokeredMessage.Properties["$.PayloadBodyFormat"] = "Stream";
            brokeredMessage.Properties["$.EnclosedTypes"] = typeof(T).AssemblyQualifiedName;
            foreach (var property in message.Properties)
            {
                brokeredMessage.Properties[property.Key] = property.Value;
            }
            await internalSender.SendAsync(brokeredMessage).ConfigureAwait(false);
        }

        public async Task CloseAsync()
        {
            await internalSender.CloseAsync().ConfigureAwait(false);
        }

        public static async Task<MessageSender> CreateAsync(MessageSenderConfig messageSenderConfig)
        {
            var internalMessageSender = await MessageSenderFactory.CreateMessageSenderAsync(messageSenderConfig).ConfigureAwait(false);
            return new MessageSender(internalMessageSender);
        }
    }

    public class Message<T>
    {
        public string Id { get; set; }
        public string CorrelationId { get; set; }
        public string Label { get; set; } = typeof(T).Name;
        public TimeSpan TimeToLive { get; set; } = TimeSpan.MaxValue;
        public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();
        public T Body { get; set; }
    }
}

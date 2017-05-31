using MessageProcessing.Driver.Messages;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
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
        public IEnumerable<KeyValuePair<string, object>> GetProperties<T>(Message<T> message)
        {
            var properties = new Dictionary<string, object>();
            properties["NServiceBus.MessageId"] = message.Id;
            properties["NServiceBus.CorrelationId"] = message.CorrelationId;
            properties["NServiceBus.Version"] = "4.7.6";
            properties["NServiceBus.MessageIntent"] = "Send";
            properties["NServiceBus.OriginatingEndpoint"] = "";
            properties["NServiceBus.ContentType"] = "application/json";
            properties["NServiceBus.EnclosedMessageTypes"] = message.Body.GetType().AssemblyQualifiedName;
            properties["NServiceBus.OriginatingMachine"] = Environment.MachineName;
            return properties;
        }
    }
}

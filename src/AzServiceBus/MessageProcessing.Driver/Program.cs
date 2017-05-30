using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessing.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            MessageSenderConfig messageSenderConfig = JsonConvert.DeserializeObject<MessageSenderConfig>(ConfigurationManager.AppSettings["messageSenderConfig"]);
            var sender = await MessageSenderFactory.CreateMessageSenderAsync(messageSenderConfig).ConfigureAwait(false);
            await sender.SendAsync(new BrokeredMessage()).ConfigureAwait(false);
        }
    }
}

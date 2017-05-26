using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessing.Driver.Messages
{
    public class CreateCart
    {
        public string CustomerId { get; set; }
        public string CartId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

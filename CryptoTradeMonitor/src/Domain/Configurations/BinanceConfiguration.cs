using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Configurations
{
    public class BinanceConfiguration
    {
        public string WebSocketUri { get; set; }
        public List<HttpClientConfiguration> HttpClients { get; set; }
    }    
}

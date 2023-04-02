using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ExchangeInfo
    {
        public List<SymbolInfo> Symbols { get; set; }

        public ExchangeInfo(List<SymbolInfo> symbols)
        {
            Symbols = symbols;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALPHA_QUOTE.Models
{
    public record Stock(string Symbol, List<StockValues> Values);
}

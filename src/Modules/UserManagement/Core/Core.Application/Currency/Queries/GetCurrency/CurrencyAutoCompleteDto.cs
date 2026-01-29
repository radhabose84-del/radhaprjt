using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Currency.Queries.GetCurrency
{
    public class CurrencyAutoCompleteDto
    {
        
        public int Id { get; set; }
        public string? Code { get; set; }
    }
}
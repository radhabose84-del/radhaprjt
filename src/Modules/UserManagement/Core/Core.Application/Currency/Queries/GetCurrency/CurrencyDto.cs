using Core.Application.Common.HttpResponse;
using Core.Application.Common.Mappings;
using Core.Domain.Enums;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Currency.Queries.GetCurrency
{
    public class CurrencyDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Status IsActive { get; set; }
        
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
  
    }
}
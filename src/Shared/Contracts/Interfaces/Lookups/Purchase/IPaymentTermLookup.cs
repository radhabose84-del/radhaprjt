#nullable disable
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Purchase;

namespace Contracts.Interfaces.Lookups.Purchase
{
    public interface IPaymentTermLookup
    {
        Task<List<PaymentTermLookupDto>> GetAllPaymentTermAsync();
    }
}

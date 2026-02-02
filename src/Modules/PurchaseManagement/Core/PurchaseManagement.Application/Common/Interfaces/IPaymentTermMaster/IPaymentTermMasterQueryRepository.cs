using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermAutoComplete;

namespace PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster
{
    public interface IPaymentTermMasterQueryRepository
    {
        Task<(List<PaymentTermMasterDto>, int)> GetAllPaymentTermMasterAsync(int PageNumber, int PageSize, string? SearchTerm);

        Task<PaymentTermMasterDto> GetByIdAsync(int id);

        Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);

        Task<bool> ExistsByIdAsync(int id);

        Task<List<AutoCompleteDto>> GetPaymentTermAutoComplete(string? searchPattern, string? paymentTermCode);       

    }
}
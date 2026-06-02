using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterAutoComplete
{
    public class TnCTemplateAutoCompleteQuery : IRequest<List<TnCAutoCompleteDto>>
    {

        public int? ModuleId { get; set; }            // AppData.Modules
        public int? TransactionTypeId { get; set; }   // Finance.TransactionTypeMaster
        public string? SearchPattern { get; set; }
        
    }
}
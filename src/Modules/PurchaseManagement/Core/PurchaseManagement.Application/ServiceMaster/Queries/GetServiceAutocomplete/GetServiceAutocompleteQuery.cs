using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Queries.GetServiceAutocomplete
{
    public class GetServiceAutocompleteQuery : IRequest< List<ServiceMasterAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}
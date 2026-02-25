using MediatR;

namespace InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterAutoComplete
{
    public class GetHSNMasterAutoCompleteQuery : IRequest<List<GetHSNMasterAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }     
        public string? TypeCode { get; set; }   
        
    }
}
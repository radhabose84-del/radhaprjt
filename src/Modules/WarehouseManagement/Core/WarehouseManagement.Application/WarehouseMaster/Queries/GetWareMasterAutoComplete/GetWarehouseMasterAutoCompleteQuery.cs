using MediatR;

namespace WarehouseManagement.Application.WarehouseMaster.Queries.GetWareMasterAutoComplete
{
    public class GetWarehouseMasterAutoCompleteQuery : IRequest<List<GetWarehouseAutoCompleteDto>>
    {
        
        public string?    SearchPattern { get; set; }
    }
}
using MediatR;

namespace WarehouseManagement.Application.RackMaster.Queries.GetRackMasterAutoComplete
{
    public class GetRackMasterAutoCompleteQuery : IRequest<List<GetRackMasterAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }

        public int? WarehouseId { get; set; } = null;


       
    }
}
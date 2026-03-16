using MediatR;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetWareMasterAutoComplete;

namespace WarehouseManagement.Application.WarehouseMaster.Queries.GetWarehouseByUnitId
{
    public class GetWarehouseByUnitIdQuery : IRequest<List<GetWarehouseAutoCompleteDto>>
    {
        public int UnitId { get; set; }
    }
}

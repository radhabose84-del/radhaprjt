using MediatR;

namespace WarehouseManagement.Application.BinMaster.Queries.GetBinMasterAutoComplete
{
    public class GetBinMasterAutoComplete : IRequest<List<BinAutoDto>>
    {
        public string? SearchPattern { get; set; }

        public int Top { get; set; } = 10;
        public int? WarehouseId { get; set; }
        public int? RackId { get; set; }

    }
    
     
}
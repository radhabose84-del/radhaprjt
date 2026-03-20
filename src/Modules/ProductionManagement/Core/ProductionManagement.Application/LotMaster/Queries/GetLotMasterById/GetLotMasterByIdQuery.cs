using MediatR;
using ProductionManagement.Application.LotMaster.Dto;

namespace ProductionManagement.Application.LotMaster.Queries.GetLotMasterById
{
    public class GetLotMasterByIdQuery : IRequest<LotMasterDto?>
    {
        public int Id { get; set; }
    }
}

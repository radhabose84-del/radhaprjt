using MediatR;
using SalesManagement.Application.LotMaster.Dto;

namespace SalesManagement.Application.LotMaster.Queries.GetLotMasterById
{
    public class GetLotMasterByIdQuery : IRequest<LotMasterDto?>
    {
        public int Id { get; set; }
    }
}

using MediatR;
using LogisticsManagement.Application.FreightMaster.Dto;

namespace LogisticsManagement.Application.FreightMaster.Queries.GetFreightMasterById
{
    public class GetFreightMasterByIdQuery : IRequest<FreightMasterDto?>
    {
        public int Id { get; set; }
    }
}

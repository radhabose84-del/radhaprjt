using MediatR;
using ProductionManagement.Application.RepackingMaster.Dto;

namespace ProductionManagement.Application.RepackingMaster.Queries.GetRepackingMasterById
{
    public class GetRepackingMasterByIdQuery : IRequest<RepackingMasterDto?>
    {
        public int Id { get; set; }
    }
}

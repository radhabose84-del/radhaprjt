using MediatR;
using ProductionManagement.Application.ProcessMaster.Dto;

namespace ProductionManagement.Application.ProcessMaster.Queries.GetProcessMasterById
{
    public class GetProcessMasterByIdQuery : IRequest<ProcessMasterDto>
    {
        public int Id { get; set; }
    }
}

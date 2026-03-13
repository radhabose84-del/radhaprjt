using MediatR;
using ProductionManagement.Application.CountMaster.Dto;

namespace ProductionManagement.Application.CountMaster.Queries.GetCountMasterById
{
    public class GetCountMasterByIdQuery : IRequest<CountMasterDto>
    {
        public int Id { get; set; }
    }
}

using MediatR;
using ProductionManagement.Application.MiscTypeMaster.Dto;

namespace ProductionManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery : IRequest<MiscTypeMasterDto>
    {
        public int Id { get; set; }
    }
}

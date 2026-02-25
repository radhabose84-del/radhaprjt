using MediatR;
using SalesManagement.Application.MiscTypeMaster.Dto;

namespace SalesManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery : IRequest<MiscTypeMasterDto>
    {
        public int Id { get; set; }
    }
}

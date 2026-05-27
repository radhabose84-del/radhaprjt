using MediatR;
using QCManagement.Application.MiscTypeMaster.Dto;

namespace QCManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery : IRequest<MiscTypeMasterDto?>
    {
        public int Id { get; set; }
    }
}

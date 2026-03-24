using GateEntryManagement.Application.MiscTypeMaster.Dto;
using MediatR;

namespace GateEntryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery : IRequest<MiscTypeMasterDto?>
    {
        public int Id { get; set; }
    }
}

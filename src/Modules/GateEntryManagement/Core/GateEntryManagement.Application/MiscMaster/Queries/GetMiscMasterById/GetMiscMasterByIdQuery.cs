using GateEntryManagement.Application.MiscMaster.Dto;
using MediatR;

namespace GateEntryManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery : IRequest<MiscMasterDto?>
    {
        public int Id { get; set; }
    }
}

using GateEntryManagement.Application.GateInward.Dto;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Queries.GetGateInwardById
{
    public class GetGateInwardByIdQuery : IRequest<GateInwardHdrDto?>
    {
        public int Id { get; set; }
    }
}

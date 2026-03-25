using GateEntryManagement.Application.GatePass.Dto;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Queries.GetGatePassById
{
    public class GetGatePassByIdQuery : IRequest<GatePassHdrDto?>
    {
        public int Id { get; set; }
    }
}

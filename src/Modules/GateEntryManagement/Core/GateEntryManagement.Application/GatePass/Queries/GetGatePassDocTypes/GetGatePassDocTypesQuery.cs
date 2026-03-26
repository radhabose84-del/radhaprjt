using Contracts.Dtos.Lookups.Finance;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Queries.GetGatePassDocTypes
{
    public class GetGatePassDocTypesQuery : IRequest<List<TransactionTypeLookupDto>>
    {
        public int? ModuleId { get; set; }
        public int? MenuId { get; set; }
    }
}

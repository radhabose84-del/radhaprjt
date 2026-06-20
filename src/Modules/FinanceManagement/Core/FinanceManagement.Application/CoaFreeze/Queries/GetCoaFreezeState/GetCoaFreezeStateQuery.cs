using FinanceManagement.Application.CoaFreeze.Dto;
using MediatR;

namespace FinanceManagement.Application.CoaFreeze.Queries.GetCoaFreezeState
{
    // Freeze banner + status for the COA console (US-GL02-FR-008a). Company from token.
    public sealed record GetCoaFreezeStateQuery : IRequest<CoaFreezeStateDto>;
}

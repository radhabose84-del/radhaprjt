using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.CoaFreeze.Commands.SetCoaFreezeState
{
    // TEST/ADMIN hook for 08a so the freeze engine is exercisable before US-GL02-08B's governed
    // dual-approval freeze/unfreeze exists. IsFrozen=true seals; IsFrozen=false opens an unfreeze
    // window (UnfreezeWindowMinutes, default 60) which the auto-re-freeze job later re-seals.
    public class SetCoaFreezeStateCommand : IRequest<ApiResponseDTO<bool>>
    {
        public bool IsFrozen { get; set; }
        public int? UnfreezeWindowMinutes { get; set; }
    }
}

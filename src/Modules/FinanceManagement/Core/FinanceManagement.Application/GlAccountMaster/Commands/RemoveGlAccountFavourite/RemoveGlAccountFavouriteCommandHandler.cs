using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.RemoveGlAccountFavourite
{
    public class RemoveGlAccountFavouriteCommandHandler : IRequestHandler<RemoveGlAccountFavouriteCommand, ApiResponseDTO<bool>>
    {
        private readonly IGlAccountUserPrefStore _prefStore;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public RemoveGlAccountFavouriteCommandHandler(
            IGlAccountUserPrefStore prefStore,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _prefStore = prefStore;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<bool>> Handle(RemoveGlAccountFavouriteCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var userId = _ipAddressService.GetUserId();

            await _prefStore.RemoveFavouriteAsync(userId, companyId, request.GlAccountMasterId, cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "ACCOUNT_FAVOURITE_REMOVE",
                actionName: request.GlAccountMasterId.ToString(),
                details: $"GL account {request.GlAccountMasterId} removed from favourites for user {userId}.",
                module: "GlAccountMaster"), cancellationToken);

            return new ApiResponseDTO<bool> { IsSuccess = true, Message = "Account removed from favourites.", Data = true };
        }
    }
}

using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.AddGlAccountFavourite
{
    public class AddGlAccountFavouriteCommandHandler : IRequestHandler<AddGlAccountFavouriteCommand, ApiResponseDTO<bool>>
    {
        private readonly IGlAccountUserPrefStore _prefStore;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public AddGlAccountFavouriteCommandHandler(
            IGlAccountUserPrefStore prefStore,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _prefStore = prefStore;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<bool>> Handle(AddGlAccountFavouriteCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var userId = _ipAddressService.GetUserId();

            await _prefStore.AddFavouriteAsync(userId, companyId, request.GlAccountMasterId, cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "ACCOUNT_FAVOURITE_ADD",
                actionName: request.GlAccountMasterId.ToString(),
                details: $"GL account {request.GlAccountMasterId} added to favourites for user {userId}.",
                module: "GlAccountMaster"), cancellationToken);

            return new ApiResponseDTO<bool> { IsSuccess = true, Message = "Account added to favourites.", Data = true };
        }
    }
}

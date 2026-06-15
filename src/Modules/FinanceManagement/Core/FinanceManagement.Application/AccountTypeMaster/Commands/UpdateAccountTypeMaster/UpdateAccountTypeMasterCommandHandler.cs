using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountTypeMaster.Commands.UpdateAccountTypeMaster
{
    public class UpdateAccountTypeMasterCommandHandler : IRequestHandler<UpdateAccountTypeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IAccountTypeMasterCommandRepository _commandRepository;
        private readonly IAccountTypeMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateAccountTypeMasterCommandHandler(
            IAccountTypeMasterCommandRepository commandRepository,
            IAccountTypeMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateAccountTypeMasterCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsAccountTypeLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.AccountTypeMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "ACCOUNT_TYPE_MASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Account Type Master with Id {request.Id} updated successfully.",
                module: "AccountTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Account Type Master updated successfully.",
                Data = updatedId
            };
        }
    }
}

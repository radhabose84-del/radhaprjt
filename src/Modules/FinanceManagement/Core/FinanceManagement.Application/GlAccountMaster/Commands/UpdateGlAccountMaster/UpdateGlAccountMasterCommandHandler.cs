using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.UpdateGlAccountMaster
{
    public class UpdateGlAccountMasterCommandHandler : IRequestHandler<UpdateGlAccountMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IGlAccountMasterCommandRepository _commandRepository;
        private readonly IGlAccountMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateGlAccountMasterCommandHandler(
            IGlAccountMasterCommandRepository commandRepository,
            IGlAccountMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateGlAccountMasterCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsGlAccountLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This GL Account is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.GlAccountMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "GL_ACCOUNT_MASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"GL Account with Id {request.Id} updated successfully.",
                module: "GlAccountMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "GL Account updated successfully.",
                Data = updatedId
            };
        }
    }
}

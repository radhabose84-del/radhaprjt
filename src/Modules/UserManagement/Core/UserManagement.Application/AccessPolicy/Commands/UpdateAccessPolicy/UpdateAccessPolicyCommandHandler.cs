using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;

namespace UserManagement.Application.AccessPolicy.Commands.UpdateAccessPolicy
{
    public class UpdateAccessPolicyCommandHandler : IRequestHandler<UpdateAccessPolicyCommand, ApiResponseDTO<int>>
    {
        private readonly IAccessPolicyCommandRepository _commandRepository;
        private readonly IAccessPolicyQueryRepository   _queryRepository;
        private readonly IMediator                      _mediator;
        private readonly IMapper                        _mapper;

        public UpdateAccessPolicyCommandHandler(
            IAccessPolicyCommandRepository commandRepository,
            IAccessPolicyQueryRepository   queryRepository,
            IMediator                      mediator,
            IMapper                        mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository   = queryRepository;
            _mediator          = mediator;
            _mapper            = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            UpdateAccessPolicyCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.AccessPolicy>(request);
            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode:   "ACCESSPOLICY_UPDATE",
                actionName:   request.Id.ToString(),
                details:      $"AccessPolicy with Id {request.Id} updated successfully.",
                module:       "AccessPolicy"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message   = "Access Policy updated successfully.",
                Data      = result
            };
        }
    }
}

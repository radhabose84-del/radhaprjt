using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.CommissionSplit.Commands.UpdateCommissionSplit
{
    public class UpdateCommissionSplitCommandHandler : IRequestHandler<UpdateCommissionSplitCommand, ApiResponseDTO<int>>
    {
        private readonly ICommissionSplitCommandRepository _commandRepository;
        private readonly ICommissionSplitQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateCommissionSplitCommandHandler(
            ICommissionSplitCommandRepository commandRepository,
            ICommissionSplitQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateCommissionSplitCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.CommissionSplit>(request);

            // Map child collection (replace strategy — old children removed in repository)
            if (request.Details != null && request.Details.Count > 0)
            {
                entity.CommissionSplitDetails = request.Details.Select(d => new CommissionSplitDetail
                {
                    RoleId = d.RoleId,
                    ShareTypeId = d.ShareTypeId,
                    ShareValue = d.ShareValue,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "COMMISSION_SPLIT_UPDATE",
                actionName: request.Id.ToString(),
                details: $"CommissionSplit with Id {request.Id} updated successfully.",
                module: "CommissionSplit"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "CommissionSplit updated successfully.",
                Data = result
            };
        }
    }
}

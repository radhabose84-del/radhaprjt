using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.CommissionSplit.Commands.CreateCommissionSplit
{
    public class CreateCommissionSplitCommandHandler : IRequestHandler<CreateCommissionSplitCommand, ApiResponseDTO<int>>
    {
        private readonly ICommissionSplitCommandRepository _commandRepository;
        private readonly ICommissionSplitQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateCommissionSplitCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreateCommissionSplitCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.CommissionSplit>(request);

            // Map child collection
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

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COMMISSION_SPLIT_CREATE",
                actionName: entity.SplitCode ?? string.Empty,
                details: $"CommissionSplit '{entity.SplitCode}' created successfully with Id {newId}.",
                module: "CommissionSplit"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "CommissionSplit created successfully.",
                Data = newId
            };
        }
    }
}

using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.MarketingOfficer.Commands.UpdateMarketingOfficer
{
    public class UpdateMarketingOfficerCommandHandler : IRequestHandler<UpdateMarketingOfficerCommand, ApiResponseDTO<int>>
    {
        private readonly IMarketingOfficerCommandRepository _commandRepository;
        private readonly IMarketingOfficerQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateMarketingOfficerCommandHandler(
            IMarketingOfficerCommandRepository commandRepository,
            IMarketingOfficerQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateMarketingOfficerCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsMarketingOfficerLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.MarketingOfficer>(request);

            entity.OfficerSalesGroups = request.SalesGroups
                .Select(sg => new Domain.Entities.OfficerSalesGroup
                {
                    SalesGroupId = sg.SalesGroupId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "MARKETING_OFFICER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Marketing Officer with Id {request.Id} updated successfully.",
                module: "MarketingOfficer"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Marketing Officer updated successfully.",
                Data = result
            };
        }
    }
}

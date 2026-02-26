using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.MarketingOfficer.Commands.CreateMarketingOfficer
{
    public class CreateMarketingOfficerCommandHandler : IRequestHandler<CreateMarketingOfficerCommand, ApiResponseDTO<int>>
    {
        private readonly IMarketingOfficerCommandRepository _commandRepository;
        private readonly IMarketingOfficerQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateMarketingOfficerCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreateMarketingOfficerCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.MarketingOfficer>(request);

            entity.OfficerSalesGroups = request.SalesGroups
                .Select(sg => new Domain.Entities.OfficerSalesGroup
                {
                    SalesGroupId = sg.SalesGroupId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "MARKETING_OFFICER_CREATE",
                actionName: request.EmployeeNo,
                details: $"Marketing Officer '{request.EmployeeNo}' created successfully with Id {newId}.",
                module: "MarketingOfficer"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Marketing Officer created successfully.",
                Data = newId
            };
        }
    }
}

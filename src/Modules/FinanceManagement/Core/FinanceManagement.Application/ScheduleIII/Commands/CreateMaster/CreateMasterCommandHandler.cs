using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateMaster
{
    public class CreateMasterCommandHandler : IRequestHandler<CreateMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public CreateMasterCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IScheduleIIIQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateMasterCommand request, CancellationToken cancellationToken)
        {
            // CompanyId + DivisionId come from the token, never the payload.
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var divisionId = _ipAddressService.GetDivisionId()
                ?? throw new ExceptionRules("No active division in session.");

            // Ensure the structure header exists (DRAFT on first line), then add the line.
            var headerId = await _commandRepository.EnsureHeaderAsync(companyId, divisionId);

            var detail = _mapper.Map<Domain.Entities.ScheduleIIIDetail>(request);
            detail.ScheduleIIIHeaderId = headerId;

            var newId = await _commandRepository.CreateDetailAsync(detail);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "S3_DETAIL_CREATE",
                actionName: newId.ToString(),
                details: $"Schedule III line added successfully with Id {newId}.",
                module: "ScheduleIIIDetail"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Schedule III line added successfully.",
                Data = newId
            };
        }
    }
}

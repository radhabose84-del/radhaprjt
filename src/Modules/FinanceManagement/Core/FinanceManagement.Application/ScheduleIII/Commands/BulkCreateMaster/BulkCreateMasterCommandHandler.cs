using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.ScheduleIII.Commands.BulkCreateMaster
{
    public class BulkCreateMasterCommandHandler : IRequestHandler<BulkCreateMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public BulkCreateMasterCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IMediator mediator,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(BulkCreateMasterCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var divisionId = _ipAddressService.GetDivisionId()
                ?? throw new ExceptionRules("No active division in session.");

            var headerId = await _commandRepository.EnsureHeaderAsync(companyId, divisionId);

            var details = request.Lines.Select(l => new ScheduleIIIDetail
            {
                ScheduleIIIHeaderId = headerId,
                ScheduleIIISectionId = l.ScheduleIIISectionId,
                ScheduleIIISectionItemId = l.ScheduleIIISectionItemId,
                DisplayOrder = l.DisplayOrder,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            }).ToList();

            var count = await _commandRepository.CreateDetailRangeAsync(details);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "S3_DETAIL_BULK_CREATE",
                actionName: count.ToString(),
                details: $"Schedule III bulk add — {count} line(s) added to the structure.",
                module: "ScheduleIIIDetail"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = $"{count} line(s) added successfully.",
                Data = count
            };
        }
    }
}

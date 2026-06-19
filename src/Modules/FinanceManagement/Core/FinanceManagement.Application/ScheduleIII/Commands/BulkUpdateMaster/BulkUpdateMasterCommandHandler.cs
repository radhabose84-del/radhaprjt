using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.ScheduleIII.Commands.BulkUpdateMaster
{
    public class BulkUpdateMasterCommandHandler : IRequestHandler<BulkUpdateMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public BulkUpdateMasterCommandHandler(IScheduleIIICommandRepository commandRepository, IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(BulkUpdateMasterCommand request, CancellationToken cancellationToken)
        {
            var details = request.Lines.Select(l => new ScheduleIIIDetail
            {
                Id = l.Id,
                ScheduleIIISectionId = l.ScheduleIIISectionId,
                ScheduleIIISectionItemId = l.ScheduleIIISectionItemId,
                DisplayOrder = l.DisplayOrder,
                IsActive = l.IsActive == 1 ? Status.Active : Status.Inactive
            }).ToList();

            var count = await _commandRepository.UpdateDetailRangeAsync(details);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "S3_DETAIL_BULK_UPDATE",
                actionName: count.ToString(),
                details: $"Schedule III bulk update — {count} line(s) updated.",
                module: "ScheduleIIIDetail"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = $"{count} line(s) updated successfully.",
                Data = count
            };
        }
    }
}

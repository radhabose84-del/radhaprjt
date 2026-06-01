using AutoMapper;
using MediatR;
using PurchaseManagement.Application.BlanketMaster.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.BlanketMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BlanketMaster.Commands.Update;

public class UpdateBlanketMasterCommandHandler : IRequestHandler<UpdateBlanketMasterCommand, BlanketHeaderDto>
{
    private readonly IBlanketMasterCommandRepository _commandRepo;
    private readonly IBlanketMasterQueryRepository _queryRepo;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public UpdateBlanketMasterCommandHandler(
        IBlanketMasterCommandRepository commandRepo,
        IBlanketMasterQueryRepository queryRepo,
        IMediator mediator,
        IMapper mapper)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<BlanketHeaderDto> Handle(UpdateBlanketMasterCommand request, CancellationToken cancellationToken)
    {
        var header = _mapper.Map<BlanketHeader>(request);

        // Build details with calculated values
        var details = new List<BlanketDetail>();
        foreach (var item in request.Details)
        {
            var detail = new BlanketDetail
            {
                Id = item.Id,
                ItemSno = item.ItemSno,
                ItemId = item.ItemId,
                UOMId = item.UOMId,
                EstimatedQuantity = item.EstimatedQuantity,
                Rate = item.Rate,
                TotalPrice = item.EstimatedQuantity * item.Rate,
                HSNId = item.HSNId,
                GSTPercentage = item.GSTPercentage,
                QualitySpecification = item.QualitySpecification
            };

            foreach (var sched in item.Schedules)
            {
                detail.Schedules.Add(new BlanketSchedule
                {
                    Id = sched.Id,
                    ScheduleNo = sched.ScheduleNo,
                    ScheduleDate = sched.ScheduleDate,
                    ScheduleQuantity = sched.ScheduleQuantity,
                    Remarks = sched.Remarks
                });
            }

            details.Add(detail);
        }

        var updated = await _commandRepo.UpdateAsync(header, details, cancellationToken);

        var result = await _queryRepo.GetByIdAsync(updated.Id, cancellationToken);

        // Audit
        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: "BLANKET_MASTER_UPDATE",
            actionName: request.Id.ToString(),
            details: $"Blanket Master with Id {request.Id} updated successfully.",
            module: "BlanketMaster"
        );
        await _mediator.Publish(auditEvent, cancellationToken);

        return result!;
    }
}

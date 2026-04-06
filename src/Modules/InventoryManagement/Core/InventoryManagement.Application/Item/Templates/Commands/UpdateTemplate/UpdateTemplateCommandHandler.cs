using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.Commands.UpdateTemplate;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.item.ItemDetail.Templates;
using InventoryManagement.Domain.Events;
using MediatR;

public sealed class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, bool>
{
    private readonly ITemplateCommandRepository _cmd;
    private readonly ITemplateQueryRepository _qry;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public UpdateTemplateCommandHandler(
        ITemplateCommandRepository cmd,
        ITemplateQueryRepository qry,
        IMediator mediator,
        IMapper mapper)
    {
        _cmd = cmd; _qry = qry; _mediator = mediator; _mapper = mapper;
    }

    public async Task<bool> Handle(UpdateTemplateCommand request, CancellationToken ct)
    {
        if (request.IsActive == 0)
        {
            var isLinked = await _qry.IsTemplateLinkedAsync(request.Id, ct);
            if (isLinked)
                throw new ExceptionRules(
                    "This master is linked with other records. You cannot inactivate this record.");
        }

        var entity = await _qry.GetByIdAsync(request.Id, ct);
        if (entity is null) return false;

        // Header updates
        entity.TemplateName = request.TemplateName.Trim();
        var status = request.IsActive == 1
            ? BaseEntity.Status.Active
            : BaseEntity.Status.Inactive;

        entity.IsActive = status;

        // Build incoming parameters graph
        var newParams = new List<InspectionParameter>();
        foreach (var p in request.Parameters ?? new())
        {
            newParams.Add(new InspectionParameter
            {
                Id = p.Id ?? 0,
                TemplateId = entity.Id,
                Parameter = p.Parameter.Trim(),
                AcceptanceCriteriaValue = p.AcceptanceCriteriaValue?.Trim(),
                Numeric = p.Numeric,
                MinimumValue = p.MinimumValue,
                MaximumValue = p.MaximumValue
            });
        }

        // ✅ Atomic transaction under execution strategy
        await _cmd.UpdateWithParametersAsync(entity, newParams, ct);

        await _mediator.Publish(new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: "INSPTPL_UPDATE",
            actionName: entity.TemplateName,
            details: $"Inspection Template '{entity.TemplateName}' (Id {entity.Id}) updated.",
            module: "InspectionTemplate"
        ), ct);

        return true;
    }
}

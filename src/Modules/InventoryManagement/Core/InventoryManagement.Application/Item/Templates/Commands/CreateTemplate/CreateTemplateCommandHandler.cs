// Create handler
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.item.ItemDetail.Templates;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.Templates.Commands.CreateTemplate
{
    public sealed class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, int>
    {
        private readonly ITemplateCommandRepository _cmd;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateTemplateCommandHandler(ITemplateCommandRepository cmd, IMediator mediator, IMapper mapper)
        { _cmd = cmd; _mediator = mediator; _mapper = mapper; }

        public async Task<int> Handle(CreateTemplateCommand request, CancellationToken ct)
        {
            var entity = new InspectionTemplate
            {
                TemplateName = request.TemplateName.Trim(),
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

            foreach (var p in request.Parameters ?? new())
            {
                entity.Parameters.Add(new InspectionParameter
                {
                    Parameter = p.Parameter.Trim(),
                    AcceptanceCriteriaValue = p.AcceptanceCriteriaValue?.Trim(),
                    Numeric = p.Numeric,
                    MinimumValue = p.MinimumValue,
                    MaximumValue = p.MaximumValue
                });
            }

            var id = await _cmd.CreateAsync(entity, ct);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "INSPTPL_CREATE",
                actionName: entity.TemplateName,
                details: $"Inspection Template '{entity.TemplateName}' created with Id {id}.",
                module: "InspectionTemplate"
            ), ct);

            return id;
        }
    }
}

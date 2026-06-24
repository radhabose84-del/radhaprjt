using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.CreateRecurringJournalTemplate
{
    public class CreateRecurringJournalTemplateCommandHandler : IRequestHandler<CreateRecurringJournalTemplateCommand, ApiResponseDTO<int>>
    {
        private readonly IRecurringJournalTemplateCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateRecurringJournalTemplateCommandHandler(
            IRecurringJournalTemplateCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateRecurringJournalTemplateCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>(request);
            entity.Lines = BuildLines(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "RECURRING_TEMPLATE_CREATE",
                actionName: request.TemplateName ?? string.Empty,
                details: $"Recurring journal template '{request.TemplateName}' created with Id {newId}.",
                module: "RecurringJournalTemplate"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Recurring journal template created successfully.",
                Data = newId
            };
        }

        private static List<FinanceManagement.Domain.Entities.RecurringJournalTemplateDetail> BuildLines(CreateRecurringJournalTemplateCommand request)
        {
            var lineNo = 0;
            return request.Lines.Select(l => new FinanceManagement.Domain.Entities.RecurringJournalTemplateDetail
            {
                LineNo = ++lineNo,
                GlAccountId = l.GlAccountId,
                DrAmount = l.DrAmount,
                CrAmount = l.CrAmount,
                AmountFormula = l.AmountFormula,
                CostCentreId = l.CostCentreId,
                ProfitCentreId = l.ProfitCentreId,
                LineNarration = l.LineNarration
            }).ToList();
        }
    }
}

using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.UpdateRecurringJournalTemplate;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.JournalMaster.RecurringJournalTemplate
{
    public class UpdateRecurringJournalTemplateCommandValidator : AbstractValidator<UpdateRecurringJournalTemplateCommand>
    {
        private readonly IRecurringJournalTemplateQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public UpdateRecurringJournalTemplateCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IRecurringJournalTemplateQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

            var maxName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>("TemplateName") ?? 150;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Id is required.")
                .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                .WithMessage("Recurring journal template not found.")
                .When(x => x.Id > 0);

            RuleFor(x => x.TemplateName)
                .NotNull().WithMessage("Template Name is required.")
                .NotEmpty().WithMessage("Template Name is required.")
                .MaximumLength(maxName).WithMessage($"Template Name cannot be longer than {maxName} characters.");

            RuleFor(x => x.TemplateName)
                .MustAsync(async (cmd, name, ct) => !await _queryRepository.AlreadyExistsByNameAsync(name!, cmd.Id))
                .WithMessage("Template Name already exists.")
                .When(x => !string.IsNullOrWhiteSpace(x.TemplateName));

            RuleFor(x => x.VoucherTypeId).GreaterThan(0).WithMessage("Voucher type is required.");
            RuleFor(x => x.VoucherTypeId)
                .MustAsync(async (id, ct) => await _queryRepository.VoucherTypeExistsAsync(id, CompanyId()))
                .WithMessage("Voucher type does not exist.")
                .When(x => x.VoucherTypeId > 0);

            RuleFor(x => x.FrequencyId).GreaterThan(0).WithMessage("Frequency is required.");
            RuleFor(x => x.FrequencyId)
                .MustAsync(async (id, ct) => await _queryRepository.FrequencyExistsAsync(id))
                .WithMessage("Frequency is invalid.")
                .When(x => x.FrequencyId > 0);

            RuleFor(x => x.AmountAdjustmentRuleId).GreaterThan(0).WithMessage("Amount adjustment rule is required.");
            RuleFor(x => x.AmountAdjustmentRuleId)
                .MustAsync(async (id, ct) => await _queryRepository.AmountAdjustmentRuleExistsAsync(id))
                .WithMessage("Amount adjustment rule is invalid.")
                .When(x => x.AmountAdjustmentRuleId > 0);

            RuleFor(x => x.IsActive)
                .InclusiveBetween(0, 1).WithMessage("IsActive must be either 0 or 1.");

            RuleFor(x => x.EndDate!.Value)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End Date must be greater than or equal to Start Date.")
                .When(x => x.EndDate.HasValue);

            RuleFor(x => x.Lines)
                .NotEmpty().WithMessage("At least one template line is required.");

            When(x => x.Lines is { Count: > 0 }, () =>
            {
                RuleForEach(x => x.Lines).ChildRules(line =>
                {
                    line.RuleFor(l => l.GlAccountId).GreaterThan(0).WithMessage("Line GL account is required.");
                });

                RuleFor(x => x).MustAsync(AllAccountsExistAsync)
                    .WithMessage("One or more GL accounts do not exist or are inactive.");
            });
        }

        private int CompanyId() => _ipAddressService.GetCompanyId() ?? 0;

        private async Task<bool> AllAccountsExistAsync(UpdateRecurringJournalTemplateCommand command, CancellationToken ct)
        {
            var company = CompanyId();
            foreach (var accId in command.Lines.Select(l => l.GlAccountId).Distinct().Where(i => i > 0))
            {
                if (!await _queryRepository.GlAccountExistsAsync(accId, company))
                    return false;
            }
            return true;
        }
    }
}

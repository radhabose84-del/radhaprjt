using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.UpdateJournal;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.JournalMaster.Journal
{
    public class UpdateJournalCommandValidator : AbstractValidator<UpdateJournalCommand>
    {
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public UpdateJournalCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IJournalQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

            var maxNarration = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.JournalHeader>("Narration") ?? 500;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Id is required.")
                .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                .WithMessage("Journal voucher not found.")
                .MustAsync(async (id, ct) => await _queryRepository.IsDraftAsync(id))
                .WithMessage("Only draft journals can be edited.")
                .When(x => x.Id > 0);

            RuleFor(x => x.VoucherTypeId)
                .GreaterThan(0).WithMessage("Voucher type is required.");

            RuleFor(x => x.VoucherTypeId)
                .MustAsync(async (id, ct) => await _queryRepository.VoucherTypeExistsAsync(id, CompanyId()))
                .WithMessage("Voucher type does not exist.")
                .When(x => x.VoucherTypeId > 0);

            RuleFor(x => x.Narration)
                .MaximumLength(maxNarration)
                .When(x => !string.IsNullOrEmpty(x.Narration));

            RuleFor(x => x.Lines)
                .NotEmpty().WithMessage("At least one journal line is required.");

            RuleFor(x => x)
                .MustAsync(async (x, ct) => await _queryRepository.GetOpenPeriodByDateAsync(CompanyId(), x.VoucherDate) != null)
                .WithMessage("Voucher date must fall within an open accounting period.");

            When(x => x.Lines is { Count: > 0 }, () =>
            {
                RuleFor(x => x.Lines)
                    .Must(lines => lines.Any(l => l.DrAmount > 0) && lines.Any(l => l.CrAmount > 0))
                    .WithMessage("At least one debit line and one credit line are required.");

                RuleFor(x => x)
                    .Must(x => x.Lines.Sum(l => l.DrAmount) == x.Lines.Sum(l => l.CrAmount) && x.Lines.Sum(l => l.DrAmount) > 0)
                    .WithMessage("Total debit must equal total credit and be greater than zero.");

                RuleForEach(x => x.Lines).ChildRules(line =>
                {
                    line.RuleFor(l => l.GlAccountId).GreaterThan(0).WithMessage("Line GL account is required.");
                    line.RuleFor(l => l.CurrencyId).GreaterThan(0).WithMessage("Line currency is required.");
                    line.RuleFor(l => l.DrAmount).GreaterThanOrEqualTo(0).WithMessage("Debit amount cannot be negative.");
                    line.RuleFor(l => l.CrAmount).GreaterThanOrEqualTo(0).WithMessage("Credit amount cannot be negative.");
                    line.RuleFor(l => l)
                        .Must(l => (l.DrAmount > 0) ^ (l.CrAmount > 0))
                        .WithMessage("Each line must be either a debit or a credit (exactly one positive amount).");
                });

                RuleFor(x => x)
                    .MustAsync(AllAccountsExistAsync)
                    .WithMessage("One or more GL accounts do not exist or are inactive.");

                RuleFor(x => x)
                    .MustAsync(PAndLLinesHaveCostCentreAsync)
                    .WithMessage("A cost centre is required on lines whose account is cost-centre mandatory (P&L).");
            });
        }

        private int CompanyId() => _ipAddressService.GetCompanyId() ?? 0;

        private async Task<bool> AllAccountsExistAsync(UpdateJournalCommand command, CancellationToken ct)
        {
            var company = CompanyId();
            foreach (var accId in command.Lines.Select(l => l.GlAccountId).Distinct().Where(i => i > 0))
            {
                if (!await _queryRepository.GlAccountExistsAsync(accId, company))
                    return false;
            }
            return true;
        }

        private async Task<bool> PAndLLinesHaveCostCentreAsync(UpdateJournalCommand command, CancellationToken ct)
        {
            var accIds = command.Lines.Where(l => l.GlAccountId > 0).Select(l => l.GlAccountId).Distinct().ToList();
            var mandatory = (await _queryRepository.GetCostCentreMandatoryAccountIdsAsync(accIds)).ToHashSet();
            return command.Lines
                .Where(l => mandatory.Contains(l.GlAccountId))
                .All(l => l.CostCentreId is > 0);
        }
    }
}

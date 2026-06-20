using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.UpdateProfitCentre;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ProfitCentre
{
    public class UpdateProfitCentreCommandValidator : AbstractValidator<UpdateProfitCentreCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProfitCentreQueryRepository _queryRepository;
        private readonly IUserLookup _userLookup;

        public UpdateProfitCentreCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IProfitCentreQueryRepository queryRepository,
            IUserLookup userLookup)
        {
            _queryRepository = queryRepository;
            _userLookup = userLookup;

            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.ProfitCentre>("ProfitCentreName") ?? 150;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ProfitCentreName)
                            .NotNull().WithMessage($"{nameof(UpdateProfitCentreCommand.ProfitCentreName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateProfitCentreCommand.ProfitCentreName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ProfitCentreName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateProfitCentreCommand.ProfitCentreName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Profit Centre {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateProfitCentreCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Responsible Head — optional, but must be a valid user when supplied.
            RuleFor(x => x.ResponsibleHeadId)
                .MustAsync(async (headId, ct) => await _userLookup.GetByIdAsync(headId!.Value) != null)
                .WithMessage("A valid Responsible Head is required.")
                .When(x => x.ResponsibleHeadId.HasValue && x.ResponsibleHeadId.Value > 0);
        }
    }
}


using FluentValidation;
using SalesManagement.Application.BusinessUnit.Commands.UpdateBusinessUnit;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.BusinessUnit
{
    public class UpdateBusinessUnitCommandValidator : AbstractValidator<UpdateBusinessUnitCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IBusinessUnitQueryRepository _queryRepository;

        public UpdateBusinessUnitCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IBusinessUnitQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.BusinessUnit>("BusinessUnitName") ?? 100;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.BusinessUnit>("Description") ?? 500;

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
                        RuleFor(x => x.BusinessUnitName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateBusinessUnitCommand.BusinessUnitName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateBusinessUnitCommand.BusinessUnitName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.BusinessUnitName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateBusinessUnitCommand.BusinessUnitName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateBusinessUnitCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.BusinessUnitName)
                            .MustAsync(async (cmd, name, ct) => !await _queryRepository.NameAlreadyExistsAsync(name!, cmd.Id))
                            .WithMessage($"{nameof(UpdateBusinessUnitCommand.BusinessUnitName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.BusinessUnitName));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Business Unit ID is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Business Unit {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateBusinessUnitCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}

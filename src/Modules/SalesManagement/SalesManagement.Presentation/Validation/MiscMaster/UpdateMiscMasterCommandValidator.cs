#nullable disable

using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.MiscMaster
{
    public class UpdateMiscMasterCommandValidator : AbstractValidator<UpdateMiscMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMiscMasterQueryRepository _queryRepository;

        public UpdateMiscMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IMiscMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthDescription = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.MiscMaster>("Description") ?? 250;

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
                        RuleFor(x => x.Description)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.Description)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.Description)} {rule.Error}");

                        RuleFor(x => x.SortOrder)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.SortOrder)} must be 0 or greater.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) =>
                                !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Misc Master {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}

using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IPackType;
using ProductionManagement.Application.PackType.Commands.UpdatePackType;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.PackType
{
    public class UpdatePackTypeCommandValidator : AbstractValidator<UpdatePackTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPackTypeQueryRepository _queryRepository;

        public UpdatePackTypeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IPackTypeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName = maxLengthProvider.GetMaxLength<ProductionManagement.Domain.Entities.PackType>("PackTypeName") ?? 100;

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
                        RuleFor(x => x.PackTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdatePackTypeCommand.PackTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdatePackTypeCommand.PackTypeName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.PackTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdatePackTypeCommand.PackTypeName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"PackType {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdatePackTypeCommand.IsActive)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.NetWeight)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdatePackTypeCommand.NetWeight)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.TareWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdatePackTypeCommand.TareWeight)} {rule.Error}");

                        RuleFor(x => x.ConesPerBag)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdatePackTypeCommand.ConesPerBag)} {rule.Error}")
                            .When(x => x.ConesPerBag.HasValue);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}

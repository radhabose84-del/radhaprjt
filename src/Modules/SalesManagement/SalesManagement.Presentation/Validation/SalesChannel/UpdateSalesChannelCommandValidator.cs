#nullable disable

using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Commands.UpdateSalesChannel;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesChannel
{
    public class UpdateSalesChannelCommandValidator : AbstractValidator<UpdateSalesChannelCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesChannelQueryRepository _queryRepository;

        public UpdateSalesChannelCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesChannelQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.SalesChannel>("SalesChannelName") ?? 100;

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
                        RuleFor(x => x.SalesChannelName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateSalesChannelCommand.SalesChannelName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesChannelCommand.SalesChannelName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SalesChannelName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateSalesChannelCommand.SalesChannelName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Channel {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateSalesChannelCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}

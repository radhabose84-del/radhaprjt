#nullable disable

using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Commands.CreateSalesChannel;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesChannel
{
    public class CreateSalesChannelCommandValidator : AbstractValidator<CreateSalesChannelCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesChannelQueryRepository _queryRepository;

        public CreateSalesChannelCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesChannelQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCode = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.SalesChannel>("SalesChannelCode") ?? 20;
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
                        RuleFor(x => x.SalesChannelCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesChannelCommand.SalesChannelCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesChannelCommand.SalesChannelCode)} {rule.Error}");

                        RuleFor(x => x.SalesChannelName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesChannelCommand.SalesChannelName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesChannelCommand.SalesChannelName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.SalesChannelCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateSalesChannelCommand.SalesChannelCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesChannelCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SalesChannelCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateSalesChannelCommand.SalesChannelCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.SalesChannelName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateSalesChannelCommand.SalesChannelName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.SalesChannelCode)
                            .MustAsync(async (code, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(code))
                            .WithMessage($"{nameof(CreateSalesChannelCommand.SalesChannelCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesChannelCode));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}

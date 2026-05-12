using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Commands.CreateSalesAgreement;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesAgreement
{
    public class CreateSalesAgreementCommandValidator : AbstractValidator<CreateSalesAgreementCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesAgreementQueryRepository _queryRepository;

        public CreateSalesAgreementCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesAgreementQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.SalesAgreementHeader>("Remarks") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ValidFrom)
                            .NotEmpty().WithMessage($"ValidFrom {rule.Error}");

                        RuleFor(x => x.ValidTo)
                            .NotEmpty().WithMessage($"ValidTo {rule.Error}");

                        RuleFor(x => x.CustomerId)
                            .NotEmpty().WithMessage($"CustomerId {rule.Error}");

                        RuleFor(x => x.SalesGroupId)
                            .NotEmpty().WithMessage($"SalesGroupId {rule.Error}");

                        RuleFor(x => x.PaymentTermsId)
                            .NotEmpty().WithMessage($"PaymentTermsId {rule.Error}");

                        RuleFor(x => x.SalesAgreementDetails)
                            .NotNull().WithMessage($"SalesAgreementDetails {rule.Error}")
                            .NotEmpty().WithMessage($"SalesAgreementDetails {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "DateCompare":
                        RuleFor(x => x.ValidTo)
                            .GreaterThanOrEqualTo(x => x.ValidFrom)
                            .WithMessage($"ValidTo {rule.Error} ValidFrom.");
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.SalesAgreementDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.ItemId)
                                    .GreaterThan(0).WithMessage($"ItemId {rule.Error}");

                                detail.RuleFor(d => d.AgreedRate)
                                    .GreaterThan(0).WithMessage($"AgreedRate {rule.Error}");

                                detail.RuleFor(d => d.TotalQty)
                                    .GreaterThan(0).WithMessage($"TotalQty {rule.Error}");
                            })
                            .When(x => x.SalesAgreementDetails != null && x.SalesAgreementDetails.Count > 0);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CustomerId)
                            .MustAsync(async (id, ct) => await _queryRepository.CustomerExistsAsync(id))
                            .WithMessage($"CustomerId {rule.Error}")
                            .When(x => x.CustomerId > 0);

                        RuleFor(x => x.SalesGroupId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesGroupExistsAsync(id))
                            .WithMessage($"SalesGroupId {rule.Error}")
                            .When(x => x.SalesGroupId > 0);

                        RuleFor(x => x.PaymentTermsId)
                            .MustAsync(async (id, ct) => await _queryRepository.PaymentTermsExistsAsync(id))
                            .WithMessage($"PaymentTermsId {rule.Error}")
                            .When(x => x.PaymentTermsId > 0);

                        RuleForEach(x => x.SalesAgreementDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.ItemId)
                                    .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id))
                                    .WithMessage($"ItemId {rule.Error}")
                                    .When(d => d.ItemId > 0);

                                detail.RuleFor(d => d.VariantId)
                                    .MustAsync(async (id, ct) => await _queryRepository.VariantExistsAsync(id!.Value))
                                    .WithMessage($"VariantId {rule.Error}")
                                    .When(d => d.VariantId.HasValue && d.VariantId.Value > 0);
                            })
                            .When(x => x.SalesAgreementDetails != null && x.SalesAgreementDetails.Count > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}

using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Commands.CreateSalesLead;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesLead
{
    public class CreateSalesLeadCommandValidator : AbstractValidator<CreateSalesLeadCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesLeadQueryRepository _queryRepository;

        public CreateSalesLeadCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesLeadQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthProspectCompanyName = maxLengthProvider.GetMaxLength<Domain.Entities.SalesLead>("ProspectCompanyName") ?? 200;
            var maxLengthContactName = maxLengthProvider.GetMaxLength<Domain.Entities.SalesLead>("ContactName") ?? 100;
            var maxLengthMobileNumber = maxLengthProvider.GetMaxLength<Domain.Entities.SalesLead>("MobileNumber") ?? 20;
            var maxLengthEmailId = maxLengthProvider.GetMaxLength<Domain.Entities.SalesLead>("EmailId") ?? 150;
            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.SalesLead>("Remarks") ?? 500;

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
                        RuleFor(x => x.ContactName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesLeadCommand.ContactName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesLeadCommand.ContactName)} {rule.Error}");

                        RuleFor(x => x.MobileNumber)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesLeadCommand.MobileNumber)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesLeadCommand.MobileNumber)} {rule.Error}");

                        RuleFor(x => x.MarketingPersonId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesLeadCommand.MarketingPersonId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesLeadCommand.MarketingPersonId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ProspectCompanyName)
                            .MaximumLength(maxLengthProspectCompanyName)
                            .WithMessage($"{nameof(CreateSalesLeadCommand.ProspectCompanyName)} {rule.Error} {maxLengthProspectCompanyName} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ProspectCompanyName));

                        RuleFor(x => x.ContactName)
                            .MaximumLength(maxLengthContactName)
                            .WithMessage($"{nameof(CreateSalesLeadCommand.ContactName)} {rule.Error} {maxLengthContactName} characters.");

                        RuleFor(x => x.MobileNumber)
                            .MaximumLength(maxLengthMobileNumber)
                            .WithMessage($"{nameof(CreateSalesLeadCommand.MobileNumber)} {rule.Error} {maxLengthMobileNumber} characters.");

                        RuleFor(x => x.EmailId)
                            .MaximumLength(maxLengthEmailId)
                            .WithMessage($"{nameof(CreateSalesLeadCommand.EmailId)} {rule.Error} {maxLengthEmailId} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.EmailId));

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateSalesLeadCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.MobileNumber)
                            .MustAsync(async (mobile, ct) =>
                                !await _queryRepository.MobileNumberExistsForProspectAsync(mobile!))
                            .WithMessage($"{nameof(CreateSalesLeadCommand.MobileNumber)} {rule.Error}")
                            .When(x => x.PartyId == null && !string.IsNullOrWhiteSpace(x.MobileNumber));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ContactId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.ContactExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateSalesLeadCommand.ContactId)} {rule.Error}")
                            .When(x => x.ContactId.HasValue && x.ContactId > 0);

                        RuleFor(x => x.LeadSourceId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.LeadSourceExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateSalesLeadCommand.LeadSourceId)} {rule.Error}")
                            .When(x => x.LeadSourceId.HasValue && x.LeadSourceId > 0);

                        RuleFor(x => x.PartyId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.PartyExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateSalesLeadCommand.PartyId)} {rule.Error}")
                            .When(x => x.PartyId.HasValue && x.PartyId > 0);

                        RuleFor(x => x.CityId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.CityExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateSalesLeadCommand.CityId)} {rule.Error}")
                            .When(x => x.CityId.HasValue && x.CityId > 0);

                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.ItemExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateSalesLeadCommand.ItemId)} {rule.Error}")
                            .When(x => x.ItemId.HasValue && x.ItemId > 0);

                        RuleFor(x => x.MarketingPersonId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.MarketingPersonExistsAsync(id))
                            .WithMessage($"{nameof(CreateSalesLeadCommand.MarketingPersonId)} {rule.Error}")
                            .When(x => x.MarketingPersonId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}

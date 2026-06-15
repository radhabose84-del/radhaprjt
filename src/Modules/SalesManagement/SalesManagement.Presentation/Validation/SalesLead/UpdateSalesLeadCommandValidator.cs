using FluentValidation;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Commands.UpdateSalesLead;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesLead
{
    public class UpdateSalesLeadCommandValidator : AbstractValidator<UpdateSalesLeadCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesLeadQueryRepository _queryRepository;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public UpdateSalesLeadCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesLeadQueryRepository queryRepository,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _queryRepository = queryRepository;
            _accessFilter = accessFilter;

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
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.ContactName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.ContactName)} {rule.Error}");

                        RuleFor(x => x.MobileNumber)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.MobileNumber)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.MobileNumber)} {rule.Error}");

                        RuleFor(x => x.MarketingOfficerId)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.MarketingOfficerId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.MarketingOfficerId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ProspectCompanyName)
                            .MaximumLength(maxLengthProspectCompanyName)
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.ProspectCompanyName)} {rule.Error} {maxLengthProspectCompanyName} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ProspectCompanyName));

                        RuleFor(x => x.ContactName)
                            .MaximumLength(maxLengthContactName)
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.ContactName)} {rule.Error} {maxLengthContactName} characters.");

                        RuleFor(x => x.MobileNumber)
                            .MaximumLength(maxLengthMobileNumber)
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.MobileNumber)} {rule.Error} {maxLengthMobileNumber} characters.");

                        RuleFor(x => x.EmailId)
                            .MaximumLength(maxLengthEmailId)
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.EmailId)} {rule.Error} {maxLengthEmailId} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.EmailId));

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0)
                            .WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Lead {rule.Error}");

                        // Closed leads are read-only
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.IsClosedAsync(id))
                            .WithMessage("This lead is closed and cannot be modified.")
                            .When(x => x.Id > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.MobileNumber)
                            .MustAsync(async (cmd, mobile, ct) =>
                                !await _queryRepository.MobileNumberExistsForProspectAsync(mobile!, cmd.Id!))
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.MobileNumber)} {rule.Error}")
                            .When(x => x.PartyId == null && !string.IsNullOrWhiteSpace(x.MobileNumber));

                        RuleFor(x => x.MobileNumber)
                            .MustAsync(async (mobile, ct) =>
                                !await _queryRepository.MobileNumberExistsInSalesContactAsync(mobile!))
                            .WithMessage("Contact mobile number already exists. Please select correct contact details.")
                            .When(x => !x.ContactId.HasValue && !string.IsNullOrWhiteSpace(x.ContactName)
                                    && !string.IsNullOrWhiteSpace(x.MobileNumber));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ContactId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.ContactExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.ContactId)} {rule.Error}")
                            .When(x => x.ContactId.HasValue && x.ContactId > 0);

                        RuleFor(x => x.LeadSourceId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.LeadSourceExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.LeadSourceId)} {rule.Error}")
                            .When(x => x.LeadSourceId.HasValue && x.LeadSourceId > 0);

                        RuleFor(x => x.PartyId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.PartyExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.PartyId)} {rule.Error}")
                            .When(x => x.PartyId.HasValue && x.PartyId > 0);

                        RuleFor(x => x.CityId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.CityExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.CityId)} {rule.Error}")
                            .When(x => x.CityId.HasValue && x.CityId > 0);

                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.ItemExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.ItemId)} {rule.Error}")
                            .When(x => x.ItemId.HasValue && x.ItemId > 0);

                        RuleFor(x => x.UomId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.UomExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.UomId)} {rule.Error}")
                            .When(x => x.UomId.HasValue && x.UomId > 0);

                        RuleFor(x => x.MarketingOfficerId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.MarketingOfficerExistsAsync(id))
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.MarketingOfficerId)} {rule.Error}")
                            .When(x => x.MarketingOfficerId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.IsActive)} {rule.Error}");
                        break;

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.PartyId)
                            .MustAsync(async (id, ct) => await _accessFilter.CanAccessCustomerAsync(id!.Value, ct))
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.PartyId)} {rule.Error}")
                            .When(x => x.PartyId.HasValue && x.PartyId.Value > 0);

                        RuleFor(x => x.MarketingOfficerId)
                            .MustAsync(async (id, ct) => !await _accessFilter.ShouldApplyFilterAsync(ct)
                                        || id == _accessFilter.GetCurrentMarketingOfficerId())
                            .WithMessage($"{nameof(UpdateSalesLeadCommand.MarketingOfficerId)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}

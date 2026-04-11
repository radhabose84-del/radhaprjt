using Contracts.Interfaces.Lookups.Party;
using FluentValidation;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.SalesContact.Commands.UpdateSalesContact;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesContact
{
    public class UpdateSalesContactCommandValidator : AbstractValidator<UpdateSalesContactCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesContactQueryRepository _queryRepo;
        private readonly IPartyLookup _partyLookup;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public UpdateSalesContactCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesContactQueryRepository queryRepo,
            IPartyLookup partyLookup,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _queryRepo = queryRepo;
            _partyLookup = partyLookup;
            _accessFilter = accessFilter;

            var maxLengthName   = maxLengthProvider.GetMaxLength<Domain.Entities.SalesContact>("ContactName") ?? 100;
            var maxLengthMobile = maxLengthProvider.GetMaxLength<Domain.Entities.SalesContact>("MobileNumber") ?? 15;
            var maxLengthEmail  = maxLengthProvider.GetMaxLength<Domain.Entities.SalesContact>("Email") ?? 100;
            var maxLengthRemark = maxLengthProvider.GetMaxLength<Domain.Entities.SalesContact>("Remarks") ?? 500;

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
                        RuleFor(x => x.ContactName)
                            .NotNull().WithMessage($"{nameof(UpdateSalesContactCommand.ContactName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateSalesContactCommand.ContactName)} {rule.Error}");

                        RuleFor(x => x.MobileNumber)
                            .NotNull().WithMessage($"{nameof(UpdateSalesContactCommand.MobileNumber)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateSalesContactCommand.MobileNumber)} {rule.Error}");

                        RuleFor(x => x.ContactTypeId)
                            .NotNull().WithMessage($"{nameof(UpdateSalesContactCommand.ContactTypeId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateSalesContactCommand.ContactTypeId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ContactName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateSalesContactCommand.ContactName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.MobileNumber)
                            .MaximumLength(maxLengthMobile)
                            .WithMessage($"{nameof(UpdateSalesContactCommand.MobileNumber)} {rule.Error} {maxLengthMobile} characters.");

                        RuleFor(x => x.Email)
                            .MaximumLength(maxLengthEmail)
                            .WithMessage($"{nameof(UpdateSalesContactCommand.Email)} {rule.Error} {maxLengthEmail} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Email));

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemark)
                            .WithMessage($"{nameof(UpdateSalesContactCommand.Remarks)} {rule.Error} {maxLengthRemark} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Sales Contact {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ContactTypeId)
                            .MustAsync(async (contactTypeId, ct) => await _queryRepo.ContactTypeExistsAsync(contactTypeId))
                            .WithMessage($"{nameof(UpdateSalesContactCommand.ContactTypeId)} {rule.Error}")
                            .When(x => x.ContactTypeId > 0);

                        RuleFor(x => x.PartyId)
                            .MustAsync(async (partyId, ct) => await _partyLookup.GetByIdAsync(partyId!.Value, ct) != null)
                            .WithMessage($"{nameof(UpdateSalesContactCommand.PartyId)} {rule.Error}")
                            .When(x => x.PartyId.HasValue && x.PartyId.Value > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.MobileNumber)
                            .MustAsync(async (command, mobile, ct) =>
                                !await _queryRepo.MobileAlreadyExistsAsync(mobile!, command.Id))
                            .WithMessage($"{nameof(UpdateSalesContactCommand.MobileNumber)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateSalesContactCommand.IsActive)} {rule.Error}");
                        break;

                    case "MobileNumber":
                        RuleFor(x => x.MobileNumber)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(UpdateSalesContactCommand.MobileNumber)}{rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber));
                        break;

                    case "Email":
                        RuleFor(x => x.Email)
                            .EmailAddress()
                            .WithMessage($"{nameof(UpdateSalesContactCommand.Email)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Email));
                        break;

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.PartyId)
                            .MustAsync(async (id, ct) => await _accessFilter.CanAccessCustomerAsync(id!.Value, ct))
                            .WithMessage($"{nameof(UpdateSalesContactCommand.PartyId)} {rule.Error}")
                            .When(x => x.PartyId.HasValue && x.PartyId.Value > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}

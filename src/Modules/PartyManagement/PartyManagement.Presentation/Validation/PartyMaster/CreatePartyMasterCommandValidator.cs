using Contracts.Interfaces.Lookups.Workflow;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
using PartyManagement.Domain.Common;
using FluentValidation;
using PartyManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PartyManagement.Presentation.Validation.PartyMaster
{
    public class CreatePartyMasterCommandValidator : AbstractValidator<CreatePartyMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPartyMasterCommandRepository _iPartyMasterCommandRepository;
        private readonly IPartyMasterQueryRepository _iPartyMasterQueryRepository;
        private readonly IWorkflowLookup _workflowLookup;
        public CreatePartyMasterCommandValidator(IPartyMasterCommandRepository iPartyMasterCommandRepository, MaxLengthProvider maxLengthProvider
         , IWorkflowLookup workflowLookup
        , IPartyMasterQueryRepository iPartyMasterQueryRepository)
        {
            _iPartyMasterCommandRepository = iPartyMasterCommandRepository;
             _workflowLookup = workflowLookup;
            _iPartyMasterQueryRepository = iPartyMasterQueryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {

                   case "Workflow":
                            RuleFor(x => x.PartyMaster.UnitId)
                                .MustAsync(async (unitId, cancellation) =>
                                    await _workflowLookup.IsApproveWorkflowConfigureAsync(
                                        PartyManagement.Domain.Common.MiscEnumEntity.PartyDocumentImage.PartyMaster, // entity type
                                        unitId,
                                        0))                      // DepartmentId not required, pass null
                                .WithMessage(rule.Error);
                            break;
                    case "AlreadyExists":
                        RuleFor(x => new { x.PartyMaster.PartyName })
                        .MustAsync(async (input, cancellation) =>
                            !await _iPartyMasterCommandRepository.ExistsAsync(input.PartyName ?? string.Empty))
                        .WithName("PartyName")
                        .WithMessage(" PartyName already exists.");

                        RuleFor(x => x.PartyMaster)
                            .MustAsync(async (partyMaster, cancellation) =>
                            {
                                if (partyMaster == null) return true;

                                // Allow duplicates if IsGroup == 1
                                if (partyMaster.IsGroup == 1)
                                    return true;
                             // ✅ Fetch Registration details
                                var registration = await _iPartyMasterQueryRepository.GetRegistrationDetails(partyMaster.RegistrationTypeId ?? 0);

                                 // ✅ Skip GST validation if registration type is Un-Registered (using enum constant)
                                    if (registration != null &&
                                        registration.Description.Trim().Equals(
                                            MiscEnumEntity.PartyDocumentImage.UnRegistered,
                                            StringComparison.OrdinalIgnoreCase))
                                    {
                                        return true;
                                    }
                                // Otherwise, check for duplicate GST number
                                return !await _iPartyMasterCommandRepository.GstNumberExistsAsync(partyMaster.GSTNumber ?? string.Empty);
                            })
                            .WithMessage("GST Number already exists for non-group parties.");

                         // ✅ EmailID (check across PartyContact)
                        RuleFor(x => x.PartyMaster.PartyContacts)
                            .MustAsync(async (contacts, cancellation) =>
                            {
                                if (contacts == null || !contacts.Any()) return true;
                                foreach (var contact in contacts)
                                {
                                    if (await _iPartyMasterCommandRepository
                                        .EmailExistsAsync(contact.EmailID ?? string.Empty))
                                        return false;
                                }
                                return true;
                            })
                            .WithMessage("EmailID already exists for another party.");

                            // ✅ MobileNo (check across PartyContact)
                            RuleFor(x => x.PartyMaster.PartyContacts)
                                .MustAsync(async (contacts, cancellation) =>
                                {
                                    if (contacts == null || !contacts.Any()) return true;
                                    foreach (var contact in contacts)
                                    {
                                        if (await _iPartyMasterCommandRepository
                                            .MobileExistsAsync(contact.MobileNo ?? string.Empty))
                                            return false;
                                    }
                                    return true;
                                })
                                .WithMessage("Mobile number already exists for another party.");

                        break;

                    case "NotEmpty":
                        RuleFor(x => x.PartyMaster.RegistrationTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.RegistrationTypeId)} {rule.Error}");

                        RuleFor(x => x.PartyMaster.PartyName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyName)} {rule.Error}");

                        RuleFor(x => x.PartyMaster.PAN)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PAN)} {rule.Error}");

                        RuleFor(x => x.PartyMaster.PartyTypes)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyTypes)} {rule.Error}");

                        RuleForEach(x => x.PartyMaster.PartyTypes).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.PartyTypeId)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyTypes)}.{nameof(CreatePartyMasterDto.PartyTypeDto.PartyTypeId)} {rule.Error}");

                            partyType.RuleFor(pt => pt.PartyGroupId)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyTypes)}.{nameof(CreatePartyMasterDto.PartyTypeDto.PartyGroupId)} {rule.Error}");
                        });

                        RuleFor(x => x.PartyMaster.PartyUnitCompanies)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyUnitCompanies)} {rule.Error}");

                        RuleForEach(x => x.PartyMaster.PartyUnitCompanies).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.CompanyId)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyUnitCompanies)}.{nameof(CreatePartyMasterDto.PartyUnitCompanyDto.CompanyId)} {rule.Error}");

                            partyType.RuleFor(pt => pt.UnitId)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyUnitCompanies)}.{nameof(CreatePartyMasterDto.PartyUnitCompanyDto.UnitId)} {rule.Error}");
                        });

                        RuleFor(x => x.PartyMaster.PartyContacts)
                           .NotEmpty()
                           .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyContacts)} {rule.Error}");

                        RuleForEach(x => x.PartyMaster.PartyContacts).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.FirstName)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyContacts)}.{nameof(CreatePartyMasterDto.PartyContactDto.FirstName)} {rule.Error}");

                            partyType.RuleFor(pt => pt.MobileNo)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyContacts)}.{nameof(CreatePartyMasterDto.PartyContactDto.MobileNo)} {rule.Error}");

                            partyType.RuleFor(pt => pt.EmailID)
                               .NotEmpty()
                               .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyContacts)}.{nameof(CreatePartyMasterDto.PartyContactDto.EmailID)} {rule.Error}");

                            partyType.RuleFor(pt => pt.ContactBy)
                              .NotEmpty()
                              .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyContacts)}.{nameof(CreatePartyMasterDto.PartyContactDto.ContactBy)} {rule.Error}");
                        });

                        RuleFor(x => x.PartyMaster.PartyAddresses)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyAddresses)} {rule.Error}");

                        RuleForEach(x => x.PartyMaster.PartyAddresses).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.AddressType)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyAddresses)}.{nameof(CreatePartyMasterDto.PartyAddressDto.AddressType)} {rule.Error}");

                            partyType.RuleFor(pt => pt.City)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyAddresses)}.{nameof(CreatePartyMasterDto.PartyAddressDto.City)} {rule.Error}");

                            partyType.RuleFor(pt => pt.State)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyAddresses)}.{nameof(CreatePartyMasterDto.PartyAddressDto.State)} {rule.Error}");

                            partyType.RuleFor(pt => pt.Country)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyAddresses)}.{nameof(CreatePartyMasterDto.PartyAddressDto.Country)} {rule.Error}");
                        });
                        break;

                    case "MinLength":
                        RuleFor(x => x.PartyMaster.RegistrationTypeId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.RegistrationTypeId)} {rule.Error} {0}");

                        RuleForEach(x => x.PartyMaster.PartyTypes).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.PartyTypeId)
                                .GreaterThanOrEqualTo(1)
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyTypes)}.{nameof(CreatePartyMasterDto.PartyTypeDto.PartyTypeId)} {rule.Error}");

                            partyType.RuleFor(pt => pt.PartyGroupId)
                               .GreaterThanOrEqualTo(1)
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyTypes)}.{nameof(CreatePartyMasterDto.PartyTypeDto.PartyGroupId)} {rule.Error}");
                        });

                         RuleForEach(x => x.PartyMaster.PartyUnitCompanies).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.CompanyId)
                                .GreaterThanOrEqualTo(1)
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyUnitCompanies)}.{nameof(CreatePartyMasterDto.PartyUnitCompanyDto.CompanyId)} {rule.Error}");

                            partyType.RuleFor(pt => pt.UnitId)
                               .GreaterThanOrEqualTo(1)
                                .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyUnitCompanies)}.{nameof(CreatePartyMasterDto.PartyUnitCompanyDto.UnitId)} {rule.Error}");
                        });

                        break;

                    case "MobileNumber":
                        RuleFor(x => x.PartyMaster.PartyContacts)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyContacts)} {rule.Error}");

                        RuleForEach(x => x.PartyMaster.PartyContacts).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.MobileNo)
                                .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                                .WithMessage($"{nameof(CreatePartyMasterDto.PartyContactDto.MobileNo)} {rule.Error}");
                        });
                        break;

                    case "Email":
                        RuleFor(x => x.PartyMaster.PartyContacts)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePartyMasterCommand.PartyMaster.PartyContacts)} {rule.Error}");

                        RuleForEach(x => x.PartyMaster.PartyContacts).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.EmailID)
                                .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                                .WithMessage($"{nameof(CreatePartyMasterDto.PartyContactDto.EmailID)} {rule.Error}");
                        });
                        break;

                }
            }
        }
         
    }
}

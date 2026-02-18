#nullable disable
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Command.UpdatePartyMaster;
using PartyManagement.Domain.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace PartyManagement.Presentation.Validation.PartyMaster
{
    public class UpdatePartyMasterCommandValidator : AbstractValidator<UpdatePartyMasterCommand>
    {

        private readonly List<ValidationRule> _validationRules;
        private readonly IPartyMasterCommandRepository _iPartyMasterCommandRepository;
         private readonly IPartyMasterQueryRepository _iPartyMasterQueryRepository;

        public UpdatePartyMasterCommandValidator(IPartyMasterCommandRepository iPartyMasterCommandRepository, IPartyMasterQueryRepository iPartyMasterQueryRepository)
        {
            _validationRules = new List<ValidationRule>();
            _iPartyMasterCommandRepository = iPartyMasterCommandRepository;
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
                    case "AlreadyExists":
                        RuleFor(x => x.UpdatePartyMaster.PartyName)
                            .MustAsync(async (root, partyName, cancellation) =>
                                !await _iPartyMasterCommandRepository.ExistsForUpdateAsync(
                                    partyName ?? string.Empty,
                                    root.UpdatePartyMaster.Id))
                            .WithName("PartyName")
                            .WithMessage("PartyName already exists.");

                        RuleFor(x => x.UpdatePartyMaster)
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

                                // Otherwise check for duplicate GST excluding current PartyId
                                return !await _iPartyMasterCommandRepository
                                    .GstNumberExistsAsync(partyMaster.GSTNumber ?? string.Empty, partyMaster.Id);
                            })
                            .WithMessage("GST Number already exists for non-group parties.");


                        // ✅ EmailID (Update)
                        RuleFor(x => x.UpdatePartyMaster.PartyContactsUpdate)
                            .MustAsync(async (contacts, cancellation) =>
                            {
                                if (contacts == null || !contacts.Any()) return true;

                                foreach (var contact in contacts)
                                {
                                    if (await _iPartyMasterCommandRepository
                                        .EmailExistsUpdateAsync(contact.EmailID ?? string.Empty, contact.Id))
                                        return false;
                                }
                                return true;
                            })
                            .WithMessage("EmailID already exists for another party.");

                        // ✅ MobileNo (Update)
                        RuleFor(x => x.UpdatePartyMaster.PartyContactsUpdate)
                            .MustAsync(async (contacts, cancellation) =>
                            {
                                if (contacts == null || !contacts.Any()) return true;

                                foreach (var contact in contacts)
                                {
                                    if (await _iPartyMasterCommandRepository
                                        .MobileExistsUpdateAsync(contact.MobileNo ?? string.Empty, contact.Id))
                                        return false;
                                }
                                return true;
                            })
                            .WithMessage("Mobile number already exists for another party.");
                        break;

                    case "NotEmpty":
                        RuleFor(x => x.UpdatePartyMaster.RegistrationTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.RegistrationTypeId)} {rule.Error}");

                        RuleFor(x => x.UpdatePartyMaster.PartyName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyName)} {rule.Error}");

                        RuleFor(x => x.UpdatePartyMaster.PAN)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PAN)} {rule.Error}");

                        RuleFor(x => x.UpdatePartyMaster.PartyTypesUpdate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyTypesUpdate)} {rule.Error}");

                        RuleForEach(x => x.UpdatePartyMaster.PartyTypesUpdate).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.PartyTypeId)
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyTypesUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyTypeDto.PartyTypeId)} {rule.Error}");

                            partyType.RuleFor(pt => pt.PartyGroupId)
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyTypesUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyTypeDto.PartyGroupId)} {rule.Error}");
                        });

                        RuleFor(x => x.UpdatePartyMaster.PartyUnitCompaniesUpdate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyUnitCompaniesUpdate)} {rule.Error}");

                        RuleForEach(x => x.UpdatePartyMaster.PartyUnitCompaniesUpdate).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.CompanyId)
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyUnitCompaniesUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyUniCompanyDto.CompanyId)} {rule.Error}");

                            partyType.RuleFor(pt => pt.UnitId)
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyUnitCompaniesUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyUniCompanyDto.UnitId)} {rule.Error}");
                        });

                        RuleFor(x => x.UpdatePartyMaster.PartyContactsUpdate)
                           .NotEmpty()
                           .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyContactsUpdate)} {rule.Error}");

                        RuleForEach(x => x.UpdatePartyMaster.PartyContactsUpdate).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.FirstName)
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyContactsUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyContactDto.FirstName)} {rule.Error}");

                            partyType.RuleFor(pt => pt.MobileNo)
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyContactsUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyContactDto.MobileNo)} {rule.Error}");

                            partyType.RuleFor(pt => pt.EmailID)
                               .NotEmpty()
                               .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyContactsUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyContactDto.EmailID)} {rule.Error}");

                            partyType.RuleFor(pt => pt.ContactBy)
                              .NotEmpty()
                              .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyContactsUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyContactDto.ContactBy)} {rule.Error}");
                        });

                        RuleFor(x => x.UpdatePartyMaster.PartyAddressesUpdate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyAddressesUpdate)} {rule.Error}");

                        RuleForEach(x => x.UpdatePartyMaster.PartyAddressesUpdate).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.AddressType)
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyAddressesUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyAddressDto.AddressType)} {rule.Error}");

                            partyType.RuleFor(pt => pt.City)
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyAddressesUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyAddressDto.City)} {rule.Error}");

                            partyType.RuleFor(pt => pt.State)
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyAddressesUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyAddressDto.State)} {rule.Error}");

                            partyType.RuleFor(pt => pt.Country)
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyAddressesUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyAddressDto.Country)} {rule.Error}");
                        });
                        break;

                    case "MinLength":
                        RuleFor(x => x.UpdatePartyMaster.RegistrationTypeId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.RegistrationTypeId)} {rule.Error} {0}");

                        RuleForEach(x => x.UpdatePartyMaster.PartyTypesUpdate).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.PartyTypeId)
                                .GreaterThanOrEqualTo(1)
                                .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyTypesUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyTypeDto.PartyTypeId)} {rule.Error}");

                            partyType.RuleFor(pt => pt.PartyGroupId)
                               .GreaterThanOrEqualTo(1)
                                .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyTypesUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyTypeDto.PartyGroupId)} {rule.Error}");
                        });

                        RuleForEach(x => x.UpdatePartyMaster.PartyUnitCompaniesUpdate).ChildRules(partyType =>
                       {
                           partyType.RuleFor(pt => pt.CompanyId)
                               .GreaterThanOrEqualTo(1)
                               .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyUnitCompaniesUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyUniCompanyDto.CompanyId)} {rule.Error}");

                           partyType.RuleFor(pt => pt.UnitId)
                              .GreaterThanOrEqualTo(1)
                               .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyUnitCompaniesUpdate)}.{nameof(UpdatePartyMasterDto.UpdatePartyUniCompanyDto.UnitId)} {rule.Error}");
                       });
                        break;

                    case "MobileNumber":
                        RuleFor(x => x.UpdatePartyMaster.PartyContactsUpdate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyContactsUpdate)} {rule.Error}");

                        RuleForEach(x => x.UpdatePartyMaster.PartyContactsUpdate).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.MobileNo)
                                .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                                .WithMessage($"{nameof(UpdatePartyMasterDto.UpdatePartyContactDto.MobileNo)} {rule.Error}");
                        });
                        break;

                    case "Email":
                        RuleFor(x => x.UpdatePartyMaster.PartyContactsUpdate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdatePartyMasterCommand.UpdatePartyMaster.PartyContactsUpdate)} {rule.Error}");

                        RuleForEach(x => x.UpdatePartyMaster.PartyContactsUpdate).ChildRules(partyType =>
                        {
                            partyType.RuleFor(pt => pt.EmailID)
                                .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                                .WithMessage($"{nameof(UpdatePartyMasterDto.UpdatePartyContactDto.EmailID)} {rule.Error}");
                        });
                        break;

                }
            }
        }
    }
}
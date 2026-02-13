using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.GRN.GateEntry.Commands.CreateGateEntry;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using FluentValidation;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.GRN.GateEntry
{
    public class CreateGateEntryCommandValidator : AbstractValidator<CreateGateEntryCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateGateEntryCommandValidator(MaxLengthProvider maxLengthProvider)
        {
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
                        RuleFor(x => x.GateEntryDetails.PartyId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateGateEntryCommand.GateEntryDetails.PartyId)} {rule.Error}");

                         RuleFor(x => x.GateEntryDetails.ReceivingTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateGateEntryCommand.GateEntryDetails.ReceivingTypeId)} {rule.Error}");

                        RuleForEach(x => x.GateEntryDetails.GateEntryDetails).ChildRules(GateEntry =>
                       {
                           GateEntry.RuleFor(pt => pt.PoId)
                               .NotEmpty()
                               .WithMessage($"{nameof(CreateGateEntryCommand.GateEntryDetails.GateEntryDetails)}.{nameof(CreateGateEntryDto.GateEntryDetailDto.PoId)} {rule.Error}");

                           GateEntry.RuleFor(pt => pt.PoDate)
                               .NotEmpty()
                               .WithMessage($"{nameof(CreateGateEntryCommand.GateEntryDetails.GateEntryDetails)}.{nameof(CreateGateEntryDto.GateEntryDetailDto.PoDate)} {rule.Error}");

                           GateEntry.RuleFor(pt => pt.PoCreatedBy)
                               .NotEmpty()
                               .WithMessage($"{nameof(CreateGateEntryCommand.GateEntryDetails.GateEntryDetails)}.{nameof(CreateGateEntryDto.GateEntryDetailDto.PoCreatedBy)} {rule.Error}");

                           GateEntry.RuleFor(pt => pt.ContactDetails)
                               .NotEmpty()
                               .WithMessage($"{nameof(CreateGateEntryCommand.GateEntryDetails.GateEntryDetails)}.{nameof(CreateGateEntryDto.GateEntryDetailDto.ContactDetails)} {rule.Error}");

                            GateEntry.RuleFor(pt => pt.PoCategoryId)
                               .NotEmpty()
                               .WithMessage($"{nameof(CreateGateEntryCommand.GateEntryDetails.GateEntryDetails)}.{nameof(CreateGateEntryDto.GateEntryDetailDto.PoCategoryId)} {rule.Error}");
                       });
                        break;

                        case "MinLength":
                        RuleFor(x => x.GateEntryDetails.PartyId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateGateEntryCommand.GateEntryDetails.PartyId)} {rule.Error} {0}");

                        RuleFor(x => x.GateEntryDetails.ReceivingTypeId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateGateEntryCommand.GateEntryDetails.ReceivingTypeId)} {rule.Error} {0}");

                        RuleForEach(x => x.GateEntryDetails.GateEntryDetails).ChildRules(GateEntry =>
                       {
                           GateEntry.RuleFor(pt => pt.PoId)
                               .GreaterThanOrEqualTo(1)
                               .WithMessage($"{nameof(CreateGateEntryCommand.GateEntryDetails.GateEntryDetails)}.{nameof(CreateGateEntryDto.GateEntryDetailDto.PoId)} {rule.Error}");

                            GateEntry.RuleFor(pt => pt.PoCategoryId)
                               .GreaterThanOrEqualTo(1)
                               .WithMessage($"{nameof(CreateGateEntryCommand.GateEntryDetails.GateEntryDetails)}.{nameof(CreateGateEntryDto.GateEntryDetailDto.PoCategoryId)} {rule.Error}");
                       });
                        break;
                         
                    
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using FluentValidation;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.MiscMaster
{
    public class DeleteMiscMasterCommandValidator : AbstractValidator<DeleteMiscMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;


        public DeleteMiscMasterCommandValidator( IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            
              _validationRules = ValidationRuleLoader.LoadValidationRules();
            _miscMasterQueryRepository = miscMasterQueryRepository;

              if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                  switch (rule.Rule)
                {
                       case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteMiscMasterCommand.Id)} {rule.Error}");
                        break;                   
                        case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _miscMasterQueryRepository.NotFoundAsync(Id))             
                           .WithName("MiscMaster Id")
                            .WithMessage($"{rule.Error}");
                            break; 
                    default:
                        
                        break;
                }
            }

        }

    }
}
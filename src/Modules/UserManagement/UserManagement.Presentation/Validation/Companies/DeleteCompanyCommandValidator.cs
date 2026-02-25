using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Application.Companies.Commands.DeleteCompany;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Companies
{
    public class DeleteCompanyCommandValidator : AbstractValidator<DeleteCompanyCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICompanyQueryRepository _companyQueryRepository;
        public DeleteCompanyCommandValidator( ICompanyQueryRepository companyQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _companyQueryRepository = companyQueryRepository;

            
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
                            .WithMessage($"{nameof(DeleteCompanyCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                         RuleFor(x => x.Id)
                      .MustAsync(async (Id, cancellation) => !await _companyQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                    default:
                        
                        break;
                }
            }
            
        }
    }
}
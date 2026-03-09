using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Application.DocumentSequence.Commands.DeleteDocumentSequence;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DocumentSequence
{
    public class DeleteDocumentSequenceCommandValidator : AbstractValidator<DeleteDocumentSequenceCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDocumentSequenceQueryRepository _queryRepository;

        public DeleteDocumentSequenceCommandValidator(IDocumentSequenceQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteDocumentSequenceCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Document Sequence {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}

using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Application.DocumentSequence.Commands.UpdateDocumentSequence;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DocumentSequence
{
    public class UpdateDocumentSequenceCommandValidator : AbstractValidator<UpdateDocumentSequenceCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDocumentSequenceQueryRepository _queryRepository;

        public UpdateDocumentSequenceCommandValidator(IDocumentSequenceQueryRepository queryRepository)
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
                        RuleFor(x => x.TransactionTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDocumentSequenceCommand.TransactionTypeId)} {rule.Error}");

                        RuleFor(x => x.FinancialYearId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDocumentSequenceCommand.FinancialYearId)} {rule.Error}");

                        RuleFor(x => x.DocNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDocumentSequenceCommand.DocNo)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.DocNo)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateDocumentSequenceCommand.DocNo)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Document Sequence {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.TransactionTypeId)
                            .MustAsync(async (typeId, ct) =>
                                await _queryRepository.TransactionTypeIdExistsAsync(typeId))
                            .WithMessage($"{nameof(UpdateDocumentSequenceCommand.TransactionTypeId)} {rule.Error}")
                            .When(x => x.TransactionTypeId > 0);

                        RuleFor(x => x.FinancialYearId)
                            .MustAsync(async (financialYearId, ct) =>
                                await _queryRepository.FinancialYearExistsAsync(financialYearId))
                            .WithMessage($"{nameof(UpdateDocumentSequenceCommand.FinancialYearId)} {rule.Error}")
                            .When(x => x.FinancialYearId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.DocNo)
                            .MustAsync(async (command, docNo, ct) =>
                                !await _queryRepository.CompositeKeyExistsAsync(command.TransactionTypeId, command.FinancialYearId, docNo, command.Id))
                            .WithMessage($"Document Sequence for this Type, Financial Year and DocNo {rule.Error}")
                            .When(x => x.TransactionTypeId > 0 && x.FinancialYearId > 0 && x.DocNo > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateDocumentSequenceCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}

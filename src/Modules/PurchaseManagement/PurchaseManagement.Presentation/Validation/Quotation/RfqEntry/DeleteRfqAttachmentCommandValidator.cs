using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.DeleteAttachment;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.Quotation.RfqEntry;

public class DeleteRfqAttachmentCommandValidator : AbstractValidator<DeleteRfqAttachmentCommand>
{
    private readonly IRfqQueryRepository _queryRepo;
    private readonly List<ValidationRule> _validationRules;

    public DeleteRfqAttachmentCommandValidator(IRfqQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
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
                    RuleFor(x => x.RfqId)
                        .GreaterThan(0)
                        .WithMessage($"RfqId {rule.Error}");

                    RuleFor(x => x.AttachmentId)
                        .GreaterThan(0)
                        .WithMessage($"AttachmentId {rule.Error}");
                    break;

                case "NotFound":
                    RuleFor(x => x)
                        .MustAsync(async (cmd, ct) =>
                            await _queryRepo.AttachmentExistsAsync(cmd.RfqId, cmd.AttachmentId, ct))
                        .WithMessage($"Attachment {rule.Error}")
                        .When(x => x.RfqId > 0 && x.AttachmentId > 0);
                    break;

                default:
                    break;
            }
        }
    }
}

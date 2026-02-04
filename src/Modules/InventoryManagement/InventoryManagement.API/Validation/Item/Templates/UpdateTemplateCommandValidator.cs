// PurchaseManagement.API/Validation/Templates/UpdateTemplateCommandValidator.cs
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.Commands.UpdateTemplate;
using FluentValidation;

namespace InventoryManagement.API.Validation.Item.Templates
{
    public sealed class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
    {
        public UpdateTemplateCommandValidator(ITemplateQueryRepository repo)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id).GreaterThan(0);

            RuleFor(x => x.TemplateName)
                .NotEmpty().MaximumLength(100)
                .MustAsync(async (cmd, name, ct) =>
                    !await repo.ExistsByNameAsync(name.Trim(), excludeId: cmd.Id, ct))
                .WithMessage("A template with the same name already exists.");

            RuleForEach(x => x.Parameters).ChildRules(param =>
            {
                param.RuleFor(p => p.Parameter).NotEmpty().MaximumLength(200);
                param.When(p => p.Numeric, () =>
                {
                    param.RuleFor(p => p.MinimumValue).NotNull();
                    param.RuleFor(p => p.MaximumValue).NotNull();
                    param.RuleFor(p => p.MaximumValue)
                        .GreaterThanOrEqualTo(p => p.MinimumValue!)
                        .WithMessage("MaximumValue must be ≥ MinimumValue.");
                });
            });
        }
    }
}

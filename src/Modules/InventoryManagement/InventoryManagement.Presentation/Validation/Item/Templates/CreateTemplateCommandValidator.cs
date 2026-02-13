// PurchaseManagement.Presentation/Validation/Templates/CreateTemplateCommandValidator.cs
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.Commands.CreateTemplate;
using FluentValidation;

namespace InventoryManagement.Presentation.Validation.Item.Templates
{
    public sealed class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
    {
        public CreateTemplateCommandValidator(ITemplateQueryRepository repo)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.TemplateName)
                .NotEmpty().MaximumLength(100)
                .MustAsync(async (name, ct) => !await repo.ExistsByNameAsync(name.Trim(), excludeId: null, ct))
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

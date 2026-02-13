// CreateItemTemplateCommandValidator.cs
using System.Linq;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using FluentValidation;

public sealed class CreateItemTemplateCommandValidator : AbstractValidator<ItemDto>
{
    public CreateItemTemplateCommandValidator()
    {
        RuleFor(x => x.ItemName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ItemGroupId).NotNull().GreaterThan(0);
        RuleFor(x => x.ItemCategoryId).NotNull().GreaterThan(0);
        RuleFor(x => x.HasVariants).Equal(true);

        RuleFor(x => x.VariantAttributes)
            .NotEmpty().WithMessage("Variant attributes are required for a template.");

        RuleForEach(x => x.VariantAttributes).ChildRules(attrs =>
        {
            attrs.RuleFor(a => a.AttributeId).GreaterThan(0);
            attrs.RuleFor(a => a.VariantBasedOn).GreaterThan(0);
            attrs.RuleFor(a => a.Order).GreaterThan(0);
        });

        RuleFor(x => x.VariantAttributes.Select(a => a.AttributeId))
           .Must(ids => ids.Distinct().Count() == ids.Count())
           .WithMessage("Duplicate attributes are not allowed.");

        RuleFor(x => x.VariantAttributes.Select(a => a.Order))
           .Must(ord => ord.Distinct().Count() == ord.Count())
           .WithMessage("Attribute order must be unique.");
    }
}

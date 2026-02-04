// PurchaseManagement.API/Validation/Templates/DeleteTemplateCommandValidator.cs
using InventoryManagement.Application.Item.Templates.Commands.DeleteTemplate;
using FluentValidation;

namespace InventoryManagement.API.Validation.Item.Templates
{
    public sealed class DeleteTemplateCommandValidator : AbstractValidator<DeleteTemplateCommand>
    {
        public DeleteTemplateCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}

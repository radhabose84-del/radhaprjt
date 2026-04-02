using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.Commands.DeleteTemplate;
using FluentValidation;

namespace InventoryManagement.Presentation.Validation.Item.Templates
{
    public sealed class DeleteTemplateCommandValidator : AbstractValidator<DeleteTemplateCommand>
    {
        private readonly ITemplateQueryRepository _queryRepo;

        public DeleteTemplateCommandValidator(ITemplateQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage($"{nameof(DeleteTemplateCommand.Id)} is required.")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => await _queryRepo.ExistsByIdAsync(id, ct))
                        .WithMessage("Inspection Template not found.");

                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => !await _queryRepo.SoftDeleteValidationAsync(id, ct))
                        .WithMessage("This master is linked with other records. You cannot delete this record.");
                });
        }
    }
}

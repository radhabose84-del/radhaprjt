using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Command.DeleteTnCTemplateMasterCommand;
using FluentValidation;

namespace PurchaseManagement.Presentation.Validation.TnCTemplateMaster
{
    public class DeleteTnCTemplateMasterValidator : AbstractValidator<DeleteTnCTemplateMasterCommand>
    {
        
        public DeleteTnCTemplateMasterValidator(ITnCTemplateMasterQueryRepository repo )
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("A valid Id is required.");

            // Must exist and not already soft-deleted
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) => await ExistsActiveAsync(repo, cmd.Id))
                .WithMessage("T&C Template not found or already deleted.");

            // Block delete when linked with child records
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => !await repo.SoftDeleteValidationAsync(id))
                .WithMessage("This master is linked with other records. You cannot delete this record.");
        }

           private static async Task<bool> ExistsActiveAsync(
            ITnCTemplateMasterQueryRepository repo, int id)
        {
            // Prefer a repo method that returns null when IsDeleted=1
            var dto = await repo.GetByIdAsync(id);
            if (dto is null) return false;           
            return dto.IsActive; 
        }
        
    }
}
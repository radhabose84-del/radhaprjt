using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Command.DeleteTnCTemplateMasterCommand;
using FluentValidation;

namespace PurchaseManagement.API.Validation.TnCTemplateMaster
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

            // If the template has been used in a transaction, do not allow delete (set Inactive instead)
            // RuleFor(x => x)
            //     .MustAsync(async (cmd, ct) => !await repo.IsUsedInTransactionsAsync(cmd.Id, ct))
            //     .WithMessage("This template is already used in transactions and cannot be deleted. Set it to Inactive instead.");
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
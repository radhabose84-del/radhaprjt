using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Command.DeleteRackMaster;
using FluentValidation;

namespace WarehouseManagement.Presentation.Validation.RackMaster
{
    public class DeleteRackMasterCommandValidator : AbstractValidator<DeleteRackMasterCommand>
    {
        
        private readonly IRackMasterCommandRepository _cmdRepo;
      
        public DeleteRackMasterCommandValidator(    IRackMasterCommandRepository cmdRepo)
        {

            _cmdRepo = cmdRepo;
             RuleFor(x => x.Id).GreaterThan(0);

        // Exists?
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => (await cmdRepo.GetByIdAsync(cmd.Id)) is not null)
            .WithMessage(cmd => $"RackMaster Id {cmd.Id} not found.");

       
       

      
        }
    }
}
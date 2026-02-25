using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Commands.UpdateService;
using FluentValidation;

namespace PurchaseManagement.Presentation.Validation.ServiceMaster
{
    public class UpdateServiceMasterCommandValidator : AbstractValidator<UpdateServiceCommand>
    {

        private readonly IServiceQueryRepository _queryRepo;

        public UpdateServiceMasterCommandValidator(IServiceQueryRepository queryRepo)
        {
          
            _queryRepo = queryRepo;


            RuleFor(x => x.Id)
               .GreaterThan(0);

            RuleFor(x => x.ServiceDescription)
                .NotEmpty().WithMessage("Service Description is required.")
                .MaximumLength(100);

            RuleFor(x => x.SacId)
                .GreaterThan(0).WithMessage("SAC is required.");

            RuleFor(x => x.UomId)
                .GreaterThan(0).WithMessage("UOM is required.");

            RuleFor(x => x.ServiceCategoryId)
                .GreaterThan(0).WithMessage("Service Category is required.");

            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    !await queryRepo.ExistsSimilarAsync(
                        cmd.SacId, cmd.UomId, cmd.ServiceDescription, cmd.Id, ct))
                .WithMessage("A similar service already exists (same SAC, UOM & Description).");

        }
        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Commands.DeleteService;
using FluentValidation;

namespace PurchaseManagement.API.Validation.ServiceMaster
{
    public class SoftDeleteServiceCommandValidator  : AbstractValidator<DeleteServiceCommand>
    {
         private readonly IServiceQueryRepository queryRepo;
        public SoftDeleteServiceCommandValidator(IServiceQueryRepository queryRepo)
        {
            // 1) Id required
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("A valid Service Id is required.");

            // 2) Ensure entity exists and is not already deleted
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) =>
                {
                    var entity = await queryRepo.GetServiceMasterByIdAsync(id);
                    return entity != null && entity.IsDeleted == PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted;
                })
                .WithMessage("Service not found or already deleted.");

                 RuleFor(x => x.Id).MustAsync(async (id, ct) =>
                 !await queryRepo.HasActiveDependenciesAsync(id, ct))
                .WithMessage("Service cannot be deleted because it is referenced by other records.");
        }

    }
}
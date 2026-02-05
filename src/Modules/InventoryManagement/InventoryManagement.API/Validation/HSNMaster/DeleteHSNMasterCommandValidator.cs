using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.DeleteHSNMaster;
using FluentValidation;

namespace InventoryManagement.API.Validation.HSNMaster
{
    public class DeleteHSNMasterCommandValidator  : AbstractValidator<DeleteHSNMasterCommand>
    {
        private readonly IHSNMasterQueryRepository _hsnMasterQueryRepository;

        public DeleteHSNMasterCommandValidator(IHSNMasterQueryRepository hsnMasterQueryRepository)
        {
            _hsnMasterQueryRepository = hsnMasterQueryRepository;

                 RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Invalid Id provided.");

               RuleFor(x => x.Id)
                .MustAsync(async (id, cancellation) =>
                    !await _hsnMasterQueryRepository.NotFoundAsync(id))
                .WithMessage("HSN Master record not found or already deleted.");


            RuleFor(x => x.Id)
                .MustAsync(async (id, cancellation) =>
                    !await _hsnMasterQueryRepository.FKColumnValidation(id))
                .WithMessage("HSN Master cannot be deleted as it is referenced by other records.");

        }
        
    }
}
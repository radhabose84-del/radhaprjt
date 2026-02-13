using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.BinMaster.Command.DeleteBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.DeleteWarehouseMaster;
using FluentValidation;

namespace WarehouseManagement.Presentation.Validation.BinMaster
{
    public class DeleteBinMasterCommandValidator : AbstractValidator<DeleteWarehouseMasterCommand>
    {
        private readonly IValidator<DeleteBinMasterCommand> _validator;
        private readonly IBinMasterCommandRepository _binMasterCommandRepository;


        public DeleteBinMasterCommandValidator(IBinMasterCommandRepository binMasterCommandRepository)
        {
            _binMasterCommandRepository = binMasterCommandRepository;
            
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id is required.")
                // Ensure the bin exists and is not already deleted
                .MustAsync(async (id, ct) =>
                {
                    var entity = await _binMasterCommandRepository.GetByIdAsync(id, ct);
                    if (entity == null) return false;

                    // adjust if IsDeleted is enum instead of bool
                    return entity.IsDeleted == WarehouseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted;
                })
                .WithMessage("Bin not found or already deleted.");

        }

    }
}
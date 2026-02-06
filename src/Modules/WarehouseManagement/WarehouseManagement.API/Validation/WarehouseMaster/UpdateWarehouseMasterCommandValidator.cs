using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.UpdateWarehouseMaster;
using FluentValidation;

namespace WarehouseManagement.API.Validation.WarehouseMaster
{
    public class UpdateWarehouseMasterCommandValidator : AbstractValidator<UpdateWarehouseMasterCommand>
    {
         private readonly IWarehouseMasterQueryRepository _warehouseMasterQueryRepository;

        public UpdateWarehouseMasterCommandValidator(IWarehouseMasterQueryRepository warehouseQuery)
        {
            _warehouseMasterQueryRepository = warehouseQuery;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");

            RuleFor(x => x.WarehouseName)
                .NotEmpty().WithMessage("Warehouse Name is required.")
                .MaximumLength(100).WithMessage("Warehouse Name cannot exceed 100 characters.")
                .MustAsync(async (cmd, name, cancellation) =>
                    !await _warehouseMasterQueryRepository.ExistsByNameAsync(name, cmd.Id))
                .WithMessage(cmd => $"Warehouse name '{cmd.WarehouseName}' already exists.");

            RuleFor(x => x.UnitId)
                .NotEmpty().WithMessage("Unit is required.");

            RuleFor(x => x.WarehouseTypeId)
                .NotEmpty().WithMessage("Warehouse Type is required.");

            RuleFor(x => x.StorageTypeId)
                .NotEmpty().WithMessage("Storage Type is required.");

            RuleFor(x => x.CapacityUOMId)
                .NotEmpty().WithMessage("Capacity UOM is required.");           

            RuleFor(x => x.Pincode)
                .MaximumLength(10).WithMessage("Pincode cannot exceed 10 characters.");

            RuleFor(x => x.AllowedItemGroupIds)
                .NotNull().WithMessage("Allowed Item Group must not be null.")
                .Must(list => list.Count > 0).WithMessage("At least one Allowed Item Group must be selected.");
        }
    }
}
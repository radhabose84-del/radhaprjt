using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.CreateWarehouseMaster;
using FluentValidation;

namespace WarehouseManagement.Presentation.Validation.WarehouseMaster
{
    public class CreateWarehouseMasterCommandValidator : AbstractValidator<CreateWarehouseMasterCommand>
    {
        private readonly IWarehouseMasterQueryRepository _warehouseMasterQueryRepository;
          
         public CreateWarehouseMasterCommandValidator(IWarehouseMasterQueryRepository warehouseMasterQueryRepository)
        {
            _warehouseMasterQueryRepository = warehouseMasterQueryRepository;

            RuleFor(x => x.WarehouseName)
                .NotEmpty().WithMessage("Warehouse Name is required.")
                .MaximumLength(100).WithMessage("Warehouse Name cannot exceed 100 characters.");

            RuleFor(x => x.WarehouseName)
                .MustAsync(async (name, _) =>
                    !await _warehouseMasterQueryRepository.ExistsByNameAsync(name))
                .WithMessage(x => $"Warehouse name '{x.WarehouseName}' already exists.")
                .When(x => !string.IsNullOrWhiteSpace(x.WarehouseName));

            RuleFor(x => x.UnitId)
                .GreaterThan(0).WithMessage("Unit Id must be greater than 0.");

            RuleFor(x => x.WarehouseTypeId)
                .GreaterThan(0).WithMessage("Warehouse Type Id must be greater than 0.");

            RuleFor(x => x.StorageTypeId)
                .GreaterThan(0).WithMessage("Storage Type Id must be greater than 0.");

            RuleFor(x => x.CapacityUOMId)
                .GreaterThan(0).WithMessage("Capacity UOM Id must be greater than 0.");

            RuleFor(x => x.AddressLine1)
                .NotEmpty().WithMessage("Address Line 1 is required.")
                .MaximumLength(200);

            RuleFor(x => x.Pincode)
                .NotEmpty().WithMessage("Pincode is required.")
                .Matches(@"^\d{6}$").WithMessage("Pincode must be 6 digits.");

            RuleFor(x => x.MaxCapacity)
                .GreaterThan(0).WithMessage("Max Capacity must be greater than 0.");
        }  
    }
}
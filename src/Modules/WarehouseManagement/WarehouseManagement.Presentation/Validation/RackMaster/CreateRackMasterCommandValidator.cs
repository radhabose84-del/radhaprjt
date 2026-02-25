using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Command.CreateRackMaster;
using FluentValidation;

namespace WarehouseManagement.Presentation.Validation.RackMaster
{
    public class CreateRackMasterCommandValidator : AbstractValidator<CreateRackMasterCommand>
    {

        private readonly IRackMasterQueryRepository _rackMasterQueryRepository;


        public CreateRackMasterCommandValidator(IRackMasterQueryRepository rackMasterQueryRepository)
        {
            _rackMasterQueryRepository = rackMasterQueryRepository;

            RuleFor(x => x.WarehouseId)
               .GreaterThan(0).WithMessage("Warehouse is required.");

            RuleFor(x => x.RackName)
               .MaximumLength(100);

            RuleFor(x => x.MaxCapacity)
              .GreaterThanOrEqualTo(0).When(x => x.MaxCapacity.HasValue);
            RuleFor(x => x.RackWidth)
               .GreaterThanOrEqualTo(0).When(x => x.RackWidth.HasValue);

            RuleFor(x => x.RackHeight)
                .GreaterThanOrEqualTo(0).When(x => x.RackHeight.HasValue);

            RuleFor(x => x.CapacityUOMId)
                .GreaterThan(0).When(x => x.CapacityUOMId.HasValue);

            RuleFor(x => x.DimensionUOMId)
                .GreaterThan(0).When(x => x.DimensionUOMId.HasValue);
                
              RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    !await _rackMasterQueryRepository.RackSlotAlreadyExistsAsync(
                        cmd.WarehouseId, cmd.FloorId, cmd.AisleId, cmd.RackLevelId, null))
                .WithMessage("A rack already exists for the same Floor/Aisle/Level in this warehouse.")
                .When(x => x.FloorId.HasValue || x.AisleId.HasValue || x.RackLevelId.HasValue);    

        }
        
    }
}
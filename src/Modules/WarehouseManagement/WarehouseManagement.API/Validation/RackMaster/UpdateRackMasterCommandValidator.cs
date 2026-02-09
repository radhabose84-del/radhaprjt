using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Command.UpdateRackMaster;
using FluentValidation;

namespace WarehouseManagement.API.Validation.RackMaster
{
    public class UpdateRackMasterCommandValidator : AbstractValidator<UpdateRackMasterCommand>
    {
        private readonly IRackMasterQueryRepository _rackMasterQueryRepository;
        public UpdateRackMasterCommandValidator(IRackMasterQueryRepository rackQuery)
        {
            _rackMasterQueryRepository = rackQuery;

            
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.WarehouseId)
                .GreaterThan(0);

            RuleFor(x => x.RackName)
                .MaximumLength(100)
                .When(x => x.RackName != null);

           
            RuleFor(x => x)
                .Custom((cmd, ctx) =>
                {
                    bool any = cmd.FloorId.HasValue || cmd.AisleId.HasValue || cmd.RackLevelId.HasValue;
                    bool all = cmd.FloorId.HasValue && cmd.AisleId.HasValue && cmd.RackLevelId.HasValue;

                    if (any && !all)
                        ctx.AddFailure("Floor/Aisle/RackLevel must all be provided together (or all omitted).");
                });

            
            RuleFor(x => x.FloorId).GreaterThan(0).When(x => x.FloorId.HasValue);
            RuleFor(x => x.AisleId).GreaterThan(0).When(x => x.AisleId.HasValue);
            RuleFor(x => x.RackLevelId).GreaterThan(0).When(x => x.RackLevelId.HasValue);

            
            RuleFor(x => x.MaxCapacity).GreaterThanOrEqualTo(0).When(x => x.MaxCapacity.HasValue);
            RuleFor(x => x.RackWidth).GreaterThanOrEqualTo(0).When(x => x.RackWidth.HasValue);
            RuleFor(x => x.RackHeight).GreaterThanOrEqualTo(0).When(x => x.RackHeight.HasValue);

           
            RuleFor(x => x.CapacityUOMId)
                .GreaterThan(0)
                .When(x => x.MaxCapacity.HasValue);

            
            RuleFor(x => x.DimensionUOMId)
                .GreaterThan(0)
                .When(x => x.RackWidth.HasValue || x.RackHeight.HasValue);

            
            RuleFor(x => x.IsActive)
                .Must(v => v == 0 || v == 1)
                .WithMessage("IsActive must be 0 or 1.");

            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    if (!(cmd.FloorId.HasValue && cmd.AisleId.HasValue && cmd.RackLevelId.HasValue))
                        return true; 

                    var exists = await _rackMasterQueryRepository.RackSlotAlreadyExistsAsync(
                        cmd.WarehouseId, cmd.FloorId, cmd.AisleId, cmd.RackLevelId, cmd.Id);

                    return !exists;
                })
                .WithMessage("A rack already exists for the same Warehouse/Floor/Aisle/Level.");
        }

        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.BinMaster.Command.UpdateBinMaster;
using FluentValidation;

namespace WarehouseManagement.API.Validation.BinMaster
{
    public class UpdateBinMasterCommandValidator     : AbstractValidator<UpdateBinMasterCommand>
    {
        public UpdateBinMasterCommandValidator()
        {
            // Id must be > 0
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id is required.");

            // Capacity must be > 0
            RuleFor(x => x.BinCapacity)
                .GreaterThan(0).WithMessage("BinCapacity must be greater than 0.");

            // UOM must be provided
            RuleFor(x => x.CapacityUOMId)
                .GreaterThan(0).WithMessage("CapacityUOMId is required.");

            // BinName optional but max 50 chars
            RuleFor(x => x.BinName)
                .MaximumLength(50)
                .WithMessage("BinName must be at most 50 characters.");

            // RackId optional but must be > 0 if present
            RuleFor(x => x.RackId)
                .GreaterThan(0).When(x => x.RackId.HasValue)
                .WithMessage("RackId, if provided, must be greater than 0.");

            // IsActive should be explicitly true/false (default is fine, just no null)
            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("IsActive status is required.");

        }
    }
}
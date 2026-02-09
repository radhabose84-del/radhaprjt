using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.BinMaster.Command.CreateBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using FluentValidation;

namespace WarehouseManagement.API.Validation.BinMaster
{
    public class CreateBinMasterCommandValidator : AbstractValidator<CreateBinMasterCommand>
    {

        private readonly IBinMasterQueryRepository _binMasterQueryRepository;

        public CreateBinMasterCommandValidator(IBinMasterQueryRepository binMasterQueryRepository)
        {
            _binMasterQueryRepository = binMasterQueryRepository;
            
              // Warehouse
            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("WarehouseId is required.");

            // Capacity
            RuleFor(x => x.BinCapacity)
                .GreaterThan(0).WithMessage("Capacity must be greater than 0.");

            // UOM
            RuleFor(x => x.CapacityUOMId)
                .GreaterThan(0).WithMessage("CapacityUOMId is required.");
                
            // RuleFor(x => x.BinCode)
            // .MaximumLength(20)
            // .When(x => !string.IsNullOrWhiteSpace(x.BinCode))
            // .WithMessage("BinCode must be at most 20 characters.")
            // .Matches("^[A-Za-z0-9_-]*$")
            // .When(x => !string.IsNullOrWhiteSpace(x.BinCode))
            // .WithMessage("BinCode can only contain letters, numbers, underscores and hyphens.")
            // .MustAsync(async (cmd, binCode, ct) =>
            // {
            //     if (string.IsNullOrWhiteSpace(binCode)) return true; // server will generate
            //     return !await _binMasterQueryRepository
            //         .ExistsByWarehouseAndCodeAsync(cmd.WarehouseId, binCode, ct);
            // })
            // .WithMessage(cmd => $"A bin with code '{cmd.BinCode}' already exists in WarehouseId={cmd.WarehouseId}."); 
            // BinCode (optional; handler will auto-generate if empty)
            // RuleFor(x => x.BinCode)
            //     .MaximumLength(20).WithMessage("BinCode must be at most 20 characters.")
            //     .Matches("^[A-Za-z0-9_-]*$")
            //         .When(x => !string.IsNullOrWhiteSpace(x.BinCode))
            //         .WithMessage("BinCode can only contain letters, numbers, underscores and hyphens.")
            //     // Uniqueness within Warehouse — only when provided
            //     .MustAsync(async (cmd, binCode, ct) =>
            //     {
            //         if (string.IsNullOrWhiteSpace(binCode))
            //             return true; // empty => allowed; will be auto-generated in handler
            //         return !await _binMasterQueryRepository
            //             .ExistsByWarehouseAndCodeAsync(cmd.WarehouseId, binCode, ct);
            //     })
            //     .WithMessage(cmd =>
            //         $"A bin with code '{cmd.BinCode}' already exists in WarehouseId={cmd.WarehouseId}.");

            // BinName (optional)
            RuleFor(x => x.BinName)
                .MaximumLength(50).WithMessage("BinName must be at most 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.BinName));

            // RackId (optional, if present must be > 0)
            RuleFor(x => x.RackId)
                .GreaterThan(0).When(x => x.RackId.HasValue)
                .WithMessage("RackId, if provided, must be greater than 0.");

        }

    }
}
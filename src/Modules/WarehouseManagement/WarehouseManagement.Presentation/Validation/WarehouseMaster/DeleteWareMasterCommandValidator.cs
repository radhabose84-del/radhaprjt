using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.DeleteWarehouseMaster;
using FluentValidation;

namespace WarehouseManagement.Presentation.Validation.WarehouseMaster
{
    public class DeleteWareMasterCommandValidator : AbstractValidator<DeleteWarehouseMasterCommand>
    {

        private readonly IValidator<DeleteWarehouseMasterCommand> _validator;

        private readonly IWarehouseMasterCommandRepository _warehouseMasterCommandRepository;



        public DeleteWareMasterCommandValidator(IWarehouseMasterCommandRepository warehouseMasterCommandRepository)
        {

            _warehouseMasterCommandRepository = warehouseMasterCommandRepository;
            RuleFor(x => x.Id).GreaterThan(0);
             
             RuleFor(x => x)
            .MustAsync(async (cmd, ct) => (await _warehouseMasterCommandRepository.GetByIdAsync(cmd.Id)) is not null)
            .WithMessage(cmd => $"WarehouseMaster Id {cmd.Id} not found.");
          
        }

    }
}
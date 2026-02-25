using MaintenanceManagement.Application.Common.Interfaces.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.Command.UpdateMachineSpecfication;
using FluentValidation;

namespace MaintenanceManagement.Presentation.Validation.MachineSpecification
{
    public class UpdateMachineSpecCommandValidator : AbstractValidator<UpdateMachineSpecficationCommand>
    {

      
        public UpdateMachineSpecCommandValidator(IMachineSpecificationQueryRepository repository)
        {
            RuleFor(x => x.Specifications)
                .NotNull().WithMessage("Specification list is required.")
                .NotEmpty().WithMessage("Specification list cannot be empty.");

            RuleForEach(x => x.Specifications).SetValidator(new MachineSpecificationUpdateDtoValidator(repository));
        }
         public class MachineSpecificationUpdateDtoValidator : AbstractValidator<MachineSpecificationUpdateDto>
        {
            public MachineSpecificationUpdateDtoValidator(IMachineSpecificationQueryRepository repository)
            {

                RuleFor(x => x.SpecificationId)
                    .NotEmpty().WithMessage("SpecificationId is required.")
                    .GreaterThan(0).WithMessage("SpecificationId must be greater than 0.");

                RuleFor(x => x.SpecificationValue)
                    .NotEmpty().WithMessage("SpecificationValue is required.");

                RuleFor(x => x.MachineId)
                    .NotEmpty().WithMessage("MachineId is required.")
                    .GreaterThan(0).WithMessage("MachineId must be greater than 0.");

                RuleFor(x => x.MachineId)
                 .GreaterThan(0).WithMessage("MachineId is required.")
                 .MustAsync(async (machineId, cancellation) =>
                 {
                     var specs = await repository.GetByIdAsync(machineId);
                     return specs != null && specs.Any();
                 })
                 .WithMessage("Machine with given ID does not exist or has no specifications.");
            
              
        }
    }
    }
}
using MaintenanceManagement.Application.Common.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.Command.CreateMachineSpecfication;
using FluentValidation;

namespace MaintenanceManagement.API.Validation.MachineSpecification
{
    public class CreateMachineSpecCommandValidator : AbstractValidator<CreateMachineSpecficationCommand>
    {
        public CreateMachineSpecCommandValidator(IMachineSpecificationCommandRepository repository)
        {
            RuleFor(x => x.Specifications)
                .NotNull().WithMessage("Specification list is required.")
                .NotEmpty().WithMessage("Specification list cannot be empty.");

            RuleForEach(x => x.Specifications).SetValidator(new MachineSpecificationCreateDtoValidator(repository));
        }
    }

    public class MachineSpecificationCreateDtoValidator : AbstractValidator<MachineSpecificationCreateDto>
    {
        public MachineSpecificationCreateDtoValidator(IMachineSpecificationCommandRepository repository)
        {
            RuleFor(x => x.SpecificationId)
                .NotEmpty().WithMessage("SpecificationId is required.")
                .GreaterThan(0).WithMessage("SpecificationId must be greater than 0.");

            RuleFor(x => x.SpecificationValue)
                .NotEmpty().WithMessage("SpecificationValue is required.")
                .Must(value =>
                     {
                        if (!decimal.TryParse(value, out var number))
                        return false;

                    return number > 0; 
              }).WithMessage("SpecificationValue must be greater than 0.");

            RuleFor(x => x.MachineId)
                .NotEmpty().WithMessage("MachineId is required.")
                .GreaterThan(0).WithMessage("MachineId must be greater than 0.");

            RuleFor(x => x)
                .MustAsync(async (dto, cancellation) =>
                    !await repository.IsDuplicateSpecificationAsync(dto.MachineId, dto.SpecificationId))
                .WithMessage("This SpecificationId already exists for the MachineId.");
        }
    }
}

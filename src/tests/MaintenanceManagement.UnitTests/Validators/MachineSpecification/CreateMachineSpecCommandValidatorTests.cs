using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.IMachineSpecification;
using MaintenanceManagement.Application.Common.Interfaces.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.Command.CreateMachineSpecfication;
using MaintenanceManagement.Application.MachineSpecification.DeleteMachineSpecfication;
using MaintenanceManagement.Application.MachineSpecification.Command.UpdateMachineSpecfication;
using MaintenanceManagement.Presentation.Validation.MachineSpecification;

namespace MaintenanceManagement.UnitTests.Validators.MachineSpecification
{
    public sealed class CreateMachineSpecCommandValidatorTests
    {
        private readonly Mock<IMachineSpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_NullSpecifications_FailsValidation()
        {
            var validator = new CreateMachineSpecCommandValidator(_mockCommandRepo.Object);
            var command = new CreateMachineSpecficationCommand { Specifications = null };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptySpecifications_FailsValidation()
        {
            var validator = new CreateMachineSpecCommandValidator(_mockCommandRepo.Object);
            var command = new CreateMachineSpecficationCommand { Specifications = new() };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }

    public sealed class DeleteMachineSpecCommandValidatorTests
    {
        private readonly Mock<IMachineSpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var validator = new DeleteMachineSpecCommandValidator(_mockQueryRepo.Object);
            var result = await validator.TestValidateAsync(new DeleteMachineSpecficationCommand { Id = 0 });
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }

    public sealed class UpdateMachineSpecCommandValidatorTests
    {
        private readonly Mock<IMachineSpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_NullSpecifications_FailsValidation()
        {
            var validator = new UpdateMachineSpecCommandValidator(_mockQueryRepo.Object);
            var command = new UpdateMachineSpecficationCommand { Specifications = null };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }
}

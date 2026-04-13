using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.Command.UpdateMachineSpecfication;
using MaintenanceManagement.Presentation.Validation.MachineSpecification;

namespace MaintenanceManagement.UnitTests.Validators.MachineSpecification
{
    public sealed class UpdateMachineSpecCommandValidatorBatchATests
    {
        private readonly Mock<IMachineSpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateMachineSpecCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public void Constructor_WithValidRepo_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_EmptySpecifications_FailsValidation()
        {
            var command = new UpdateMachineSpecficationCommand { Specifications = new() };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NullSpecifications_HasErrorForSpecifications()
        {
            var command = new UpdateMachineSpecficationCommand { Specifications = null };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Specifications);
        }

        [Fact]
        public async Task Validate_SpecificationWithZeroIds_FailsValidation()
        {
            var command = new UpdateMachineSpecficationCommand
            {
                Specifications = new List<MachineSpecificationUpdateDto>
                {
                    new() { SpecificationId = 0, MachineId = 0, SpecificationValue = "" }
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

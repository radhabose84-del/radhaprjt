using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.DeleteMachineSpecfication;
using MaintenanceManagement.Presentation.Validation.MachineSpecification;

namespace MaintenanceManagement.UnitTests.Validators.MachineSpecification
{
    public sealed class DeleteMachineSpecCommandValidatorBatchATests
    {
        private readonly Mock<IMachineSpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteMachineSpecCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public void Constructor_WithValidRepo_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_ZeroId_HasErrorForId()
        {
            var result = await CreateValidator().TestValidateAsync(
                new DeleteMachineSpecficationCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ZeroId_ErrorsNotEmpty()
        {
            var result = await CreateValidator().TestValidateAsync(
                new DeleteMachineSpecficationCommand { Id = 0 });

            result.Errors.Should().NotBeEmpty();
        }
    }
}

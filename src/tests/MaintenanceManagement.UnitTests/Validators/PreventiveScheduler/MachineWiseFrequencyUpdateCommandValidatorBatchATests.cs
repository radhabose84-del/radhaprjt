using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MachineWiseFrequencyUpdate;
using MaintenanceManagement.Presentation.Validation.PreventiveSchedulers;

namespace MaintenanceManagement.UnitTests.Validators.PreventiveScheduler
{
    public sealed class MachineWiseFrequencyUpdateCommandValidatorBatchATests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);

        private MachineWiseFrequencyUpdateCommandValidator CreateValidator() =>
            new(_mockQuery.Object);

        [Fact]
        public void Constructor_WithValidQuery_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new MachineWiseFrequencyUpdateCommand { Id = 0, FrequencyInterval = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroId_HasErrorForId()
        {
            var command = new MachineWiseFrequencyUpdateCommand { Id = 0, FrequencyInterval = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ZeroFrequencyInterval_FailsValidation()
        {
            var command = new MachineWiseFrequencyUpdateCommand { Id = 1, FrequencyInterval = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ActiveWithDefaultDate_FailsValidation()
        {
            var command = new MachineWiseFrequencyUpdateCommand
            {
                Id = 1,
                FrequencyInterval = 1,
                IsActive = 1,
                LastMaintenanceActivityDate = DateOnly.MinValue
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ActiveInActivePreventive;
using MaintenanceManagement.Presentation.Validation.PreventiveSchedulers;

namespace MaintenanceManagement.UnitTests.Validators.PreventiveScheduler
{
    public sealed class UpdateActiveStatusCommandValidatorBatchATests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);

        private UpdateActiveStatusCommandValidator CreateValidator() =>
            new(_mockQuery.Object);

        [Fact]
        public void Constructor_WithValidQuery_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_ZeroId_HasErrorForId()
        {
            var command = new ActiveInActivePreventiveCommand { Id = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ZeroId_ErrorsNotEmpty()
        {
            var command = new ActiveInActivePreventiveCommand { Id = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

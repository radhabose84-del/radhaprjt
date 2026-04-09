using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.DeletePreventiveScheduler;
using MaintenanceManagement.Presentation.Validation.PreventiveSchedulers;

namespace MaintenanceManagement.UnitTests.Validators.PreventiveScheduler
{
    public sealed class DeletePreventiveSchedulerCommandValidatorTests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQueryRepo = new(MockBehavior.Loose);

        private DeletePreventiveSchedulerCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UpdateValidation(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeletePreventiveSchedulerCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeletePreventiveSchedulerCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.UpdateValidation(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeletePreventiveSchedulerCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }
    }
}

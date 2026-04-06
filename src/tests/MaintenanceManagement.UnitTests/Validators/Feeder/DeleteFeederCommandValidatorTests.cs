using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.DeleteFeeder;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.Power.Feeder;
using Xunit;

namespace MaintenanceManagement.UnitTests.Validators.Feeder
{
    public sealed class DeleteFeederCommandValidatorTests
    {
        private readonly Mock<IFeederQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private DeleteFeederCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAllAsyncMocks(int id, bool notFound = true, bool softDeleteBlocked = false)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(notFound);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(softDeleteBlocked);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupAllAsyncMocks(1);
            var result = await CreateValidator().TestValidateAsync(new DeleteFeederCommand { Id = 1 });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteFeederCommand { Id = 0 });
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupAllAsyncMocks(99, notFound: false, softDeleteBlocked: false);
            var result = await CreateValidator().TestValidateAsync(new DeleteFeederCommand { Id = 99 });
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_LinkedRecord_FailsValidation()
        {
            SetupAllAsyncMocks(1, notFound: true, softDeleteBlocked: true);
            var result = await CreateValidator().TestValidateAsync(new DeleteFeederCommand { Id = 1 });
            result.Errors.Should().NotBeEmpty();
        }
    }
}

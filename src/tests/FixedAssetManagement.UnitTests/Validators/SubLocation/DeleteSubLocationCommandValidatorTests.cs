using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.Location.Command.DeleteAubLocation;
using FAM.Presentation.Validation.SubLocation;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.SubLocation
{
    public sealed class DeleteSubLocationCommandValidatorTests
    {
        private readonly Mock<ISubLocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteSubLocationCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteSubLocationCommand { Id = 1 };
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteSubLocationCommand { Id = 0 };
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_LinkedRecords_FailsValidation()
        {
            var command = new DeleteSubLocationCommand { Id = 1 };
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

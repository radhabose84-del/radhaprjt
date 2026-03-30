using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using InventoryManagement.Presentation.Validation.MiscTypeMaster;

namespace InventoryManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class DeleteMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        public DeleteMiscTypeMasterCommandValidatorTests()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(It.IsAny<int>()))
                .ReturnsAsync(false);
        }

        private DeleteMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            var command = new DeleteMiscTypeMasterCommand { Id = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteMiscTypeMasterCommand { Id = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            var command = new DeleteMiscTypeMasterCommand { Id = 999 };

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(999))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_HasDependencies_FailsValidation()
        {
            var command = new DeleteMiscTypeMasterCommand { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

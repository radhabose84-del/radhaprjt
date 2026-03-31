using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Presentation.Validation.HSNMaster;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.HSNMaster
{
    public sealed class DeleteHSNMasterCommandValidatorTests
    {
        private readonly Mock<IHSNMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        public DeleteHSNMasterCommandValidatorTests()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.FKColumnValidation(It.IsAny<int>()))
                .ReturnsAsync(false);
        }

        private DeleteHSNMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            var command = HSNMasterBuilders.ValidDeleteCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = HSNMasterBuilders.ValidDeleteCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            var command = HSNMasterBuilders.ValidDeleteCommand(id: 999);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(999))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_HasFKReferences_FailsValidation()
        {
            var command = HSNMasterBuilders.ValidDeleteCommand();

            _mockQueryRepo
                .Setup(r => r.FKColumnValidation(command.Id))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

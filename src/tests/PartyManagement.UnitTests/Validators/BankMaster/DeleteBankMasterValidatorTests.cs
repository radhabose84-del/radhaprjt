using FluentValidation.TestHelper;
using PartyManagement.Application.BankMaster.Command.Delete;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using PartyManagement.Presentation.Validation.BankMaster;

namespace PartyManagement.UnitTests.Validators.BankMaster
{
    public sealed class DeleteBankMasterValidatorTests
    {
        private readonly Mock<IBankMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteBankMasterValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks(1);
            var command = new DeleteBankMasterCommand(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteBankMasterCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(999))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(999))
                .ReturnsAsync(false);
            var command = new DeleteBankMasterCommand(999);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_LinkedRecords_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(1))
                .ReturnsAsync(true);
            var command = new DeleteBankMasterCommand(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("linked"));
        }
    }
}

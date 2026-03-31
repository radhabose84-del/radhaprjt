using FluentValidation.TestHelper;
using PartyManagement.Application.BankMaster;
using PartyManagement.Application.BankMaster.Command.Update;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using PartyManagement.Presentation.Validation.BankMaster;

namespace PartyManagement.UnitTests.Validators.BankMaster
{
    public sealed class UpdateBankMasterValidatorTests
    {
        private readonly Mock<IBankMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateBankMasterValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupNoDuplicate()
        {
            _mockQueryRepo
                .Setup(r => r.ExistsByBankCodeAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupNoDuplicate();
            var command = new UpdateBankMasterCommand(new UpdateBankMasterDto(1, "Updated Bank", 1));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            SetupNoDuplicate();
            var command = new UpdateBankMasterCommand(new UpdateBankMasterDto(0, "Some Bank", 1));

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyBankName_FailsValidation(string? bankName)
        {
            var command = new UpdateBankMasterCommand(new UpdateBankMasterDto(1, bankName!, 1));

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateBankName_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.ExistsByBankCodeAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new UpdateBankMasterCommand(new UpdateBankMasterDto(1, "Duplicate Bank", 1));

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

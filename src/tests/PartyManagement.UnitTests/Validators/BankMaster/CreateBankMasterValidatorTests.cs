using FluentValidation.TestHelper;
using PartyManagement.Application.BankMaster;
using PartyManagement.Application.BankMaster.Command.Create;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using PartyManagement.Presentation.Validation.BankMaster;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.BankMaster
{
    public sealed class CreateBankMasterValidatorTests
    {
        private readonly Mock<IBankMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateBankMasterValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupNoDuplicate(string bankName = "ICICI Bank")
        {
            _mockQueryRepo
                .Setup(r => r.ExistsByBankCodeAsync(bankName, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupNoDuplicate();
            var command = BankMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyBankName_FailsValidation(string? bankName)
        {
            var command = new CreateBankMasterCommand(new CreateBankMasterDto(bankName!));

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateBankName_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.ExistsByBankCodeAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = BankMasterBuilders.ValidCreateCommand("Duplicate Bank");

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

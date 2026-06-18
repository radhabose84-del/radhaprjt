using FluentValidation.TestHelper;
using Moq;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Presentation.Validation;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PortMaster
{
    public sealed class CreatePortMasterValidatorTests
    {
        // CreatePortMasterValidator now enforces PortCode uniqueness via IPortMasterQueryRepository.
        // Mock it to "not exists" so the valid path passes; format/empty/zero tests fail on their own rules.
        private static CreatePortMasterValidator CreateValidator()
        {
            var repo = new Mock<IPortMasterQueryRepository>();
            repo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            return new CreatePortMasterValidator(repo.Object);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = PortMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPortCode_FailsValidation(string? portCode)
        {
            var command = PortMasterBuilders.ValidCreateCommand(portCode: portCode!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PortCode);
        }

        [Fact]
        public async Task Validate_InvalidPortCodeFormat_FailsValidation()
        {
            var command = PortMasterBuilders.ValidCreateCommand(portCode: "port code!");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PortCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPortName_FailsValidation(string? portName)
        {
            var command = PortMasterBuilders.ValidCreateCommand(portName: portName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PortName);
        }

        [Fact]
        public async Task Validate_ZeroCountryId_FailsValidation()
        {
            var command = PortMasterBuilders.ValidCreateCommand(countryId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CountryId);
        }

        [Fact]
        public async Task Validate_ZeroPortTypeId_FailsValidation()
        {
            var command = PortMasterBuilders.ValidCreateCommand(portTypeId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PortTypeId);
        }
    }
}

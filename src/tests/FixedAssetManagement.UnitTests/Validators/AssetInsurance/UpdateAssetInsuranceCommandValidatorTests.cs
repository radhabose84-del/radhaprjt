using FAM.Application.AssetMaster.AssetInsurance.Commands.UpdateAssetInsurance;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FAM.Presentation.Validation.AssetMaster.AssetInsurance;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetInsurance
{
    public sealed class UpdateAssetInsuranceCommandValidatorTests
    {
        private readonly Mock<IAssetInsuranceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateAssetInsuranceCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string policyNo = "POL001", int id = 1, int assetId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(policyNo, id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.ActiveInsuranceValidation(assetId, id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetInsuranceBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks(command.PolicyNo!, command.Id, command.AssetId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroAssetId_FailsValidation()
        {
            var command = AssetInsuranceBuilders.ValidUpdateCommand();
            command.AssetId = 0;
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.ActiveInsuranceValidation(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPolicyNo_FailsValidation(string? policyNo)
        {
            var command = AssetInsuranceBuilders.ValidUpdateCommand();
            command.PolicyNo = policyNo;
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.ActiveInsuranceValidation(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicatePolicyNo_FailsValidation()
        {
            var command = AssetInsuranceBuilders.ValidUpdateCommand();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.PolicyNo!, command.Id))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.ActiveInsuranceValidation(command.AssetId, command.Id))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

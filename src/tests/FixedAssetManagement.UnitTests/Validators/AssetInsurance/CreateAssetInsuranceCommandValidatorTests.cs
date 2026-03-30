using FAM.Application.AssetMaster.AssetInsurance.Commands.CreateAssetInsurance;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FAM.Presentation.Validation.AssetMaster.AssetInsurance;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetInsurance
{
    public sealed class CreateAssetInsuranceCommandValidatorTests
    {
        private readonly Mock<IAssetInsuranceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateAssetInsuranceCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string policyNo = "POL001", int assetId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(policyNo, null))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.ActiveInsuranceValidation(assetId, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetInsuranceBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.PolicyNo!, command.AssetId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroAssetId_FailsValidation()
        {
            var command = AssetInsuranceBuilders.ValidCreateCommand(assetId: 0);
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.ActiveInsuranceValidation(0, null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPolicyNo_FailsValidation(string? policyNo)
        {
            var command = AssetInsuranceBuilders.ValidCreateCommand();
            command.PolicyNo = policyNo;
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.ActiveInsuranceValidation(It.IsAny<int>(), null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicatePolicyNo_FailsValidation()
        {
            var command = AssetInsuranceBuilders.ValidCreateCommand();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.PolicyNo!, null))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.ActiveInsuranceValidation(command.AssetId, null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

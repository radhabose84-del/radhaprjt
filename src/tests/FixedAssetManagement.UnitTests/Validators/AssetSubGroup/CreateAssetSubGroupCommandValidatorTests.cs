using FAM.Application.AssetSubGroup.Command.CreateAssetSubGroup;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Presentation.Validation.AssetSubGroup;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetSubGroup
{
    public sealed class CreateAssetSubGroupCommandValidatorTests
    {
        private readonly MaxLengthProvider _maxLengthProvider = new(null!);
        private readonly Mock<IAssetSubGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);

        private CreateAssetSubGroupCommandValidator CreateValidator() =>
            new(_maxLengthProvider, _mockCommandRepo.Object);

        private void SetupAllAsyncMocks(int groupId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.ExistsAsync(groupId))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetSubGroupBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.GroupId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = AssetSubGroupBuilders.ValidCreateCommand(code: code!);
            // ExistsAsync is outside the foreach — always runs when GroupId > 0
            SetupAllAsyncMocks(command.GroupId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptySubGroupName_FailsValidation(string? subGroupName)
        {
            var command = AssetSubGroupBuilders.ValidCreateCommand(subGroupName: subGroupName!);
            // ExistsAsync is outside the foreach — always runs when GroupId > 0
            SetupAllAsyncMocks(command.GroupId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroGroupId_FailsValidation()
        {
            var command = AssetSubGroupBuilders.ValidCreateCommand(groupId: 0);
            // FluentValidation 11 runs all rules in parallel — setup ExistsAsync for groupId=0
            _mockCommandRepo
                .Setup(r => r.ExistsAsync(0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

using FluentValidation.TestHelper;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.CreateMiscMaster;
using ProjectManagement.Presentation.Validation.Common;
using ProjectManagement.Presentation.Validation.MiscMaster;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Validators.MiscMaster
{
    public sealed class CreateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLengthProvider;

        public CreateMiscMasterCommandValidatorTests()
        {
            _mockMaxLengthProvider = new Mock<MaxLengthProvider>(MockBehavior.Strict, new object[] { null! });
            _mockMaxLengthProvider
                .Setup(m => m.GetMaxLength<ProjectManagement.Domain.Entities.MiscMaster>("Code"))
                .Returns(50);
            _mockMaxLengthProvider
                .Setup(m => m.GetMaxLength<ProjectManagement.Domain.Entities.MiscMaster>("Description"))
                .Returns(250);
        }

        private CreateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLengthProvider.Object);

        private void SetupAllAsyncMocks()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string?>(), It.IsAny<int>(), null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = MiscMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            SetupAllAsyncMocks();
            var command = MiscMasterBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            SetupAllAsyncMocks();
            var command = MiscMasterBuilders.ValidCreateCommand(description: description);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("MSC001", 1, null))
                .ReturnsAsync(true);
            var command = MiscMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_CodeExceedsMaxLength_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = MiscMasterBuilders.ValidCreateCommand(code: new string('A', 51));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_DescriptionExceedsMaxLength_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = MiscMasterBuilders.ValidCreateCommand(description: new string('A', 251));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }
}

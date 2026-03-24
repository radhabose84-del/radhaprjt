using FluentValidation.TestHelper;
using ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using ProjectManagement.Presentation.Validation.Common;
using ProjectManagement.Presentation.Validation.MiscTypeMaster;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class CreateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLengthProvider;

        public CreateMiscTypeMasterCommandValidatorTests()
        {
            _mockMaxLengthProvider = new Mock<MaxLengthProvider>(MockBehavior.Strict, new object[] { null! });
            _mockMaxLengthProvider
                .Setup(m => m.GetMaxLength<ProjectManagement.Domain.Entities.MiscTypeMaster>("MiscTypeCode"))
                .Returns(50);
            _mockMaxLengthProvider
                .Setup(m => m.GetMaxLength<ProjectManagement.Domain.Entities.MiscTypeMaster>("Description"))
                .Returns(250);
        }

        private CreateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLengthProvider.Object);

        private void SetupAllAsyncMocks(string? code = "MTT001")
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.MiscTypeCode);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyMiscTypeCode_FailsValidation(string? code)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(miscTypeCode: code);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(miscTypeCode: "EXIST001");
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("EXIST001", null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task Validate_CodeExceedsMaxLength_FailsValidation()
        {
            var longCode = new string('A', 51);
            var command = MiscTypeMasterBuilders.ValidCreateCommand(miscTypeCode: longCode);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }
    }
}

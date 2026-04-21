using Contracts.Interfaces;
using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Commands.CreateTransactionTypeMaster;
using FinanceManagement.Presentation.Validation.TransactionTypeMaster;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.TransactionTypeMaster
{
    public sealed class CreateTransactionTypeMasterCommandValidatorTests
    {
        private readonly Mock<ITransactionTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateTransactionTypeMasterCommandValidator CreateValidator()
        {
            _mockIp.Setup(x => x.GetUnitId()).Returns(1);
            return new CreateTransactionTypeMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object,
                _mockIp.Object);
        }

        private void SetupAllAsyncMocks(
            string typeName = "TestType",
            string shortName = "TT",
            int unitId = 1,
            int moduleId = 1,
            int menuId = 1)
        {
            _mockIp.Setup(x => x.GetUnitId()).Returns(unitId);
            _mockQueryRepo.Setup(r => r.TypeNameExistsAsync(typeName, unitId, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ShortNameExistsAsync(shortName, unitId, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ModuleExistsAsync(moduleId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MenuExistsAsync(menuId)).ReturnsAsync(true);
        }

        private static CreateTransactionTypeMasterCommand ValidCommand() =>
            new()
            {
                TypeName = "TestType",
                ShortName = "TT",
                Description = "Test Description",
                ModuleId = 1,
                MenuId = 1
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyTypeName_FailsValidation(string? typeName)
        {
            var command = ValidCommand();
            command.TypeName = typeName;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TypeName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyShortName_FailsValidation(string? shortName)
        {
            var command = ValidCommand();
            command.ShortName = shortName;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ShortName);
        }

        [Fact]
        public async Task Validate_ZeroModuleId_FailsValidation()
        {
            var command = ValidCommand();
            command.ModuleId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ModuleId);
        }

        [Fact]
        public async Task Validate_ZeroMenuId_FailsValidation()
        {
            var command = ValidCommand();
            command.MenuId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MenuId);
        }

        [Fact]
        public async Task Validate_DuplicateTypeName_FailsValidation()
        {
            var command = ValidCommand();
            _mockIp.Setup(x => x.GetUnitId()).Returns(1);
            _mockQueryRepo.Setup(r => r.TypeNameExistsAsync("TestType", 1, null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ShortNameExistsAsync("TT", 1, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ModuleExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MenuExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TypeName);
        }

        [Fact]
        public async Task Validate_DuplicateShortName_FailsValidation()
        {
            var command = ValidCommand();
            _mockIp.Setup(x => x.GetUnitId()).Returns(1);
            _mockQueryRepo.Setup(r => r.TypeNameExistsAsync("TestType", 1, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ShortNameExistsAsync("TT", 1, null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ModuleExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MenuExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ShortName);
        }

        [Fact]
        public async Task Validate_InvalidModuleId_FailsValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.ModuleExistsAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ModuleId);
        }

        [Fact]
        public async Task Validate_InvalidMenuId_FailsValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.MenuExistsAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MenuId);
        }

        [Fact]
        public async Task Validate_TypeNameExceedsMaxLength_FailsValidation()
        {
            var command = ValidCommand();
            command.TypeName = new string('A', 101);
            SetupAllAsyncMocks(command.TypeName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TypeName);
        }

        [Fact]
        public async Task Validate_ShortNameExceedsMaxLength_FailsValidation()
        {
            var command = ValidCommand();
            command.ShortName = new string('A', 51);
            SetupAllAsyncMocks(shortName: command.ShortName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ShortName);
        }

        [Fact]
        public async Task Validate_UsesUnitIdFromToken_ForAlreadyExistsCheck()
        {
            var command = ValidCommand();
            _mockIp.Setup(x => x.GetUnitId()).Returns(9);
            _mockQueryRepo.Setup(r => r.TypeNameExistsAsync("TestType", 9, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ShortNameExistsAsync("TT", 9, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ModuleExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MenuExistsAsync(1)).ReturnsAsync(true);

            await CreateValidator().TestValidateAsync(command);

            _mockQueryRepo.Verify(r => r.TypeNameExistsAsync("TestType", 9, null), Times.AtLeastOnce);
            _mockQueryRepo.Verify(r => r.ShortNameExistsAsync("TT", 9, null), Times.AtLeastOnce);
        }
    }
}

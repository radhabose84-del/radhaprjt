using Contracts.Interfaces.Lookups.Users;
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
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private CreateTransactionTypeMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockUnitLookup.Object);

        private void SetupAllAsyncMocks(
            string typeName = "TestType",
            string shortName = "TT",
            int unitId = 1,
            int moduleId = 1,
            int menuId = 1)
        {
            _mockQueryRepo.Setup(r => r.TypeNameExistsAsync(typeName, unitId, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ShortNameExistsAsync(shortName, unitId, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(unitId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ModuleExistsAsync(moduleId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MenuExistsAsync(menuId)).ReturnsAsync(true);

            _mockUnitLookup.Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>
                {
                    new() { UnitId = unitId, UnitName = $"Unit{unitId}" }
                });
        }

        private static CreateTransactionTypeMasterCommand ValidCommand() =>
            new()
            {
                TypeName = "TestType",
                ShortName = "TT",
                Description = "Test Description",
                UnitId = 1,
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
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.UnitId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
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
            _mockQueryRepo.Setup(r => r.TypeNameExistsAsync("TestType", 1, null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ShortNameExistsAsync("TT", 1, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ModuleExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MenuExistsAsync(1)).ReturnsAsync(true);
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>
                {
                    new() { UnitId = 1, UnitName = "Unit1" }
                });

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TypeName);
        }

        [Fact]
        public async Task Validate_DuplicateShortName_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo.Setup(r => r.TypeNameExistsAsync("TestType", 1, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ShortNameExistsAsync("TT", 1, null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ModuleExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MenuExistsAsync(1)).ReturnsAsync(true);
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>
                {
                    new() { UnitId = 1, UnitName = "Unit1" }
                });

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ShortName);
        }

        [Fact]
        public async Task Validate_InvalidUnitId_FailsValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
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
    }
}

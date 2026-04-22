using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Commands.CreateCommissionSplit;
using SalesManagement.Presentation.Validation.CommissionSplit;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.CommissionSplit
{
    public sealed class CreateCommissionSplitCommandValidatorTests
    {
        private readonly Mock<ICommissionSplitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateCommissionSplitCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static CreateCommissionSplitCommand ValidCommand(
            string? splitName = "TestSplit",
            List<CommissionSplitDetailItem>? details = null)
        {
            details ??= new List<CommissionSplitDetailItem>
            {
                new() { RoleId = 1, ShareTypeId = 10, ShareValue = 60 },
                new() { RoleId = 2, ShareTypeId = 10, ShareValue = 40 }
            };

            return new CreateCommissionSplitCommand
            {
                SplitName = splitName,
                Details = details
            };
        }

        private void SetupAllAsyncMocksPass()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetMiscMasterCodeAsync(It.IsAny<int>())).ReturnsAsync("PERCENTAGE");
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── SplitName Rules ───────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SplitName_Empty_FailsValidation(string? name)
        {
            var command = ValidCommand(splitName: name);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SplitName);
        }

        [Fact]
        public async Task SplitName_TooLong_FailsValidation()
        {
            var longName = new string('A', 101);
            var command = ValidCommand(splitName: longName);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SplitName);
        }

        [Fact]
        public async Task SplitName_AlreadyExists_FailsValidation()
        {
            var command = ValidCommand(splitName: "EXIST001");
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("EXIST001", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SplitName);
        }

        // ── Detail Rules ──────────────────────────────────────────────────────

        [Fact]
        public async Task Details_NotExactlyTwo_FailsValidation()
        {
            var command = ValidCommand(details: new List<CommissionSplitDetailItem>
            {
                new() { RoleId = 1, ShareTypeId = 10, ShareValue = 100 }
            });
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Details)
                  .WithErrorMessage("Exactly two split configuration rows are required (Agent and Sub-Agent).");
        }

        [Fact]
        public async Task Details_DuplicateRoles_FailsValidation()
        {
            var command = ValidCommand(details: new List<CommissionSplitDetailItem>
            {
                new() { RoleId = 1, ShareTypeId = 10, ShareValue = 50 },
                new() { RoleId = 1, ShareTypeId = 10, ShareValue = 50 }
            });
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Details)
                  .WithErrorMessage("Duplicate roles are not allowed.");
        }

        [Fact]
        public async Task Details_DifferentShareTypes_FailsValidation()
        {
            var command = ValidCommand(details: new List<CommissionSplitDetailItem>
            {
                new() { RoleId = 1, ShareTypeId = 10, ShareValue = 50 },
                new() { RoleId = 2, ShareTypeId = 20, ShareValue = 50 }
            });
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Details)
                  .WithErrorMessage("All rows in a split must use the same Share Type.");
        }

        [Fact]
        public async Task Details_PercentageSumNot100_FailsValidation()
        {
            var command = ValidCommand(details: new List<CommissionSplitDetailItem>
            {
                new() { RoleId = 1, ShareTypeId = 10, ShareValue = 30 },
                new() { RoleId = 2, ShareTypeId = 10, ShareValue = 30 }
            });
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Details_RoleIdZero_FailsGreaterThan()
        {
            var command = ValidCommand(details: new List<CommissionSplitDetailItem>
            {
                new() { RoleId = 0, ShareTypeId = 10, ShareValue = 60 },
                new() { RoleId = 2, ShareTypeId = 10, ShareValue = 40 }
            });
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Details_ShareValueZero_FailsGreaterThan()
        {
            var command = ValidCommand(details: new List<CommissionSplitDetailItem>
            {
                new() { RoleId = 1, ShareTypeId = 10, ShareValue = 0 },
                new() { RoleId = 2, ShareTypeId = 10, ShareValue = 100 }
            });
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Details_MiscMasterNotFound_FailsFKValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}

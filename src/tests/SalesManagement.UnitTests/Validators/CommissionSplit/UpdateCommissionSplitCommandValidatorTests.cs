using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Commands.CreateCommissionSplit;
using SalesManagement.Application.CommissionSplit.Commands.UpdateCommissionSplit;
using SalesManagement.Presentation.Validation.CommissionSplit;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.CommissionSplit
{
    public sealed class UpdateCommissionSplitCommandValidatorTests
    {
        private readonly Mock<ICommissionSplitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateCommissionSplitCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static UpdateCommissionSplitCommand ValidCommand(
            int id = 1,
            string? splitName = "UpdatedSplit",
            int isActive = 1,
            List<CommissionSplitDetailItem>? details = null)
        {
            details ??= new List<CommissionSplitDetailItem>
            {
                new() { RoleId = 1, ShareTypeId = 10, ShareValue = 60 },
                new() { RoleId = 2, ShareTypeId = 10, ShareValue = 40 }
            };

            return new UpdateCommissionSplitCommand
            {
                Id = id,
                SplitName = splitName,
                IsActive = isActive,
                Details = details
            };
        }

        private void SetupAllAsyncMocksPass(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(false);
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

        // ── Id / NotFound Rules ───────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var command = ValidCommand(id: id);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetMiscMasterCodeAsync(It.IsAny<int>())).ReturnsAsync("PERCENTAGE");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var command = ValidCommand(id: 99);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetMiscMasterCodeAsync(It.IsAny<int>())).ReturnsAsync("PERCENTAGE");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
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
            var command = ValidCommand(splitName: "DuplicateName");
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("DuplicateName", 1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("already exists."));
        }

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValues_PassesValidation(int isActive)
        {
            var command = ValidCommand(isActive: isActive);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var command = ValidCommand(isActive: isActive);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        // ── Detail Business Rules ─────────────────────────────────────────────

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

using FluentValidation.TestHelper;
using InventoryManagement.Application.Issue.Command.CreateIssueEntry;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.Issue;
using static InventoryManagement.Application.Issue.Command.CreateIssueEntry.CreateIssueEntryDto;

namespace InventoryManagement.UnitTests.Validators.Issue
{
    public sealed class CreateIssueEntryCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateIssueEntryCommandValidator CreateValidator() => new(_mockMaxLength.Object);

        private static CreateIssueDetailDto ValidDetail() => new()
        {
            ItemId = 1,
            UomId = 1,
            RequestQuantity = 10m,
            WarehouseStockId = 1,
            StorageTypeId = 1,
            TargetId = 1,
            IssueQuantity = 5m,
            IssueValue = 500m,
            CostCenterId = 1
        };

        private static CreateIssueEntryCommand ValidCommand() => new()
        {
            IssueEntry = new CreateIssueEntryDto
            {
                UnitId = 1,
                MrsHeaderId = 1,
                DepartmentId = 1,
                RequestCategoryId = 1,
                IssueDetails = new List<CreateIssueDetailDto> { ValidDetail() }
            }
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroMrsHeaderId_FailsValidation()
        {
            var command = ValidCommand();
            command.IssueEntry.MrsHeaderId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DetailWithZeroItemId_FailsValidation()
        {
            var command = ValidCommand();
            command.IssueEntry.IssueDetails[0].ItemId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DetailWithZeroUomId_FailsValidation()
        {
            var command = ValidCommand();
            command.IssueEntry.IssueDetails[0].UomId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DetailWithZeroWarehouseStockId_FailsValidation()
        {
            var command = ValidCommand();
            command.IssueEntry.IssueDetails[0].WarehouseStockId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DetailWithZeroStorageTypeId_FailsValidation()
        {
            var command = ValidCommand();
            command.IssueEntry.IssueDetails[0].StorageTypeId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DetailWithZeroTargetId_FailsValidation()
        {
            var command = ValidCommand();
            command.IssueEntry.IssueDetails[0].TargetId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

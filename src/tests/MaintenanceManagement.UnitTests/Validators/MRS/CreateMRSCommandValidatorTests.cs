using FluentValidation.TestHelper;
using MaintenanceManagement.Application.MRS.Command.CreateMRS;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MRS;

namespace MaintenanceManagement.UnitTests.Validators.MRS
{
    public sealed class CreateMRSCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateMRSCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        private static DetailRequest ValidDetail() => new()
        {
            ItemCode = "ITEM001",
            CatCode = "CAT001",
            CcCode = "CC001",
            QtyReqd = 5m
        };

        private static CreateMRSCommand ValidCommand() => new()
        {
            Header = new HeaderRequest
            {
                Divcode = "DIV001",
                IrDate = DateTime.Now.Date,
                Depcode = "DEP001",
                SubDepcode = "SUBDEP001",
                MaintenanceType = "Corrective",
                Details = new List<DetailRequest> { ValidDetail() }
            }
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyDivcode_FailsValidation()
        {
            var command = ValidCommand();
            command.Header!.Divcode = string.Empty;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyDepcode_FailsValidation()
        {
            var command = ValidCommand();
            command.Header!.Depcode = string.Empty;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptySubDepcode_FailsValidation()
        {
            var command = ValidCommand();
            command.Header!.SubDepcode = string.Empty;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyMaintenanceType_FailsValidation()
        {
            var command = ValidCommand();
            command.Header!.MaintenanceType = string.Empty;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DetailWithZeroQtyReqd_FailsValidation()
        {
            var command = ValidCommand();
            command.Header!.Details = new List<DetailRequest>
            {
                new() { ItemCode = "ITEM001", CatCode = "CAT001", CcCode = "CC001", QtyReqd = 0m }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DetailWithEmptyItemCode_FailsValidation()
        {
            var command = ValidCommand();
            command.Header!.Details = new List<DetailRequest>
            {
                new() { ItemCode = string.Empty, CatCode = "CAT001", CcCode = "CC001", QtyReqd = 5m }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

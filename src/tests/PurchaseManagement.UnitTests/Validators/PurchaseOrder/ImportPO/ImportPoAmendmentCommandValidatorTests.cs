using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.ImportPOAmendment;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Amend;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.ImportPO
{
    public sealed class ImportPoAmendmentCommandValidatorTests
    {
        private readonly Mock<IImportPOQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private ImportPoAmendmentCommandValidator CreateValidator() =>
            new(_mockRepo.Object);

        private void SetupAllAsyncMocks(int poId = 1)
        {
            _mockRepo.Setup(r => r.ExistsAsync(poId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockRepo.Setup(r => r.HasAnyGrnAsync(poId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockRepo.Setup(r => r.GetStatusCodeAsync(poId, It.IsAny<CancellationToken>())).ReturnsAsync("Approved");
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = new ImportPOAmendmentCommand
            {
                Data = new ImportPOUpdateDto { Id = 1, AmendmentReason = "Price correction" }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new ImportPOAmendmentCommand
            {
                Data = new ImportPOUpdateDto { Id = 0, AmendmentReason = "Test" }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("Data.Id");
        }

        [Fact]
        public async Task Validate_EmptyReason_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = new ImportPOAmendmentCommand
            {
                Data = new ImportPOUpdateDto { Id = 1, AmendmentReason = "" }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("Data.AmendmentReason");
        }

        [Fact]
        public async Task Validate_NonExistentPO_FailsValidation()
        {
            _mockRepo.Setup(r => r.ExistsAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var command = new ImportPOAmendmentCommand
            {
                Data = new ImportPOUpdateDto { Id = 99, AmendmentReason = "Test" }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("Data.Id")
                .WithErrorMessage("Purchase Order not found.");
        }

        [Fact]
        public async Task Validate_GrnExists_FailsValidation()
        {
            _mockRepo.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockRepo.Setup(r => r.HasAnyGrnAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var command = new ImportPOAmendmentCommand
            {
                Data = new ImportPOUpdateDto { Id = 1, AmendmentReason = "Test" }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("Data.Id")
                .WithErrorMessage("GRN exists for this PO. Amendment is not allowed.");
        }
    }
}

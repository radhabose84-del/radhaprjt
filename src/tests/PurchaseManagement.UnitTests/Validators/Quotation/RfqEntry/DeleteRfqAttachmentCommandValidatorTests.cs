using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.DeleteAttachment;
using PurchaseManagement.Presentation.Validation.Quotation.RfqEntry;

namespace PurchaseManagement.UnitTests.Validators.Quotation.RfqEntry
{
    public sealed class DeleteRfqAttachmentCommandValidatorTests
    {
        private readonly Mock<IRfqQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private DeleteRfqAttachmentCommandValidator CreateValidator() => new(_mockRepo.Object);

        private void SetupExists(bool exists, int rfqId = 1, int attachmentId = 7)
        {
            _mockRepo
                .Setup(r => r.AttachmentExistsAsync(rfqId, attachmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupExists(true);
            var command = new DeleteRfqAttachmentCommand(1, 7);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroRfqId_FailsValidation()
        {
            SetupExists(true, rfqId: 0, attachmentId: 7);
            var command = new DeleteRfqAttachmentCommand(0, 7);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RfqId);
        }

        [Fact]
        public async Task Validate_ZeroAttachmentId_FailsValidation()
        {
            SetupExists(true, rfqId: 1, attachmentId: 0);
            var command = new DeleteRfqAttachmentCommand(1, 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AttachmentId);
        }

        [Fact]
        public async Task Validate_AttachmentDoesNotExist_FailsValidation()
        {
            SetupExists(false, rfqId: 1, attachmentId: 999);
            var command = new DeleteRfqAttachmentCommand(1, 999);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x);
        }

        [Fact]
        public async Task Validate_AttachmentDoesNotExist_DoesNotCallRepoWithZeroIds()
        {
            var command = new DeleteRfqAttachmentCommand(0, 0);

            await CreateValidator().TestValidateAsync(command);

            // When() guard prevents the async existence call when ids are 0
            _mockRepo.Verify(
                r => r.AttachmentExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}

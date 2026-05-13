using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.UploadAttachment;
using PurchaseManagement.Presentation.Validation.Quotation.RfqEntry;

namespace PurchaseManagement.UnitTests.Validators.Quotation.RfqEntry
{
    public sealed class UploadRfqAttachmentCommandValidatorTests
    {
        private static UploadRfqAttachmentCommandValidator CreateValidator() => new();

        private static IFormFile BuildFile(long length, string fileName = "specs.pdf", string contentType = "application/pdf")
        {
            var mock = new Mock<IFormFile>(MockBehavior.Loose);
            mock.Setup(f => f.Length).Returns(length);
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.ContentType).Returns(contentType);
            return mock.Object;
        }

        [Fact]
        public async Task Validate_ValidFile_PassesValidation()
        {
            var command = new UploadRfqAttachmentCommand { File = BuildFile(1024) };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NullFile_FailsValidation()
        {
            var command = new UploadRfqAttachmentCommand { File = null };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.File);
        }

        [Fact]
        public async Task Validate_ZeroByteFile_FailsValidation()
        {
            var command = new UploadRfqAttachmentCommand { File = BuildFile(0) };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.File);
        }

        [Fact]
        public async Task Validate_FileExactly5MB_PassesValidation()
        {
            var command = new UploadRfqAttachmentCommand { File = BuildFile(5L * 1024 * 1024) };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_FileOver5MB_FailsValidation()
        {
            var command = new UploadRfqAttachmentCommand { File = BuildFile(5L * 1024 * 1024 + 1) };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.File)
                  .WithErrorMessage("File must not exceed 5 MB.");
        }
    }
}

using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using PurchaseManagement.Application.OCREntry.Commands.UploadDocument;
using PurchaseManagement.Presentation.Validation.OCREntry;

namespace PurchaseManagement.UnitTests.Validators.OCREntry
{
    public sealed class UploadOCRDocumentCommandValidatorTests
    {
        private static UploadOCRDocumentCommandValidator CreateValidator() => new();

        private static IFormFile BuildFile(long length, string fileName = "ocr.png")
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(length);
            return fileMock.Object;
        }

        [Fact]
        public async Task Validate_ValidFile_PassesValidation()
        {
            var command = new UploadOCRDocumentCommand { File = BuildFile(1024) };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NullFile_FailsValidation()
        {
            var command = new UploadOCRDocumentCommand { File = null };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.File);
        }

        [Fact]
        public async Task Validate_EmptyFile_FailsValidation()
        {
            var command = new UploadOCRDocumentCommand { File = BuildFile(0) };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.File);
        }

        [Fact]
        public async Task Validate_OversizedFile_FailsValidation()
        {
            var command = new UploadOCRDocumentCommand { File = BuildFile(6L * 1024 * 1024) };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.File);
        }
    }
}

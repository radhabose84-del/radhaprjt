using FluentValidation.TestHelper;
using PurchaseManagement.Application.OCREntry.Commands.DeleteDocument;
using PurchaseManagement.Presentation.Validation.OCREntry;

namespace PurchaseManagement.UnitTests.Validators.OCREntry
{
    public sealed class DeleteOCRDocumentCommandValidatorTests
    {
        private static DeleteOCRDocumentCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidFileName_PassesValidation()
        {
            var command = new DeleteOCRDocumentCommand("TEMP_905dddb8-9bc5-4667-a2a4-ac9274946078.png");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyFileName_FailsValidation(string? fileName)
        {
            var command = new DeleteOCRDocumentCommand(fileName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FileName);
        }
    }
}

using FluentValidation.TestHelper;
using GateEntryManagement.Application.GateInward.Commands.UploadGateInwardAttachment;
using GateEntryManagement.Presentation.Validation.GateInward;
using Microsoft.AspNetCore.Http;

namespace GateEntryManagement.UnitTests.Validators.GateInward
{
    public sealed class UploadGateInwardAttachmentCommandValidatorTests
    {
        private static UploadGateInwardAttachmentCommandValidator CreateValidator() => new();

        private static IFormFile FileOfSize(long bytes)
        {
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.Length).Returns(bytes);
            return mock.Object;
        }

        [Fact]
        public async Task Validate_NullFile_FailsValidation()
        {
            var result = await CreateValidator()
                .TestValidateAsync(new UploadGateInwardAttachmentCommand { File = null });

            result.ShouldHaveValidationErrorFor(x => x.File);
        }

        [Fact]
        public async Task Validate_OversizedFile_FailsValidation()
        {
            var command = new UploadGateInwardAttachmentCommand { File = FileOfSize(6L * 1024 * 1024) };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.File);
        }

        [Fact]
        public async Task Validate_ValidFile_PassesValidation()
        {
            var command = new UploadGateInwardAttachmentCommand { File = FileOfSize(1024) };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

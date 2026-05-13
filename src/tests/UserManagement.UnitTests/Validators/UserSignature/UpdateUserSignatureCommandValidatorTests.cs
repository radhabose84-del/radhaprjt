using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.UpdateUserSignature;
using UserManagement.Presentation.Validation.UserSignature;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Validators.UserSignature
{
    public sealed class UpdateUserSignatureCommandValidatorTests
    {
        private readonly Mock<IUserSignatureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateUserSignatureCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupRecordExists(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = UserSignatureBuilders.ValidUpdateCommand();
            SetupRecordExists(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = UserSignatureBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_RecordNotFound_FailsValidation()
        {
            var command = UserSignatureBuilders.ValidUpdateCommand(id: 99);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NoFileSupplied_PassesValidation()
        {
            // File optional on Update — IsActive-only toggle should be allowed
            var command = new UpdateUserSignatureCommand { Id = 1, File = null, IsActive = UserManagement.Domain.Enums.Common.Enums.Status.Inactive };
            SetupRecordExists(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("image/gif")]
        [InlineData("application/pdf")]
        public async Task Validate_DisallowedMimeType_FailsValidation(string mimeType)
        {
            var file = UserSignatureBuilders.BuildFormFile(contentType: mimeType);
            var command = new UpdateUserSignatureCommand { Id = 1, File = file, IsActive = UserManagement.Domain.Enums.Common.Enums.Status.Active };
            SetupRecordExists(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("File.ContentType");
        }

        [Fact]
        public async Task Validate_FileExceeds5Mb_FailsValidation()
        {
            var file = UserSignatureBuilders.BuildFormFile(sizeBytes: 5 * 1024 * 1024 + 1);
            var command = new UpdateUserSignatureCommand { Id = 1, File = file, IsActive = UserManagement.Domain.Enums.Common.Enums.Status.Active };
            SetupRecordExists(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("File.Length");
        }
    }
}

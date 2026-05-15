using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.CreateUserSignature;
using UserManagement.Presentation.Validation.UserSignature;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Validators.UserSignature
{
    public sealed class CreateUserSignatureCommandValidatorTests
    {
        private readonly Mock<IUserSignatureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateUserSignatureCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath(int userId = 1)
        {
            _mockQueryRepo.Setup(r => r.UserExistsAsync(userId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UserHasSignatureAsync(userId)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = UserSignatureBuilders.ValidCreateCommand();
            SetupHappyPath(command.UserId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroUserId_FailsValidation()
        {
            var command = UserSignatureBuilders.ValidCreateCommand(userId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Fact]
        public async Task Validate_UserDoesNotExist_FailsValidation()
        {
            var command = UserSignatureBuilders.ValidCreateCommand(userId: 999);
            _mockQueryRepo.Setup(r => r.UserExistsAsync(999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.UserHasSignatureAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Fact]
        public async Task Validate_UserAlreadyHasSignature_FailsValidation()
        {
            var command = UserSignatureBuilders.ValidCreateCommand(userId: 1);
            _mockQueryRepo.Setup(r => r.UserExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UserHasSignatureAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Fact]
        public async Task Validate_NullFile_FailsValidation()
        {
            var command = new CreateUserSignatureCommand { UserId = 1, File = null };
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.File);
        }

        [Theory]
        [InlineData("image/gif")]
        [InlineData("application/pdf")]
        [InlineData("image/bmp")]
        public async Task Validate_DisallowedMimeType_FailsValidation(string mimeType)
        {
            var file = UserSignatureBuilders.BuildFormFile(contentType: mimeType);
            var command = new CreateUserSignatureCommand { UserId = 1, File = file };
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("File.ContentType");
        }

        [Theory]
        [InlineData("image/jpeg")]
        [InlineData("image/png")]
        [InlineData("image/jpg")]
        public async Task Validate_AllowedMimeType_PassesContentTypeRule(string mimeType)
        {
            var file = UserSignatureBuilders.BuildFormFile(fileName: $"sig{(mimeType == "image/png" ? ".png" : ".jpg")}", contentType: mimeType);
            var command = new CreateUserSignatureCommand { UserId = 1, File = file };
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor("File.ContentType");
        }

        [Theory]
        [InlineData("sig.gif")]
        [InlineData("sig.bmp")]
        [InlineData("sig.pdf")]
        public async Task Validate_DisallowedExtension_FailsValidation(string fileName)
        {
            var file = UserSignatureBuilders.BuildFormFile(fileName: fileName, contentType: "image/png");
            var command = new CreateUserSignatureCommand { UserId = 1, File = file };
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("File.FileName");
        }

        [Fact]
        public async Task Validate_FileExceeds5Mb_FailsValidation()
        {
            var file = UserSignatureBuilders.BuildFormFile(sizeBytes: 5 * 1024 * 1024 + 1);
            var command = new CreateUserSignatureCommand { UserId = 1, File = file };
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("File.Length");
        }
    }
}

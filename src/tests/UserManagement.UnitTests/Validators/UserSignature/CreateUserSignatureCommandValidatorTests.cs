using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.CreateUserSignature;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.UserSignature;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Validators.UserSignature
{
    public sealed class CreateUserSignatureCommandValidatorTests
    {
        private readonly Mock<IUserSignatureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UserSignatureDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private CreateUserSignatureCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider(), _mockQueryRepo.Object);

        private void SetupHappyPath(int userId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.UserExistsAsync(userId))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.UserHasSignatureAsync(userId))
                .ReturnsAsync(false);
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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyFileName_FailsValidation(string? fileName)
        {
            var command = UserSignatureBuilders.ValidCreateCommand(fileName: fileName!);
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FileName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyContentType_FailsValidation(string? contentType)
        {
            var command = UserSignatureBuilders.ValidCreateCommand(contentType: contentType!);
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ContentType);
        }

        [Theory]
        [InlineData("image/gif")]
        [InlineData("application/pdf")]
        [InlineData("image/bmp")]
        public async Task Validate_DisallowedMimeType_FailsValidation(string mimeType)
        {
            var command = UserSignatureBuilders.ValidCreateCommand(contentType: mimeType);
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ContentType);
        }

        [Theory]
        [InlineData("image/jpeg")]
        [InlineData("image/png")]
        public async Task Validate_AllowedMimeType_PassesContentTypeRule(string mimeType)
        {
            var command = UserSignatureBuilders.ValidCreateCommand(contentType: mimeType);
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.ContentType);
        }

        [Fact]
        public async Task Validate_FileExceeds500KB_FailsValidation()
        {
            var largeBytes = new byte[500 * 1024 + 1]; // 500 KB + 1 byte
            var command = UserSignatureBuilders.ValidCreateCommand(bytes: largeBytes);
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SignatureImage);
        }

        [Fact]
        public async Task Validate_EmptySignatureImage_FailsValidation()
        {
            var command = UserSignatureBuilders.ValidCreateCommand(bytes: Array.Empty<byte>());
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SignatureImage);
        }
    }
}

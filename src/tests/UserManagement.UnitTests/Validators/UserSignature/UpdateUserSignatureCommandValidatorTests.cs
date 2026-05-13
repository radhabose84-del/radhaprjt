using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.UpdateUserSignature;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.UserSignature;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Validators.UserSignature
{
    public sealed class UpdateUserSignatureCommandValidatorTests
    {
        private readonly Mock<IUserSignatureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UserSignatureUpdDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private UpdateUserSignatureCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider(), _mockQueryRepo.Object);

        private void SetupRecordExists(int id = 1)
        {
            // NotFoundAsync returns true if NOT found — so for a found record, return false
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(false);
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
        public async Task Validate_FileExceeds500KB_FailsValidation()
        {
            var largeBytes = new byte[500 * 1024 + 1];
            var command = UserSignatureBuilders.ValidUpdateCommand(bytes: largeBytes);
            SetupRecordExists(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SignatureImage);
        }

        [Theory]
        [InlineData("image/gif")]
        [InlineData("application/pdf")]
        public async Task Validate_DisallowedMimeType_FailsValidation(string mimeType)
        {
            var command = UserSignatureBuilders.ValidUpdateCommand(contentType: mimeType);
            SetupRecordExists(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ContentType);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyFileName_FailsValidation(string? fileName)
        {
            var command = UserSignatureBuilders.ValidUpdateCommand(fileName: fileName!);
            SetupRecordExists(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FileName);
        }
    }
}

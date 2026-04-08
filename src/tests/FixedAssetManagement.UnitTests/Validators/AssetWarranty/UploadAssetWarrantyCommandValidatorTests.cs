using FAM.Application.AssetMaster.AssetWarranty.Commands.UploadAssetWarranty;
using FAM.Presentation.Validation.AssetMaster.AssetWarranty;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;

namespace FixedAssetManagement.UnitTests.Validators.AssetWarranty
{
    public sealed class UploadAssetWarrantyCommandValidatorTests
    {
        private UploadAssetWarrantyCommandValidator CreateValidator() => new();

        private static IFormFile CreateMockFile(string fileName = "test.jpg", long length = 1024)
        {
            var mock = new Mock<IFormFile>(MockBehavior.Loose);
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.Length).Returns(length);
            return mock.Object;
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UploadFileAssetWarrantyCommand
            {
                File = CreateMockFile("test.jpg", 1024)
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NullFile_FailsValidation()
        {
            var command = new UploadFileAssetWarrantyCommand
            {
                File = null
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_OversizedFile_FailsValidation()
        {
            var command = new UploadFileAssetWarrantyCommand
            {
                File = CreateMockFile("test.jpg", 5 * 1024 * 1024) // 5MB
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

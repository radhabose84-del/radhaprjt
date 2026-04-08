using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UploadDocumentAssetMaster;
using FAM.Presentation.Validation.AssetMaster.AssetMasterGeneral;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;

namespace FixedAssetManagement.UnitTests.Validators.AssetMasterGeneral
{
    public sealed class UploadDocumentAssetMasterGeneralCommandValidatorTests
    {
        private UploadDocumentAssetMasterGeneralCommandValidator CreateValidator() => new();

        private static IFormFile CreateMockFile(string fileName = "test.pdf", long length = 1024)
        {
            var mock = new Mock<IFormFile>(MockBehavior.Loose);
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.Length).Returns(length);
            return mock.Object;
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UploadDocumentAssetMasterGeneralCommand
            {
                File = CreateMockFile("test.pdf", 1024)
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NullFile_FailsValidation()
        {
            var command = new UploadDocumentAssetMasterGeneralCommand
            {
                File = null
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_OversizedFile_FailsValidation()
        {
            var command = new UploadDocumentAssetMasterGeneralCommand
            {
                File = CreateMockFile("test.pdf", 5 * 1024 * 1024)
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

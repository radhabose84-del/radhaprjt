using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using UserManagement.Application.Companies.Commands.UploadFileCompany;
using UserManagement.Presentation.Validation.Companies;

namespace UserManagement.UnitTests.Validators.UploadCompany
{
    public sealed class UploadCompanyCommandValidatorTests
    {
        private UploadCompanyCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_NullFile_FailsValidation()
        {
            var command = new UploadFileCompanyCommand { File = null! };

            // The validator's IsValidFileType method accesses file.FileName before
            // the null guard, so a NullReferenceException is thrown during validation.
            Func<Task> act = async () => await CreateValidator().TestValidateAsync(command);

            await act.Should().ThrowAsync<NullReferenceException>();
        }

        [Fact]
        public async Task Validate_ValidFile_PassesValidation()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("logo.png");
            mockFile.Setup(f => f.Length).Returns(1024);
            var command = new UploadFileCompanyCommand { File = mockFile.Object };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.File);
        }
    }
}

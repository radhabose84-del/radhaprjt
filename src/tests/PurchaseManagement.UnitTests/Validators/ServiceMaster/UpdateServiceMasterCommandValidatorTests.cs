using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Commands.UpdateService;
using PurchaseManagement.Presentation.Validation.ServiceMaster;

namespace PurchaseManagement.UnitTests.Validators.ServiceMaster
{
    public sealed class UpdateServiceMasterCommandValidatorTests
    {
        private readonly Mock<IServiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateServiceMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupExistsSimilar(int sacId, int uomId, string description, int id, bool exists = false)
        {
            _mockQueryRepo
                .Setup(r => r.ExistsSimilarAsync(sacId, uomId, description, id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateServiceCommand
            {
                Id = 1,
                ServiceDescription = "Updated Service",
                SacId = 1,
                UomId = 1,
                ServiceCategoryId = 1
            };
            SetupExistsSimilar(1, 1, "Updated Service", 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = new UpdateServiceCommand
            {
                Id = 1,
                ServiceDescription = description!,
                SacId = 1,
                UomId = 1,
                ServiceCategoryId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new UpdateServiceCommand
            {
                Id = 0,
                ServiceDescription = "Updated Service",
                SacId = 1,
                UomId = 1,
                ServiceCategoryId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateService_FailsValidation()
        {
            var command = new UpdateServiceCommand
            {
                Id = 1,
                ServiceDescription = "Updated Service",
                SacId = 1,
                UomId = 1,
                ServiceCategoryId = 1
            };
            SetupExistsSimilar(1, 1, "Updated Service", 1, exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

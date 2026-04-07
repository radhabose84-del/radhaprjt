using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Commands.CreateService;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.ServiceMaster;

namespace PurchaseManagement.UnitTests.Validators.ServiceMaster
{
    public sealed class CreateServiceMasterCommandValidatorTests
    {
        private readonly Mock<IServiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateServiceMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupExistsSimilar(int sacId, int uomId, string description, bool exists = false)
        {
            _mockQueryRepo
                .Setup(r => r.ExistsSimilarAsync(sacId, uomId, description, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(exists);
        }

        private void SetupExistsSimilarCatchAll(bool exists = false)
        {
            _mockQueryRepo
                .Setup(r => r.ExistsSimilarAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateServiceCommand
            {
                ServiceDescription = "Testing Service",
                SacId = 1,
                UomId = 1
            };
            SetupExistsSimilar(1, 1, "Testing Service");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyDescription_FailsValidation()
        {
            var command = new CreateServiceCommand
            {
                ServiceDescription = "",
                SacId = 1,
                UomId = 1
            };
            SetupExistsSimilarCatchAll();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NullDescription_ThrowsDueToValidatorTrimCall()
        {
            // Validator CustomAsync calls ServiceDescription.Trim() which throws NRE for null
            var command = new CreateServiceCommand
            {
                ServiceDescription = null,
                SacId = 1,
                UomId = 1
            };

            Func<Task> act = async () => await CreateValidator().TestValidateAsync(command);

            await act.Should().ThrowAsync<NullReferenceException>();
        }

        [Fact]
        public async Task Validate_ZeroSacId_FailsValidation()
        {
            var command = new CreateServiceCommand
            {
                ServiceDescription = "Testing Service",
                SacId = 0,
                UomId = 1
            };
            SetupExistsSimilarCatchAll();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SacId);
        }

        [Fact]
        public async Task Validate_ZeroUomId_FailsValidation()
        {
            var command = new CreateServiceCommand
            {
                ServiceDescription = "Testing Service",
                SacId = 1,
                UomId = 0
            };
            SetupExistsSimilarCatchAll();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UomId);
        }

        [Fact]
        public async Task Validate_DuplicateService_FailsValidation()
        {
            var command = new CreateServiceCommand
            {
                ServiceDescription = "Testing Service",
                SacId = 1,
                UomId = 1
            };
            SetupExistsSimilar(1, 1, "Testing Service", exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}

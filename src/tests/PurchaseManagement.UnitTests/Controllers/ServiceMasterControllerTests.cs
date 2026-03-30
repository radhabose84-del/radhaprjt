using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Commands.CreateService;
using PurchaseManagement.Application.ServiceMaster.Commands.DeleteService;
using PurchaseManagement.Application.ServiceMaster.Commands.UpdateService;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.Application.ServiceMaster.Queries.GetServiceAutocomplete;
using PurchaseManagement.Application.ServiceMaster.Queries.GetServiceById;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;
using Contracts.Dtos.Common;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class ServiceMasterControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<ServiceMasterController>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IServiceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IValidator<CreateServiceCommand>> _mockCreateValidator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<UpdateServiceCommand>> _mockUpdateValidator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<DeleteServiceCommand>> _mockDeleteValidator = new(MockBehavior.Strict);

        private ServiceMasterController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object, _mockQueryRepo.Object,
                _mockCreateValidator.Object, _mockUpdateValidator.Object, _mockDeleteValidator.Object);

        private void SetupValidValidation<T>(Mock<IValidator<T>> validatorMock) where T : class
        {
            validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        private void SetupInvalidValidation<T>(Mock<IValidator<T>> validatorMock, string errorMessage) where T : class
        {
            validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("TestField", errorMessage)
                }));
        }

        [Fact]
        public async Task GetAllServiceMasterAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllServicesMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<List<GetServiceMasterDto>>(new List<GetServiceMasterDto>())
                {
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllServiceMasterAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllServiceMasterAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllServicesMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<List<GetServiceMasterDto>>(new List<GetServiceMasterDto>()));

            await CreateSut().GetAllServiceMasterAsync(1, 10, null);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllServicesMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetServiceMasterByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetServiceByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ServiceMasterBuilders.ValidDto());

            var result = await CreateSut().GetServiceMasterByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateServiceMasterAsync_ValidCommand_ReturnsCreatedResult()
        {
            SetupValidValidation(_mockCreateValidator);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateServiceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ServiceMasterBuilders.ValidDto());

            var result = await CreateSut().CreateServiceMasterAsync(ServiceMasterBuilders.ValidCreateCommand());

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateServiceMasterAsync_InvalidCommand_ReturnsBadRequest()
        {
            SetupInvalidValidation(_mockCreateValidator, "ServiceDescription is required.");

            var result = await CreateSut().CreateServiceMasterAsync(new CreateServiceCommand());

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateServiceMasterAsync_ValidCommand_ReturnsOkResult()
        {
            SetupValidValidation(_mockUpdateValidator);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateServiceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ServiceMasterBuilders.ValidDto());

            var result = await CreateSut().UpdateServiceMasterAsync(ServiceMasterBuilders.ValidUpdateCommand());

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task UpdateServiceMasterAsync_InvalidCommand_ReturnsBadRequest()
        {
            SetupInvalidValidation(_mockUpdateValidator, "Id is required.");

            var result = await CreateSut().UpdateServiceMasterAsync(new UpdateServiceCommand());

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteServiceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetServiceMaster_ByName_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetServiceAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ServiceMasterAutoCompleteDto>());

            var result = await CreateSut().GetServiceMaster("test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}

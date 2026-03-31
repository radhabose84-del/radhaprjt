using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PaymentTermMaster.Command.CreatePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.DeletePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.UpdatePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermAutoComplete;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermMasterById;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class PaymentTermMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private PaymentTermMasterController CreateSut() => new(_mockMediator.Object);

        private void SetupGetAll()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllPaymentTermMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<PaymentTermMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<PaymentTermMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            SetupGetAll();

            var result = await CreateSut().GetAllMiscMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            SetupGetAll();

            await CreateSut().GetAllMiscMasterAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllPaymentTermMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPaymentTermMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PaymentTermMasterBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePaymentTermMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(PaymentTermMasterBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePaymentTermMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(PaymentTermMasterBuilders.ValidCreateCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreatePaymentTermMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePaymentTermMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(PaymentTermMasterBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePaymentTermMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePaymentTermMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeletePaymentTermMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByName_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPaymentTermAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AutoCompleteDto>());

            var result = await CreateSut().GetPaymentTermMaster(null, null);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.TransactionTypeMaster.Commands.CreateTransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Commands.UpdateTransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Commands.DeleteTransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Queries.GetAllTransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Queries.GetTransactionTypeMasterById;
using FinanceManagement.Application.TransactionTypeMaster.Queries.GetTransactionTypeMasterAutoComplete;
using FinanceManagement.Application.TransactionTypeMaster.Dto;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class TransactionTypeMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private TransactionTypeMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllTransactionTypeMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<TransactionTypeMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<TransactionTypeMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllTransactionTypeMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllTransactionTypeMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<TransactionTypeMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<TransactionTypeMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllTransactionTypeMasterAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllTransactionTypeMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTransactionTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransactionTypeMasterDto());

            var result = await CreateSut().GetTransactionTypeMasterByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTransactionTypeMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<TransactionTypeMasterLookupDto>)new List<TransactionTypeMasterLookupDto>());

            var result = await CreateSut().GetTransactionTypeMasterAutoCompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateTransactionTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateTransactionTypeMaster(new CreateTransactionTypeMasterCommand
            {
                TypeName = "Test",
                ShortName = "T",
                UnitId = 1,
                ModuleId = 1,
                MenuId = 1
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateTransactionTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateTransactionTypeMaster(new UpdateTransactionTypeMasterCommand
            {
                Id = 1,
                TypeName = "Test",
                ShortName = "T",
                UnitId = 1,
                ModuleId = 1,
                MenuId = 1,
                IsActive = 1
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteTransactionTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteTransactionTypeMaster(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteTransactionTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteTransactionTypeMaster(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteTransactionTypeMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

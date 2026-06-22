using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.CoaChangeRequest.Commands.ApproveCoaChangeImpact;
using FinanceManagement.Application.CoaChangeRequest.Commands.ApproveCoaUnfreeze;
using FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaChangeRequest;
using FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaUnfreezeRequest;
using FinanceManagement.Application.CoaChangeRequest.Commands.SealCoa;
using FinanceManagement.Application.CoaChangeRequest.Queries.GetCoaChangeRequests;
using FinanceManagement.Application.CoaChangeRequest.Queries.GetCoaUnfreezeRequestById;
using FinanceManagement.Application.CoaChangeRequest.Queries.GetPostFreezeChangeLog;
using FinanceManagement.Application.CoaChangeRequest.Dto;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class CoaChangeRequestControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private CoaChangeRequestController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCoaChangeRequestsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CoaChangeRequestDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            (await CreateSut().GetAllAsync()).Should().BeOfType<OkObjectResult>();
            _mockMediator.Verify(m => m.Send(It.IsAny<GetCoaChangeRequestsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateCoaChangeRequestCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            (await CreateSut().CreateAsync(new CreateCoaChangeRequestCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ApproveImpact_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<ApproveCoaChangeImpactCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true, Data = true });

            (await CreateSut().ApproveImpactAsync(new ApproveCoaChangeImpactCommand { ChangeRequestId = 1 })).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task PostFreezeLog_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPostFreezeChangeLogQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PostFreezeChangeLogDto>());

            (await CreateSut().GetPostFreezeLogAsync()).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Seal_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<SealCoaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true, Data = true });

            (await CreateSut().SealAsync()).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateUnfreeze_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateCoaUnfreezeRequestCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 5 });

            (await CreateSut().CreateUnfreezeAsync(new CreateCoaUnfreezeRequestCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ApproveUnfreeze_ReturnsOkResult_AndCallsMediatorOnce()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<ApproveCoaUnfreezeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true, Data = true });

            (await CreateSut().ApproveUnfreezeAsync(new ApproveCoaUnfreezeCommand { UnfreezeRequestId = 1 })).Should().BeOfType<OkObjectResult>();
            _mockMediator.Verify(m => m.Send(It.IsAny<ApproveCoaUnfreezeCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUnfreezeById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCoaUnfreezeRequestByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CoaUnfreezeRequestDto { Id = 1 });

            (await CreateSut().GetUnfreezeByIdAsync(1)).Should().BeOfType<OkObjectResult>();
        }
    }
}

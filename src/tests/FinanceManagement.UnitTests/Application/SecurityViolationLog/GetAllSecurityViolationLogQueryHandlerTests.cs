using Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.ISecurityViolationLog;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.SecurityViolationLog.Queries.GetAllSecurityViolationLog;
using FinanceManagement.Presentation.Controllers.JournalMaster;

namespace FinanceManagement.UnitTests.Application.SecurityViolationLog
{
    public sealed class GetAllSecurityViolationLogQueryHandlerTests
    {
        private readonly Mock<ISecurityViolationLogQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private static SecurityViolationLogDto Sample() =>
            new() { Id = 1, TableName = "JournalHeader", JournalHeaderId = 5, AttemptedAction = "UPDATE", UserName = "sa", Channel = "DB", AttemptedAt = DateTimeOffset.UtcNow };

        [Fact]
        public async Task Handle_ReturnsData()
        {
            var data = new List<SecurityViolationLogDto> { Sample() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((data, 1));

            var sut = new GetAllSecurityViolationLogQueryHandler(_mockQueryRepo.Object, _mockMediator.Object);
            var result = await sut.Handle(new GetAllSecurityViolationLogQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_FiltersByJournalHeaderId()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, 5)).ReturnsAsync((new List<SecurityViolationLogDto> { Sample() }, 1));

            var sut = new GetAllSecurityViolationLogQueryHandler(_mockQueryRepo.Object, _mockMediator.Object);
            var result = await sut.Handle(new GetAllSecurityViolationLogQuery { PageNumber = 1, PageSize = 10, JournalHeaderId = 5 }, CancellationToken.None);

            result.Data.Should().ContainSingle();
            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 10, 5), Times.Once);
        }

        [Fact]
        public async Task Controller_GetAll_ReturnsOk()
        {
            var mediator = new Mock<IMediator>(MockBehavior.Strict);
            mediator.Setup(m => m.Send(It.IsAny<GetAllSecurityViolationLogQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<SecurityViolationLogDto>> { IsSuccess = true, Data = new() });

            var controller = new SecurityViolationLogController(mediator.Object);
            (await controller.GetAllAsync(1, 10)).Should().BeOfType<OkObjectResult>();
        }
    }
}

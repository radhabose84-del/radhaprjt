using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Dto;
using ProductionManagement.Application.RepackingHeader.Queries.GetAllRepackingHeader;

namespace ProductionManagement.UnitTests.Application.Repacking.Queries
{
    public sealed class GetAllRepackingHeaderQueryHandlerTests
    {
        private readonly Mock<IRepackingHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllRepackingHeaderQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static RepackingHeaderDto BuildDto(int id = 1) => new()
        {
            Id = id,
            RepackDocNo = "REPACK-001",
            RepackDate = DateOnly.FromDateTime(DateTime.Today),
            ItemId = 1,
            ItemName = "Test Item",
            IsActive = true,
            IsDeleted = false
        };

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<RepackingHeaderDto> { BuildDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, (string?)null, (int?)null)).ReturnsAsync((dtoList, 1));
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllRepackingHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<RepackingHeaderDto> { BuildDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "search", (int?)null)).ReturnsAsync((dtoList, 11));
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllRepackingHeaderQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, (string?)null, (int?)null)).ReturnsAsync((new List<RepackingHeaderDto>(), 0));
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllRepackingHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var dtoList = new List<RepackingHeaderDto> { BuildDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, (string?)null, (int?)null)).ReturnsAsync((dtoList, 1));
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetAllRepackingHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "Get"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

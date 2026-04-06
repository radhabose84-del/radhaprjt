using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Dto;
using FinanceManagement.Application.EWaybillHeader.Queries.GetAllEWaybillHeader;

namespace FinanceManagement.UnitTests.Application.EWaybillHeader.Queries
{
    public sealed class GetAllEWaybillHeaderQueryHandlerTests
    {
        private readonly Mock<IEWaybillHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllEWaybillHeaderQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<EWaybillHeaderDto> { new() { Id = 1, EWBNumber = "EWB001" } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<EWaybillHeaderDto>>(dtoList)).Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllEWaybillHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<EWaybillHeaderDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "EWB")).ReturnsAsync((dtoList, 20));
            _mockMapper.Setup(m => m.Map<List<EWaybillHeaderDto>>(dtoList)).Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllEWaybillHeaderQuery { PageNumber = 2, PageSize = 5, SearchTerm = "EWB" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(20);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var empty = new List<EWaybillHeaderDto>();
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((empty, 0));
            _mockMapper.Setup(m => m.Map<List<EWaybillHeaderDto>>(empty)).Returns(empty);

            var result = await CreateSut().Handle(
                new GetAllEWaybillHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var dtoList = new List<EWaybillHeaderDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<EWaybillHeaderDto>>(dtoList)).Returns(dtoList);

            await CreateSut().Handle(
                new GetAllEWaybillHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

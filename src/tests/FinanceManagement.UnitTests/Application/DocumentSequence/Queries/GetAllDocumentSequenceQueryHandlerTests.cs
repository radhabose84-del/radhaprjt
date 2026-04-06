using FinanceManagement.Application.Common.Interfaces.IDocumentSequence;
using FinanceManagement.Application.DocumentSequence.Dto;
using FinanceManagement.Application.DocumentSequence.Queries.GetAllDocumentSequence;

namespace FinanceManagement.UnitTests.Application.DocumentSequence.Queries
{
    public sealed class GetAllDocumentSequenceQueryHandlerTests
    {
        private readonly Mock<IDocumentSequenceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllDocumentSequenceQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<DocumentSequenceDto> { new() { Id = 1, DocNo = 100 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<DocumentSequenceDto>>(dtoList)).Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllDocumentSequenceQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<DocumentSequenceDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "test")).ReturnsAsync((dtoList, 15));
            _mockMapper.Setup(m => m.Map<List<DocumentSequenceDto>>(dtoList)).Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllDocumentSequenceQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(15);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var empty = new List<DocumentSequenceDto>();
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((empty, 0));
            _mockMapper.Setup(m => m.Map<List<DocumentSequenceDto>>(empty)).Returns(empty);

            var result = await CreateSut().Handle(
                new GetAllDocumentSequenceQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var dtoList = new List<DocumentSequenceDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<DocumentSequenceDto>>(dtoList)).Returns(dtoList);

            await CreateSut().Handle(
                new GetAllDocumentSequenceQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

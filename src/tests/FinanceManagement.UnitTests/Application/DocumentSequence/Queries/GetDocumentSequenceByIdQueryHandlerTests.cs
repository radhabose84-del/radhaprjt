using FinanceManagement.Application.Common.Interfaces.IDocumentSequence;
using FinanceManagement.Application.DocumentSequence.Dto;
using FinanceManagement.Application.DocumentSequence.Queries.GetDocumentSequenceById;

namespace FinanceManagement.UnitTests.Application.DocumentSequence.Queries
{
    public sealed class GetDocumentSequenceByIdQueryHandlerTests
    {
        private readonly Mock<IDocumentSequenceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDocumentSequenceByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new DocumentSequenceDto { Id = 1, DocNo = 100 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<DocumentSequenceDto>(dto)).Returns(dto);

            var result = await CreateSut().Handle(
                new GetDocumentSequenceByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((DocumentSequenceDto?)null);

            var result = await CreateSut().Handle(
                new GetDocumentSequenceByIdQuery { Id = 999 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = new DocumentSequenceDto { Id = 1, DocNo = 100 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<DocumentSequenceDto>(dto)).Returns(dto);

            await CreateSut().Handle(
                new GetDocumentSequenceByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((DocumentSequenceDto?)null);

            await CreateSut().Handle(
                new GetDocumentSequenceByIdQuery { Id = 999 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}

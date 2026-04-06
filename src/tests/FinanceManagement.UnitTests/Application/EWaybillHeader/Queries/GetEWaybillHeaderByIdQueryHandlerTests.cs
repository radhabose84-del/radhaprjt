using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Dto;
using FinanceManagement.Application.EWaybillHeader.Queries.GetEWaybillHeaderById;

namespace FinanceManagement.UnitTests.Application.EWaybillHeader.Queries
{
    public sealed class GetEWaybillHeaderByIdQueryHandlerTests
    {
        private readonly Mock<IEWaybillHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetEWaybillHeaderByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new EWaybillHeaderDto { Id = 1, EWBNumber = "EWB001" };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<EWaybillHeaderDto>(dto)).Returns(dto);

            var result = await CreateSut().Handle(
                new GetEWaybillHeaderByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.EWBNumber.Should().Be("EWB001");
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((EWaybillHeaderDto?)null);

            var result = await CreateSut().Handle(
                new GetEWaybillHeaderByIdQuery { Id = 999 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = new EWaybillHeaderDto { Id = 1, EWBNumber = "EWB001" };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<EWaybillHeaderDto>(dto)).Returns(dto);

            await CreateSut().Handle(
                new GetEWaybillHeaderByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((EWaybillHeaderDto?)null);

            await CreateSut().Handle(
                new GetEWaybillHeaderByIdQuery { Id = 999 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}

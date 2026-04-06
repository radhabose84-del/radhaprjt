using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Dto;
using ProductionManagement.Application.YarnConversionHeader.Queries.GetYarnConversionHeaderById;

namespace ProductionManagement.UnitTests.Application.YarnConversionHeader.Queries
{
    public sealed class GetYarnConversionHeaderByIdQueryHandlerTests
    {
        private readonly Mock<IYarnConversionHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetYarnConversionHeaderByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static YarnConversionHeaderDto BuildDto(int id = 1) => new()
        {
            Id = id,
            ConversionDocNo = "YC-001",
            ConversionDate = DateOnly.FromDateTime(DateTime.Today),
            ItemId = 1,
            ItemName = "Test Item",
            IsActive = true,
            IsDeleted = false
        };

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = BuildDto(5);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetYarnConversionHeaderByIdQuery { Id = 5 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
            result.ConversionDocNo.Should().Be("YC-001");
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((YarnConversionHeaderDto?)null);

            var result = await CreateSut().Handle(
                new GetYarnConversionHeaderByIdQuery { Id = 999 },
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = BuildDto(1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetYarnConversionHeaderByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GetYarnConversionHeaderByIdQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((YarnConversionHeaderDto?)null);

            await CreateSut().Handle(
                new GetYarnConversionHeaderByIdQuery { Id = 999 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}

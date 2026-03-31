using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;
using InventoryManagement.Application.UOMConversion.Queries.GetUOMConversionById;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.UOMConversion.Queries
{
    public sealed class GetUOMConversionByIdQueryHandlerTests
    {
        private readonly Mock<IUOMConversionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUOMConversionByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = UOMConversionBuilders.ValidDto(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<UOMConversionDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUOMConversionByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            var dto = UOMConversionBuilders.ValidDto(1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<UOMConversionDto>(It.IsAny<object>())).Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetUOMConversionByIdQuery { Id = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var dto = UOMConversionBuilders.ValidDto(1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<UOMConversionDto>(It.IsAny<object>())).Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetUOMConversionByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

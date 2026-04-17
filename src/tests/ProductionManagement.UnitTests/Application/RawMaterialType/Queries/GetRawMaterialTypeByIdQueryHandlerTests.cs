using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Dto;
using ProductionManagement.Application.RawMaterialType.Queries.GetRawMaterialTypeById;

namespace ProductionManagement.UnitTests.Application.RawMaterialType.Queries
{
    public sealed class GetRawMaterialTypeByIdQueryHandlerTests
    {
        private readonly Mock<IRawMaterialTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRawMaterialTypeByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsMappedDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(new RawMaterialTypeDto { Id = 5 });
            _mockMapper.Setup(m => m.Map<RawMaterialTypeDto>(It.IsAny<RawMaterialTypeDto>()))
                .Returns(new RawMaterialTypeDto { Id = 5 });

            var result = await CreateSut().Handle(new GetRawMaterialTypeByIdQuery { Id = 5 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(5);
        }

        [Fact]
        public async Task Handle_MissingId_ReturnsNull_AndDoesNotPublishEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((RawMaterialTypeDto?)null);

            var result = await CreateSut().Handle(new GetRawMaterialTypeByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(new RawMaterialTypeDto { Id = 5 });
            _mockMapper.Setup(m => m.Map<RawMaterialTypeDto>(It.IsAny<RawMaterialTypeDto>()))
                .Returns(new RawMaterialTypeDto { Id = 5 });

            await CreateSut().Handle(new GetRawMaterialTypeByIdQuery { Id = 5 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Application.CountMaster.Queries.GetCountMasterById;
using ProductionManagement.Application.CountMaster.Dto;

namespace ProductionManagement.UnitTests.Application.CountMaster.Queries
{
    public sealed class GetCountMasterByIdQueryHandlerTests
    {
        private readonly Mock<ICountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCountMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new CountMasterDto { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<CountMasterDto>(It.IsAny<object>())).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetCountMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((CountMasterDto?)null);

            var result = await CreateSut().Handle(new GetCountMasterByIdQuery { Id = 999 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}

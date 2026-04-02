using ProductionManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProductionManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using ProductionManagement.Application.MiscTypeMaster.Dto;

namespace ProductionManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new MiscTypeMasterDto { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<MiscTypeMasterDto>(It.IsAny<object>())).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((MiscTypeMasterDto?)null);

            var result = await CreateSut().Handle(new GetMiscTypeMasterByIdQuery { Id = 999 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}

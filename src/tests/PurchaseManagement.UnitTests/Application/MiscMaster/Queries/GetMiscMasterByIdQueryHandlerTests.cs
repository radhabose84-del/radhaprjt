using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMaster;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var entity = MiscMasterBuilders.ValidEntity(1);
            var dto = MiscMasterBuilders.ValidDto(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }
    }
}

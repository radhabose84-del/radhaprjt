using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.MiscMaster.Queries.GetMiscMaster;
using InventoryManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var entity = MiscMasterBuilders.ValidEntity(1);
            var dto = MiscMasterBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(It.IsAny<InventoryManagement.Domain.Entities.MiscMaster>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsGetByIdOnce()
        {
            var entity = MiscMasterBuilders.ValidEntity(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(It.IsAny<InventoryManagement.Domain.Entities.MiscMaster>()))
                .Returns(MiscMasterBuilders.ValidDto(1));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var entity = MiscMasterBuilders.ValidEntity(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(It.IsAny<InventoryManagement.Domain.Entities.MiscMaster>()))
                .Returns(MiscMasterBuilders.ValidDto(1));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

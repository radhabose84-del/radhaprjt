using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsSuccess()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(1);
            var dto = MiscTypeMasterBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<InventoryManagement.Domain.Entities.MiscTypeMaster>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(9999))
                .ReturnsAsync((InventoryManagement.Domain.Entities.MiscTypeMaster?)null);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 9999 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ExistingId_CallsGetByIdOnce()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<InventoryManagement.Domain.Entities.MiscTypeMaster>()))
                .Returns(MiscTypeMasterBuilders.ValidDto(1));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}

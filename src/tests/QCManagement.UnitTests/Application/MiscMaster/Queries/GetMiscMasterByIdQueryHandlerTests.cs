using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IMiscMaster;
using QCManagement.Application.MiscMaster.Dto;
using QCManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.MiscMaster.Queries
{
    public class GetMiscMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetMiscMasterByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<MiscMasterDto>(It.IsAny<object>()))
                .Returns<object>(o => (o as MiscMasterDto)!);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetMiscMasterByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsNotNull()
        {
            var dto = MiscMasterBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsCorrectDto()
        {
            var dto = MiscMasterBuilders.ValidDto(id: 1, code: "PHY");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Code.Should().Be("PHY");
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((MiscMasterDto?)null);

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            var dto = MiscMasterBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditEvent()
        {
            var dto = MiscMasterBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

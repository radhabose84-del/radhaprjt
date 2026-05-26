using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IMiscTypeMaster;
using QCManagement.Application.MiscTypeMaster.Dto;
using QCManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public class GetMiscTypeMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetMiscTypeMasterByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<MiscTypeMasterDto>(It.IsAny<object>()))
                .Returns<object>(o => (o as MiscTypeMasterDto)!);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetMiscTypeMasterByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsNotNull()
        {
            var dto = MiscTypeMasterBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsCorrectDto()
        {
            var dto = MiscTypeMasterBuilders.ValidDto(id: 1, code: "QP_GROUP");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.MiscTypeCode.Should().Be("QP_GROUP");
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((MiscTypeMasterDto?)null);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            var dto = MiscTypeMasterBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditEvent()
        {
            var dto = MiscTypeMasterBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

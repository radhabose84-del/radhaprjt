using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IFreightMaster;
using SalesManagement.Application.FreightMaster.Dto;
using SalesManagement.Application.FreightMaster.Queries.GetFreightMasterById;

namespace SalesManagement.UnitTests.Application.FreightMaster.Queries
{
    public class GetFreightMasterByIdQueryHandlerTests
    {
        private readonly Mock<IFreightMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetFreightMasterByIdQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetFreightMasterByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsDto()
        {
            var dto = new FreightMasterDto { Id = 1, FreightModeId = 10, Rate = 100m };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetFreightMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Rate.Should().Be(100m);
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            var dto = new FreightMasterDto { Id = 7 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(dto);

            await CreateSut().Handle(new GetFreightMasterByIdQuery { Id = 7 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((FreightMasterDto?)null);

            var result = await CreateSut().Handle(
                new GetFreightMasterByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}

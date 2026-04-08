using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Dto;
using SalesManagement.Application.DiscountMaster.Queries.GetDiscountMasterById;

namespace SalesManagement.UnitTests.Application.DiscountMaster.Queries
{
    public class GetDiscountMasterByIdQueryHandlerTests
    {
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetDiscountMasterByIdQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetDiscountMasterByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        private static DiscountMasterDto ValidDto(int id = 1) => new()
        {
            Id = id,
            DiscountCode = "DC001",
            DiscountName = "Test Discount",
            TriggerEventId = 1,
            IsActive = true
        };

        [Fact]
        public async Task Handle_EntityExists_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ValidDto(1));

            var result = await CreateSut().Handle(
                new GetDiscountMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsCorrectId()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ValidDto(1));

            var result = await CreateSut().Handle(
                new GetDiscountMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((DiscountMasterDto?)null);

            var result = await CreateSut().Handle(
                new GetDiscountMasterByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(ValidDto(7));

            await CreateSut().Handle(
                new GetDiscountMasterByIdQuery { Id = 7 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
        }
    }
}

using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Dto;
using SalesManagement.Application.DispatchAddressMaster.Queries.GetDispatchAddressMasterById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.DispatchAddressMaster.Queries
{
    public sealed class GetDispatchAddressMasterByIdQueryHandlerTests
    {
        private readonly Mock<IDispatchAddressMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetDispatchAddressMasterByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<DispatchAddressMasterDto>(It.IsAny<DispatchAddressMasterDto>()))
                .Returns<DispatchAddressMasterDto>(dto => dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetDispatchAddressMasterByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = DispatchAddressMasterBuilders.ValidDto(id: 5);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetDispatchAddressMasterByIdQuery { Id = 5 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsCorrectFields()
        {
            var dto = DispatchAddressMasterBuilders.ValidDto(
                id: 1,
                dispatchAddressName: "Factory Gate",
                addressLine1: "Plot 10, Industrial Area",
                pinCode: "400001");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetDispatchAddressMasterByIdQuery { Id = 1 },
                CancellationToken.None);

            result!.DispatchAddressName.Should().Be("Factory Gate");
            result.AddressLine1.Should().Be("Plot 10, Industrial Area");
            result.PinCode.Should().Be("400001");
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((DispatchAddressMasterDto?)null);

            var result = await CreateSut().Handle(
                new GetDispatchAddressMasterByIdQuery { Id = 999 },
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((DispatchAddressMasterDto?)null);

            await CreateSut().Handle(
                new GetDispatchAddressMasterByIdQuery { Id = 999 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = DispatchAddressMasterBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            await CreateSut().Handle(
                new GetDispatchAddressMasterByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

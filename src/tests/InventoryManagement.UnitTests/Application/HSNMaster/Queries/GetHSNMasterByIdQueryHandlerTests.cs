using AutoMapper;
using MediatR;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterById;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.HSNMaster.Queries
{
    public sealed class GetHSNMasterByIdQueryHandlerTests
    {
        private readonly Mock<IHSNMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetHSNMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsSuccess()
        {
            var dto = HSNMasterBuilders.ValidDto(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<HSNMasterDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetHSNMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NonExistingId_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((HSNMasterDto?)null);

            var result = await CreateSut().Handle(
                new GetHSNMasterByIdQuery { Id = 99 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_NonExistingId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((HSNMasterDto?)null);

            await CreateSut().Handle(new GetHSNMasterByIdQuery { Id = 99 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}

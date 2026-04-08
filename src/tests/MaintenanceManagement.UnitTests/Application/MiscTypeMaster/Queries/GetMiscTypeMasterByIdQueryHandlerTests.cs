using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new MaintenanceManagement.Domain.Entities.MiscTypeMaster { Id = 1 });
            _mockMapper.Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<object>())).Returns(new GetMiscTypeMasterDto { Id = 1 });

            var result = await CreateSut().Handle(new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new MaintenanceManagement.Domain.Entities.MiscTypeMaster { Id = 1 });
            _mockMapper.Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<object>())).Returns(new GetMiscTypeMasterDto { Id = 1 });

            await CreateSut().Handle(new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

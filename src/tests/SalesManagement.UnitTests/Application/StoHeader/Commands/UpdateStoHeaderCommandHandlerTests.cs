using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Commands.UpdateStoHeader;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.StoHeader.Commands
{
    public sealed class UpdateStoHeaderCommandHandlerTests
    {
        private readonly Mock<IStoHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IStoHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateStoHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockOutbox.Object,
                _mockIpService.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.StoHeader>(It.IsAny<UpdateStoHeaderCommand>()))
                .Returns(new SalesManagement.Domain.Entities.StoHeader());

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.StoHeader>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Handle(new UpdateStoHeaderCommand { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.StoHeader>(It.IsAny<UpdateStoHeaderCommand>()))
                .Returns(new SalesManagement.Domain.Entities.StoHeader());

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.StoHeader>()))
                .ReturnsAsync(1);

            await CreateSut().Handle(new UpdateStoHeaderCommand { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "STO_HEADER_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

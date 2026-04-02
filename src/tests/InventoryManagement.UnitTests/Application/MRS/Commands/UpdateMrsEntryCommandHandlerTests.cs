using AutoMapper;
using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Application.MRS.Command.UpdateMrsEntry;
using InventoryManagement.Domain.Entities.MRS;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MRS.Commands
{
    public sealed class UpdateMrsEntryCommandHandlerTests
    {
        private readonly Mock<IMrsEntryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private UpdateMrsEntryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = new UpdateMrsEntryCommand { updateMrsEntry = new UpdateMrsEntryDto() };
            var header = new MrsHeader { Id = 1 };

            _mockMapper.Setup(m => m.Map<MrsHeader>(It.IsAny<object>())).Returns(header);
            _mockCommandRepo.Setup(r => r.UpdateAsync(header)).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CallsUpdateAsync()
        {
            var command = new UpdateMrsEntryCommand { updateMrsEntry = new UpdateMrsEntryDto() };
            var header = new MrsHeader { Id = 1 };

            _mockMapper.Setup(m => m.Map<MrsHeader>(It.IsAny<object>())).Returns(header);
            _mockCommandRepo.Setup(r => r.UpdateAsync(header)).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(header), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var command = new UpdateMrsEntryCommand { updateMrsEntry = new UpdateMrsEntryDto() };
            var header = new MrsHeader { Id = 1 };

            _mockMapper.Setup(m => m.Map<MrsHeader>(It.IsAny<object>())).Returns(header);
            _mockCommandRepo.Setup(r => r.UpdateAsync(header)).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

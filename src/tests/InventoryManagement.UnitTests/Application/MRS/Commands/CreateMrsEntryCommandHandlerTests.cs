using AutoMapper;
using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces.IOutbox;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Application.MRS.Command.CreateMrsEntry;
using InventoryManagement.Domain.Entities.MRS;
using InventoryManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.UnitTests.Application.MRS.Commands
{
    public sealed class CreateMrsEntryCommandHandlerTests
    {
        private readonly Mock<IMrsEntryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateMrsEntryCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreateMrsEntryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockIpService.Object, _mockOutbox.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsId()
        {
            var command = new CreateMrsEntryCommand { MrsEntry = new CreateMrsEntryDto() };
            var header = new MrsHeader { Id = 1 };

            _mockMapper.Setup(m => m.Map<MrsHeader>(It.IsAny<object>())).Returns(header);
            _mockCommandRepo.Setup(r => r.GenerateNextCodeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("MRS-001");
            _mockCommandRepo.Setup(r => r.CreateAsync(header)).ReturnsAsync(header);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockOutbox.Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_CallsCreateAsync()
        {
            var command = new CreateMrsEntryCommand { MrsEntry = new CreateMrsEntryDto() };
            var header = new MrsHeader { Id = 5 };

            _mockMapper.Setup(m => m.Map<MrsHeader>(It.IsAny<object>())).Returns(header);
            _mockCommandRepo.Setup(r => r.GenerateNextCodeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("MRS-001");
            _mockCommandRepo.Setup(r => r.CreateAsync(header)).ReturnsAsync(header);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockOutbox.Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(header), Times.Once);
        }
    }
}

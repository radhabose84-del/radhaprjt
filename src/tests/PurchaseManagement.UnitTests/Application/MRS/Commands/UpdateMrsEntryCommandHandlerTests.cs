using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMRS;
using PurchaseManagement.Application.MRS.Command.UpdateMrsEntry;
using PurchaseManagement.Domain.Entities.MRS;

namespace PurchaseManagement.UnitTests.Application.MRS.Commands
{
    public sealed class UpdateMrsEntryCommandHandlerTests
    {
        private readonly Mock<IMrsEntryCommandRepository> _mockCmdRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private UpdateMrsEntryCommandHandler CreateSut() =>
            new(_mockCmdRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockIp.Object);

        [Fact]
        public async Task Handle_SuccessfulUpdate_ReturnsTrue()
        {
            _mockMapper
                .Setup(m => m.Map<MrsHeader>(It.IsAny<object>()))
                .Returns(new MrsHeader { MrsNo = "MRS-001", MrsDate = DateTime.Today });

            _mockCmdRepo
                .Setup(r => r.UpdateAsync(It.IsAny<MrsHeader>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new UpdateMrsEntryCommand
            {
                updateMrsEntry = new PurchaseManagement.Application.MRS.Command.UpdateMrsEntry.UpdateMrsEntryDto()
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_FailedUpdate_ThrowsException()
        {
            _mockMapper
                .Setup(m => m.Map<MrsHeader>(It.IsAny<object>()))
                .Returns(new MrsHeader { MrsNo = "MRS-001", MrsDate = DateTime.Today });

            _mockCmdRepo
                .Setup(r => r.UpdateAsync(It.IsAny<MrsHeader>()))
                .ReturnsAsync(false);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new UpdateMrsEntryCommand
            {
                updateMrsEntry = new PurchaseManagement.Application.MRS.Command.UpdateMrsEntry.UpdateMrsEntryDto()
            };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Mrs update failed*");
        }

        [Fact]
        public async Task Handle_SuccessfulUpdate_PublishesAuditEvent()
        {
            _mockMapper
                .Setup(m => m.Map<MrsHeader>(It.IsAny<object>()))
                .Returns(new MrsHeader { MrsNo = "MRS-001", MrsDate = DateTime.Today });

            _mockCmdRepo.Setup(r => r.UpdateAsync(It.IsAny<MrsHeader>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var command = new UpdateMrsEntryCommand
            {
                updateMrsEntry = new PurchaseManagement.Application.MRS.Command.UpdateMrsEntry.UpdateMrsEntryDto()
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

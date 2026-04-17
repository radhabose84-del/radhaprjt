using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IIssueReturn;
using PurchaseManagement.Application.IssueReturn.Command.UpdateIssueReturn;
using PurchaseManagement.Domain.Entities.IssueReturn;

namespace PurchaseManagement.UnitTests.Application.IssueReturn.Commands
{
    public sealed class UpdateIssueReturnEntryCommandHandlerTests
    {
        private readonly Mock<IIssueReturnEntryCommandRepository> _mockCmdRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private UpdateIssueReturnEntryCommandHandler CreateSut() =>
            new(_mockCmdRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockIp.Object);

        [Fact]
        public async Task Handle_SuccessfulUpdate_ReturnsTrue()
        {
            _mockMapper
                .Setup(m => m.Map<IssueReturnHeader>(It.IsAny<object>()))
                .Returns(new IssueReturnHeader { IssueReturnNo = "IR-001", IssueReturnDate = DateTime.Today });

            _mockCmdRepo
                .Setup(r => r.UpdateAsync(It.IsAny<IssueReturnHeader>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test");
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");

            var command = new UpdateIssueReturnEntryCommand
            {
                updateIssueReturnEntry = new PurchaseManagement.Application.IssueReturn.Command.UpdateIssueReturn.UpdateIssueReturnDto()
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_FailedUpdate_ThrowsException()
        {
            _mockMapper
                .Setup(m => m.Map<IssueReturnHeader>(It.IsAny<object>()))
                .Returns(new IssueReturnHeader { IssueReturnNo = "IR-001", IssueReturnDate = DateTime.Today });

            _mockCmdRepo
                .Setup(r => r.UpdateAsync(It.IsAny<IssueReturnHeader>()))
                .ReturnsAsync(false);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test");
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");

            var command = new UpdateIssueReturnEntryCommand
            {
                updateIssueReturnEntry = new PurchaseManagement.Application.IssueReturn.Command.UpdateIssueReturn.UpdateIssueReturnDto()
            };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Issue Return update failed*");
        }

        [Fact]
        public async Task Handle_SuccessfulUpdate_PublishesAuditEvent()
        {
            _mockMapper
                .Setup(m => m.Map<IssueReturnHeader>(It.IsAny<object>()))
                .Returns(new IssueReturnHeader { IssueReturnNo = "IR-001", IssueReturnDate = DateTime.Today });

            _mockCmdRepo.Setup(r => r.UpdateAsync(It.IsAny<IssueReturnHeader>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test");
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");

            var command = new UpdateIssueReturnEntryCommand
            {
                updateIssueReturnEntry = new PurchaseManagement.Application.IssueReturn.Command.UpdateIssueReturn.UpdateIssueReturnDto()
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

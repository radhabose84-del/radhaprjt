using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Application.Issue.Command.CreateIssueEntry;
using PurchaseManagement.Domain.Entities.Issue;

namespace PurchaseManagement.UnitTests.Application.Issue.Commands
{
    public sealed class CreateIssueEntryCommandHandlerTests
    {
        private readonly Mock<IIssueEntryCommandRepository> _mockCmdRepo = new(MockBehavior.Loose);
        private readonly Mock<IIssueQueryCommandRepository> _mockQryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IPutawayRuleLookup> _mockPutaway = new(MockBehavior.Loose);
        private readonly Mock<IStockLedgerLookup> _mockStockLedger = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateIssueEntryCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreateIssueEntryCommandHandler CreateSut() =>
            new(
                _mockCmdRepo.Object, _mockQryRepo.Object, _mockMapper.Object,
                _mockMediator.Object, _mockIp.Object, _mockPutaway.Object,
                _mockStockLedger.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateIssueAsync()
        {
            var issueEntry = new CreateIssueEntryDto
            {
                RequestCategoryId = 1,
                IssueDetails = new List<CreateIssueEntryDto.CreateIssueDetailDto>()
            };

            _mockMapper
                .Setup(m => m.Map<IssueHeader>(It.IsAny<object>()))
                .Returns(new IssueHeader { IssueNo = null });

            _mockCmdRepo
                .Setup(r => r.GenerateNextCodeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("ISS-001");

            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test");
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");

            _mockCmdRepo
                .Setup(r => r.CreateIssueAsync(It.IsAny<IssueHeader>(), It.IsAny<Func<Task>?>()))
                .ReturnsAsync(42);

            _mockQryRepo
                .Setup(r => r.GetDescriptionByIdAsync(It.IsAny<int>()))
                .ReturnsAsync("SomeCategory");

            _mockStockLedger
                .Setup(s => s.InsertStockLedgerAsync(
                    It.IsAny<List<Contracts.Dtos.Stock.StockLedgerDto>>(),
                    It.IsAny<System.Data.IDbTransaction>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new CreateIssueEntryCommand { IssueEntry = issueEntry };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
            _mockCmdRepo.Verify(r => r.CreateIssueAsync(It.IsAny<IssueHeader>(), It.IsAny<Func<Task>?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_StockLedgerFails_ThrowsApplicationException()
        {
            var issueEntry = new CreateIssueEntryDto
            {
                RequestCategoryId = 1,
                IssueDetails = new List<CreateIssueEntryDto.CreateIssueDetailDto>()
            };

            _mockMapper
                .Setup(m => m.Map<IssueHeader>(It.IsAny<object>()))
                .Returns(new IssueHeader { IssueNo = null });

            _mockCmdRepo
                .Setup(r => r.GenerateNextCodeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("ISS-001");

            _mockCmdRepo
                .Setup(r => r.CreateIssueAsync(It.IsAny<IssueHeader>(), It.IsAny<Func<Task>?>()))
                .ReturnsAsync(1);

            _mockQryRepo
                .Setup(r => r.GetDescriptionByIdAsync(It.IsAny<int>()))
                .ReturnsAsync("SomeCategory");

            _mockStockLedger
                .Setup(s => s.InsertStockLedgerAsync(
                    It.IsAny<List<Contracts.Dtos.Stock.StockLedgerDto>>(),
                    It.IsAny<System.Data.IDbTransaction>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = new CreateIssueEntryCommand { IssueEntry = issueEntry };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("*StockLedger insert failed*");
        }
    }
}

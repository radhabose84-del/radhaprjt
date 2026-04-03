using AutoMapper;
using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces.IIssue;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Issue.Command.CreateIssueEntry;
using InventoryManagement.Domain.Entities.Issue;
using InventoryManagement.Domain.Entities.Stock;
using InventoryManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.UnitTests.Application.Issue.Commands
{
    public sealed class CreateIssueEntryCommandHandlerTests
    {
        private readonly Mock<IIssueEntryCommandRepository> _mockCmdRepo = new(MockBehavior.Strict);
        private readonly Mock<IIssueQueryCommandRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateIssueEntryCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IPutAwayRuleQueryRepository> _mockPutAwayRepo = new(MockBehavior.Loose);

        private CreateIssueEntryCommandHandler CreateSut() =>
            new(_mockCmdRepo.Object, _mockQueryRepo.Object, _mockMapper.Object,
                _mockMediator.Object, _mockIpService.Object, _mockLogger.Object,
                _mockPutAwayRepo.Object);

        private void SetupHappyPath(int returnId = 42)
        {
            var header = new IssueHeader { IssueNo = null! };
            _mockMapper.Setup(m => m.Map<IssueHeader>(It.IsAny<object>())).Returns(header);
            _mockCmdRepo.Setup(r => r.GenerateNextCodeAsync(It.IsAny<CancellationToken>())).ReturnsAsync("ISS-001");
            _mockIpService.Setup(i => i.GetUserId()).Returns(1);
            _mockIpService.Setup(i => i.GetUserName()).Returns("test-user");
            _mockIpService.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockQueryRepo.Setup(q => q.GetDescriptionByIdAsync(It.IsAny<int>()))
                .ReturnsAsync("GeneralStore");
            _mockCmdRepo.Setup(r => r.CreateIssueWithLedgersAsync(
                    It.IsAny<IssueHeader>(),
                    It.IsAny<List<StockLedger>>(),
                    It.IsAny<List<SubStoreStockLedger>>(),
                    It.IsAny<Func<Task>>()))
                .ReturnsAsync(returnId);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsId()
        {
            SetupHappyPath(42);
            var command = new CreateIssueEntryCommand
            {
                IssueEntry = new CreateIssueEntryDto
                {
                    RequestCategoryId = 1,
                    IssueDetails = new List<CreateIssueEntryDto.CreateIssueDetailDto>()
                }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_CallsCreateIssueWithLedgersOnce()
        {
            SetupHappyPath();
            var command = new CreateIssueEntryCommand
            {
                IssueEntry = new CreateIssueEntryDto
                {
                    RequestCategoryId = 1,
                    IssueDetails = new List<CreateIssueEntryDto.CreateIssueDetailDto>()
                }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCmdRepo.Verify(r => r.CreateIssueWithLedgersAsync(
                It.IsAny<IssueHeader>(),
                It.IsAny<List<StockLedger>>(),
                It.IsAny<List<SubStoreStockLedger>>(),
                It.IsAny<Func<Task>>()),
                Times.Once);
        }
    }
}

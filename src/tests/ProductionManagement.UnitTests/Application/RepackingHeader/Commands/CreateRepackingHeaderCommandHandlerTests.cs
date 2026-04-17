using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.CreateRepackingHeader;

namespace ProductionManagement.UnitTests.Application.RepackingHeader.Commands
{
    public sealed class CreateRepackingHeaderCommandHandlerTests
    {
        private readonly Mock<IRepackingHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateRepackingHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockDocSeq.Object, _mockMediator.Object, _mockMapper.Object, _mockIp.Object);

        private void SetupHappyPath(int newId = 1, int? typeId = 7, string docNo = "RH-001")
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingHeader>(It.IsAny<CreateRepackingHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingHeader());
            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(typeId);
            _mockDocSeq.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { docNo });
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingHeader>(), It.IsAny<int>()))
                .ReturnsAsync(newId);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        private static CreateRepackingHeaderCommand BuildCommand(int itemId = 1, int oldItemId = 1) =>
            new() { ItemId = itemId, OldItemId = oldItemId, RepackDate = new DateOnly(2026, 1, 1) };

        [Fact]
        public async Task Handle_RepackingFlow_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(BuildCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Repacking created successfully.");
        }

        [Fact]
        public async Task Handle_YarnConversionFlow_ReturnsCorrectMessage()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(BuildCommand(itemId: 1, oldItemId: 2), CancellationToken.None);
            result.Message.Should().Be("Yarn Conversion created successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(BuildCommand(), CancellationToken.None);
            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_TypeIdNotFound_ThrowsExceptionRules()
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingHeader>(It.IsAny<CreateRepackingHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingHeader());
            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(BuildCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Transaction Type*not found*");
        }

        [Fact]
        public async Task Handle_NoDocumentSequence_ThrowsExceptionRules()
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingHeader>(It.IsAny<CreateRepackingHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingHeader());
            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(7);
            _mockDocSeq.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string>());

            Func<Task> act = async () => await CreateSut().Handle(BuildCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*No document sequence configured*");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent_WithCorrectActionCode()
        {
            SetupHappyPath();
            await CreateSut().Handle(BuildCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "REPACKING_CREATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

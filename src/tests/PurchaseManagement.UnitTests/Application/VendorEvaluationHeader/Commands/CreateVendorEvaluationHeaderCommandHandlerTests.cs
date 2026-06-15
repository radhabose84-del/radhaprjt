using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.CreateVendorEvaluationHeader;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.VendorEvaluationHeader.Commands
{
    public sealed class CreateVendorEvaluationHeaderCommandHandlerTests
    {
        private readonly Mock<IVendorEvaluationHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IVendorEvaluationHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private CreateVendorEvaluationHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object,
                _mockMapper.Object, _mockDocSeq.Object, _mockIpService.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns((int?)1);

            _mockDocSeq
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)1);

            _mockDocSeq
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync((IReadOnlyList<string>)new List<string> { "EVL001" });

            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader>(It.IsAny<object>()))
                .Returns(new PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(VendorEvaluationHeaderBuilders.ValidCreateCommand(), CancellationToken.None);
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(VendorEvaluationHeaderBuilders.ValidCreateCommand(), CancellationToken.None);
            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(VendorEvaluationHeaderBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.CreateAsync(
                It.IsAny<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(VendorEvaluationHeaderBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "VENDOR_EVAL_HEADER_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCorrectMessage()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(VendorEvaluationHeaderBuilders.ValidCreateCommand(), CancellationToken.None);
            result.Message.Should().Contain("created successfully");
        }

        [Fact]
        public async Task Handle_CommandWithDetails_MapsDetailItems()
        {
            PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader? capturedEntity = null;

            _mockIpService.Setup(s => s.GetUnitId()).Returns((int?)1);
            _mockDocSeq
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)1);
            _mockDocSeq
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync((IReadOnlyList<string>)new List<string> { "EVL001" });
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader>(It.IsAny<object>()))
                .Returns(new PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .Callback<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader, int, CancellationToken>(
                    (e, _, __) => capturedEntity = e)
                .ReturnsAsync(1);

            var command = VendorEvaluationHeaderBuilders.ValidCreateCommand();
            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.VendorEvaluationDetails.Should().NotBeNull();
            capturedEntity.VendorEvaluationDetails!.Should().HaveCount(1);
        }
    }
}

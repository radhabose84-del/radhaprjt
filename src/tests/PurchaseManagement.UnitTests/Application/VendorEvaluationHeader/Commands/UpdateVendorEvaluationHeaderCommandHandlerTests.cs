using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.UpdateVendorEvaluationHeader;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.VendorEvaluationHeader.Commands
{
    public sealed class UpdateVendorEvaluationHeaderCommandHandlerTests
    {
        private readonly Mock<IVendorEvaluationHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IVendorEvaluationHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateVendorEvaluationHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int result = 1)
        {
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader>(It.IsAny<object>()))
                .Returns(new PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader());
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader>()))
                .ReturnsAsync(result);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(VendorEvaluationHeaderBuilders.ValidUpdateCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsAffectedRows()
        {
            SetupHappyPath(result: 1);
            var result = await CreateSut().Handle(VendorEvaluationHeaderBuilders.ValidUpdateCommand(), CancellationToken.None);
            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(VendorEvaluationHeaderBuilders.ValidUpdateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(VendorEvaluationHeaderBuilders.ValidUpdateCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "VENDOR_EVAL_HEADER_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCorrectMessage()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(VendorEvaluationHeaderBuilders.ValidUpdateCommand(), CancellationToken.None);
            result.Message.Should().Contain("updated successfully");
        }

        [Fact]
        public async Task Handle_CommandWithDetails_MapsDetailItems()
        {
            PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader? capturedEntity = null;
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader>(It.IsAny<object>()))
                .Returns(new PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader());
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader>()))
                .Callback<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader>(e => capturedEntity = e)
                .ReturnsAsync(1);

            var command = VendorEvaluationHeaderBuilders.ValidUpdateCommand();
            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.VendorEvaluationDetails.Should().NotBeNull();
            capturedEntity.VendorEvaluationDetails!.Should().HaveCount(1);
        }
    }
}

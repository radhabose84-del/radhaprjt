using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Purchase;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Commands.SaveDisposition;
using QCManagement.Domain.Events;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QcInspection.Commands
{
    public class SaveDispositionCommandHandlerTests
    {
        private readonly Mock<IQcInspectionCommandRepository> _cmd = new(MockBehavior.Strict);
        private readonly Mock<IQcInspectionQueryRepository> _qry = new(MockBehavior.Strict);
        private readonly Mock<IGrnLookup> _grn = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);

        private SaveDispositionCommandHandler CreateSut() =>
            new(_cmd.Object, _qry.Object, _grn.Object, _mediator.Object, _ip.Object);

        private void SetupHappyPath()
        {
            _qry.Setup(q => q.GetQcStatusCodeByIdAsync(34)).ReturnsAsync("APR");
            _qry.Setup(q => q.GetDispositionContextAsync(88)).ReturnsAsync(QcInspectionBuilders.ValidContext());
            _cmd.Setup(c => c.SaveDispositionAsync(88, 34, 1000m, 0m, null,
                    It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), true, 100, 4321)).ReturnsAsync(88);
            _grn.Setup(g => g.GetByGrnDetailIdAsync(4321, It.IsAny<CancellationToken>()))
                .ReturnsAsync(QcInspectionBuilders.ValidGrnLookup());
            _mediator.Setup(m => m.Publish(It.IsAny<QcDispositionCompletedDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_Valid_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(QcInspectionBuilders.ValidDispositionCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Valid_PersistsDisposition()
        {
            SetupHappyPath();
            await CreateSut().Handle(QcInspectionBuilders.ValidDispositionCommand(), CancellationToken.None);

            _cmd.Verify(c => c.SaveDispositionAsync(88, 34, 1000m, 0m, null,
                It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), true, 100, 4321), Times.Once);
        }

        [Fact]
        public async Task Handle_Valid_PublishesMovementEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(QcInspectionBuilders.ValidDispositionCommand(), CancellationToken.None);

            _mediator.Verify(m => m.Publish(
                It.Is<QcDispositionCompletedDomainEvent>(e => e.QcStatusCode == "APR" && e.GrnHeaderId == 100),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

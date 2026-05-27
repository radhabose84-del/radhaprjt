using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.UpdateVendorRatingGrade;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.VendorRatingGrade.Commands
{
    public sealed class UpdateVendorRatingGradeCommandHandlerTests
    {
        private readonly Mock<IVendorRatingGradeCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IVendorRatingGradeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateVendorRatingGradeCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int result = 1)
        {
            _mockMapper.Setup(m => m.Map<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorRatingGrade>(It.IsAny<object>())).Returns(VendorRatingGradeBuilders.ValidEntity());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorRatingGrade>())).ReturnsAsync(result);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(VendorRatingGradeBuilders.ValidUpdateCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsAffectedRows()
        {
            SetupHappyPath(result: 1);
            var result = await CreateSut().Handle(VendorRatingGradeBuilders.ValidUpdateCommand(), CancellationToken.None);
            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(VendorRatingGradeBuilders.ValidUpdateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.VendorEvaluation.VendorRatingGrade>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(VendorRatingGradeBuilders.ValidUpdateCommand(), CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

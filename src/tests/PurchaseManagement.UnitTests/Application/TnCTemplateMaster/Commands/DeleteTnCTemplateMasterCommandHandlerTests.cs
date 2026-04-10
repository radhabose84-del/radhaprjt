using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Command.DeleteTnCTemplateMasterCommand;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.TnCTemplateMaster.Commands
{
    public sealed class DeleteTnCTemplateMasterCommandHandlerTests
    {
        private readonly Mock<ITnCTemplateMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ITnCTemplateMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteTnCTemplateMasterCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var dto = TnCTemplateMasterBuilders.ValidDto(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(dto);

            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(
                TnCTemplateMasterBuilders.ValidDeleteCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(TnCTemplateMasterBuilders.ValidDeleteCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.SoftDeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(TnCTemplateMasterBuilders.ValidDeleteCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

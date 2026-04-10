using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Command.CreateTnCTemplateMasterCommand;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.TnCTemplateMaster.Commands
{
    public sealed class CreateTnCTemplateMasterCommandHandlerTests
    {
        private readonly Mock<ITnCTemplateMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ITnCTemplateMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<ITnCTemplateCodeGenerator> _mockCodeGenerator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateTnCTemplateMasterCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockCommandRepo.Object, _mockCodeGenerator.Object,
                _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = TnCTemplateMasterBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockCodeGenerator
                .Setup(g => g.GenerateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TNC001");

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 5);

            var result = await CreateSut().Handle(TnCTemplateMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCodeGeneratorOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(TnCTemplateMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCodeGenerator.Verify(
                g => g.GenerateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(TnCTemplateMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(TnCTemplateMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

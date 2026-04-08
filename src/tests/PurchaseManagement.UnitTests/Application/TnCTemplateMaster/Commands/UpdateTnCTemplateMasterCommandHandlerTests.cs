using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Command.UpdateTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.TnCTemplateMaster.Commands
{
    public sealed class UpdateTnCTemplateMasterCommandHandlerTests
    {
        private readonly Mock<ITnCTemplateMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ITnCTemplateMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateTnCTemplateMasterCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var dto = TnCTemplateMasterBuilders.ValidDto(id);
            var entity = TnCTemplateMasterBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockMapper
                .Setup(m => m.Map<List<PurchaseManagement.Domain.Entities.TnCTemplateApplicability>>(It.IsAny<object>()))
                .Returns(new List<PurchaseManagement.Domain.Entities.TnCTemplateApplicability>());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(),
                    It.IsAny<List<PurchaseManagement.Domain.Entities.TnCTemplateApplicability>?>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(TnCTemplateMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(TnCTemplateMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(),
                    It.IsAny<List<PurchaseManagement.Domain.Entities.TnCTemplateApplicability>?>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(TnCTemplateMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_FetchesExistingRecord()
        {
            SetupHappyPath(id: 1);

            await CreateSut().Handle(TnCTemplateMasterBuilders.ValidUpdateCommand(id: 1), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}

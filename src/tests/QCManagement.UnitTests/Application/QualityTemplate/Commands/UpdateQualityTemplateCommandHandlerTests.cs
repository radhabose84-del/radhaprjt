using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualityTemplate.Commands.UpdateQualityTemplate;
using QCManagement.Domain.Events;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualityTemplate.Commands
{
    public class UpdateQualityTemplateCommandHandlerTests
    {
        private readonly Mock<IQualityTemplateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IQualityTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateQualityTemplateCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(UpdateQualityTemplateCommand command, int returnId = 1)
        {
            var entity = QualityTemplateBuilders.ValidEntity(command.Id);
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualityTemplate>(command)).Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<QCManagement.Domain.Entities.QualityTemplate>()))
                .ReturnsAsync(returnId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = QualityTemplateBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsParametersWithCorrectIsActive()
        {
            var command = QualityTemplateBuilders.ValidUpdateCommand();
            command.Parameters![0].IsActive = 0;

            QCManagement.Domain.Entities.QualityTemplate? captured = null;
            var entity = QualityTemplateBuilders.ValidEntity(command.Id);
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualityTemplate>(command)).Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<QCManagement.Domain.Entities.QualityTemplate>()))
                .Callback<QCManagement.Domain.Entities.QualityTemplate>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            captured!.QualityTemplateParameters!.First().IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync_Once()
        {
            var command = QualityTemplateBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<QCManagement.Domain.Entities.QualityTemplate>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = QualityTemplateBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "QUALITY_TEMPLATE_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

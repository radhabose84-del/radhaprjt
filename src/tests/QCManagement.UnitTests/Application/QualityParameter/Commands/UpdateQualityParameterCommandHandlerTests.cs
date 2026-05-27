using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Application.QualityParameter.Commands.UpdateQualityParameter;
using QCManagement.Domain.Events;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualityParameter.Commands
{
    public class UpdateQualityParameterCommandHandlerTests
    {
        private readonly Mock<IQualityParameterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IQualityParameterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateQualityParameterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(UpdateQualityParameterCommand command, int updatedId = 1)
        {
            var entity = QualityParameterBuilders.ValidEntity(command.Id);
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualityParameter>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(updatedId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = QualityParameterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedId()
        {
            var command = QualityParameterBuilders.ValidUpdateCommand(id: 5);
            SetupHappyPath(command, updatedId: 5);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync_Once()
        {
            var command = QualityParameterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<QCManagement.Domain.Entities.QualityParameter>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = QualityParameterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "QUALITY_PARAMETER_UPDATE" &&
                        e.Module == "QualityParameter"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

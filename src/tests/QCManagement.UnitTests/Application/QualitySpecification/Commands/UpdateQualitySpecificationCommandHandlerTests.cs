using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.QualitySpecification.Commands.UpdateQualitySpecification;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualitySpecification.Commands
{
    public class UpdateQualitySpecificationCommandHandlerTests
    {
        private readonly Mock<IQualitySpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IQualitySpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateQualitySpecificationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(UpdateQualitySpecificationCommand command, int returnId = 1)
        {
            var entity = QualitySpecificationBuilders.ValidEntity(command.Id);
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualitySpecification>(command)).Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<QCManagement.Domain.Entities.QualitySpecification>()))
                .ReturnsAsync(returnId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = QualitySpecificationBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsParametersWithCorrectIsActive()
        {
            var command = QualitySpecificationBuilders.ValidUpdateCommand();
            command.Parameters![0].IsActive = 0;

            QCManagement.Domain.Entities.QualitySpecification? captured = null;
            var entity = QualitySpecificationBuilders.ValidEntity(command.Id);
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualitySpecification>(command)).Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<QCManagement.Domain.Entities.QualitySpecification>()))
                .Callback<QCManagement.Domain.Entities.QualitySpecification>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            captured!.QualitySpecificationParameters!.First().IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task Handle_ValidCommand_PreservesRowId()
        {
            var command = QualitySpecificationBuilders.ValidUpdateCommand();
            command.Parameters![0].Id = 99;

            QCManagement.Domain.Entities.QualitySpecification? captured = null;
            var entity = QualitySpecificationBuilders.ValidEntity(command.Id);
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualitySpecification>(command)).Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<QCManagement.Domain.Entities.QualitySpecification>()))
                .Callback<QCManagement.Domain.Entities.QualitySpecification>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            captured!.QualitySpecificationParameters!.First().Id.Should().Be(99);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = QualitySpecificationBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<QCManagement.Domain.Entities.QualitySpecification>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = QualitySpecificationBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "QUALITY_SPECIFICATION_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

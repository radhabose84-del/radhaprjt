using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Application.QualityParameter.Commands.CreateQualityParameter;
using QCManagement.Domain.Events;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualityParameter.Commands
{
    public class CreateQualityParameterCommandHandlerTests
    {
        private readonly Mock<IQualityParameterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateQualityParameterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(CreateQualityParameterCommand command, int newId = 1, int maxSeq = 0)
        {
            var entity = QualityParameterBuilders.ValidEntity(newId);
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualityParameter>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.GetMaxParameterCodeSequenceAsync()).ReturnsAsync(maxSeq);
            _mockCommandRepo.Setup(r => r.CreateAsync(entity)).ReturnsAsync(newId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = QualityParameterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = QualityParameterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 42);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesNextParameterCode()
        {
            var command = QualityParameterBuilders.ValidCreateCommand();
            QCManagement.Domain.Entities.QualityParameter? captured = null;
            var entity = new QCManagement.Domain.Entities.QualityParameter();
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualityParameter>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.GetMaxParameterCodeSequenceAsync()).ReturnsAsync(5);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualityParameter>()))
                .Callback<QCManagement.Domain.Entities.QualityParameter>(e => captured = e)
                .ReturnsAsync(6);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.ParameterCode.Should().Be("QP-000006");
        }

        [Fact]
        public async Task Handle_FirstParameter_GeneratesQP_000001()
        {
            var command = QualityParameterBuilders.ValidCreateCommand();
            QCManagement.Domain.Entities.QualityParameter? captured = null;
            var entity = new QCManagement.Domain.Entities.QualityParameter();
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualityParameter>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.GetMaxParameterCodeSequenceAsync()).ReturnsAsync(0);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualityParameter>()))
                .Callback<QCManagement.Domain.Entities.QualityParameter>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            captured!.ParameterCode.Should().Be("QP-000001");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = QualityParameterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualityParameter>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = QualityParameterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "QUALITY_PARAMETER_CREATE" &&
                        e.Module == "QualityParameter"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

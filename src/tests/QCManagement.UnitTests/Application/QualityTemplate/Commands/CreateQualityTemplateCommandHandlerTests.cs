using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualityTemplate.Commands.CreateQualityTemplate;
using QCManagement.Domain.Events;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualityTemplate.Commands
{
    public class CreateQualityTemplateCommandHandlerTests
    {
        private readonly Mock<IQualityTemplateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IQualityTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateQualityTemplateCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(CreateQualityTemplateCommand command, int newId = 1, int maxSeq = 0)
        {
            var entity = QualityTemplateBuilders.ValidEntity(newId);
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualityTemplate>(command)).Returns(entity);
            _mockQueryRepo.Setup(r => r.GetMaxTemplateCodeSequenceAsync()).ReturnsAsync(maxSeq);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualityTemplate>()))
                .ReturnsAsync(newId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 42);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_FirstTemplate_GeneratesQT_000001()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            QCManagement.Domain.Entities.QualityTemplate? captured = null;
            var entity = new QCManagement.Domain.Entities.QualityTemplate();
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualityTemplate>(command)).Returns(entity);
            _mockQueryRepo.Setup(r => r.GetMaxTemplateCodeSequenceAsync()).ReturnsAsync(0);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualityTemplate>()))
                .Callback<QCManagement.Domain.Entities.QualityTemplate>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            captured!.TemplateCode.Should().Be("QT-000001");
        }

        [Fact]
        public async Task Handle_NthTemplate_GeneratesNextTemplateCode()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            QCManagement.Domain.Entities.QualityTemplate? captured = null;
            var entity = new QCManagement.Domain.Entities.QualityTemplate();
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualityTemplate>(command)).Returns(entity);
            _mockQueryRepo.Setup(r => r.GetMaxTemplateCodeSequenceAsync()).ReturnsAsync(5);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualityTemplate>()))
                .Callback<QCManagement.Domain.Entities.QualityTemplate>(e => captured = e)
                .ReturnsAsync(6);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            captured!.TemplateCode.Should().Be("QT-000006");
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsParameterCollection()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            QCManagement.Domain.Entities.QualityTemplate? captured = null;
            var entity = new QCManagement.Domain.Entities.QualityTemplate();
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualityTemplate>(command)).Returns(entity);
            _mockQueryRepo.Setup(r => r.GetMaxTemplateCodeSequenceAsync()).ReturnsAsync(0);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualityTemplate>()))
                .Callback<QCManagement.Domain.Entities.QualityTemplate>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            captured!.QualityTemplateParameters.Should().HaveCount(2);
            captured.QualityTemplateParameters!.First().QualityParameterId.Should().Be(1);
            captured.QualityTemplateParameters!.First().IsActive.Should().Be(Status.Active);
            captured.QualityTemplateParameters!.First().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualityTemplate>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "QUALITY_TEMPLATE_CREATE" &&
                        e.Module == "QualityTemplate"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

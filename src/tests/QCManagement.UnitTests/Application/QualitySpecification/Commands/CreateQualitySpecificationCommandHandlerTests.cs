using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.QualitySpecification.Commands.CreateQualitySpecification;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualitySpecification.Commands
{
    public class CreateQualitySpecificationCommandHandlerTests
    {
        private readonly Mock<IQualitySpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IQualitySpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateQualitySpecificationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(CreateQualitySpecificationCommand command, int newId = 1, int maxSeq = 0)
        {
            var entity = QualitySpecificationBuilders.ValidEntity(newId);
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualitySpecification>(command)).Returns(entity);
            _mockQueryRepo.Setup(r => r.GetMaxSpecificationCodeSequenceAsync()).ReturnsAsync(maxSeq);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualitySpecification>()))
                .ReturnsAsync(newId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 42);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_FirstSpec_GeneratesQS_0001()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            QCManagement.Domain.Entities.QualitySpecification? captured = null;
            var entity = new QCManagement.Domain.Entities.QualitySpecification();
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualitySpecification>(command)).Returns(entity);
            _mockQueryRepo.Setup(r => r.GetMaxSpecificationCodeSequenceAsync()).ReturnsAsync(0);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualitySpecification>()))
                .Callback<QCManagement.Domain.Entities.QualitySpecification>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            captured!.SpecificationCode.Should().Be("QS-0001");
        }

        [Fact]
        public async Task Handle_NthSpec_GeneratesNextSpecCode()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            QCManagement.Domain.Entities.QualitySpecification? captured = null;
            var entity = new QCManagement.Domain.Entities.QualitySpecification();
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualitySpecification>(command)).Returns(entity);
            _mockQueryRepo.Setup(r => r.GetMaxSpecificationCodeSequenceAsync()).ReturnsAsync(7);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualitySpecification>()))
                .Callback<QCManagement.Domain.Entities.QualitySpecification>(e => captured = e)
                .ReturnsAsync(8);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            captured!.SpecificationCode.Should().Be("QS-0008");
        }

        [Fact]
        public async Task Handle_ListSelection_JoinsAllowedValuesWithPipe()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            QCManagement.Domain.Entities.QualitySpecification? captured = null;
            var entity = new QCManagement.Domain.Entities.QualitySpecification();
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualitySpecification>(command)).Returns(entity);
            _mockQueryRepo.Setup(r => r.GetMaxSpecificationCodeSequenceAsync()).ReturnsAsync(0);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualitySpecification>()))
                .Callback<QCManagement.Domain.Entities.QualitySpecification>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            var listRow = captured!.QualitySpecificationParameters!.Single(p => p.ValidationTypeId == QualitySpecificationBuilders.ValidationTypeListId);
            listRow.AllowedValues.Should().Be("A|B|C");
        }

        [Fact]
        public async Task Handle_NonListSelection_StoresAllowedValuesAsNull()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            QCManagement.Domain.Entities.QualitySpecification? captured = null;
            var entity = new QCManagement.Domain.Entities.QualitySpecification();
            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.QualitySpecification>(command)).Returns(entity);
            _mockQueryRepo.Setup(r => r.GetMaxSpecificationCodeSequenceAsync()).ReturnsAsync(0);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualitySpecification>()))
                .Callback<QCManagement.Domain.Entities.QualitySpecification>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            var rangeRow = captured!.QualitySpecificationParameters!.Single(p => p.ValidationTypeId == QualitySpecificationBuilders.ValidationTypeRangeId);
            rangeRow.AllowedValues.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.QualitySpecification>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "QUALITY_SPECIFICATION_CREATE" &&
                        e.Module == "QualitySpecification"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

using AutoMapper;
using FAM.Application.Common.Interfaces.IMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class CreateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity();
            var dto = MiscTypeMasterBuilders.ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync((FAM.Domain.Entities.MiscTypeMaster?)null);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.MiscTypeMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath();
            var command = MiscTypeMasterBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            var command = MiscTypeMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.MiscTypeMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = MiscTypeMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateCode_ThrowsValidationException()
        {
            var existing = MiscTypeMasterBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(existing);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}

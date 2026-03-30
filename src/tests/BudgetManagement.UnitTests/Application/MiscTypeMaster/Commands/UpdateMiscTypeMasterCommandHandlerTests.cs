using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using MediatR;

namespace BudgetManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class UpdateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath()
        {
            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.MiscTypeMaster?)null);

            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<UpdateMiscTypeMasterCommand>()))
                .Returns(MiscTypeMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<BudgetManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var sut = CreateSut();

            var result = await sut.Handle(MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DuplicateCode_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(MiscTypeMasterBuilders.ValidEntity(2));

            var sut = CreateSut();

            var result = await sut.Handle(MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_UpdateFails_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.MiscTypeMaster?)null);

            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<UpdateMiscTypeMasterCommand>()))
                .Returns(MiscTypeMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<BudgetManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(false);

            var sut = CreateSut();

            var result = await sut.Handle(MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

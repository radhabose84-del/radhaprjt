using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using Contracts.Common;
using MediatR;

namespace BudgetManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class DeleteMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        // Note: The handler class name in the source is DeleteMiscTypeMasterCommandHandler (typo in source)
        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath()
        {
            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.MiscMaster>(It.IsAny<DeleteMiscMasterCommand>()))
                .Returns(MiscMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BudgetManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var sut = CreateSut();

            var result = await sut.Handle(MiscMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsException()
        {
            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.MiscMaster>(It.IsAny<DeleteMiscMasterCommand>()))
                .Returns(MiscMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BudgetManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(false);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(MiscMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(MiscMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Delete"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

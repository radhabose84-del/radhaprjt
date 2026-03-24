using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using MediatR;

namespace BudgetManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class DeleteMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath()
        {
            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<DeleteMiscTypeMasterCommand>()))
                .Returns(MiscTypeMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BudgetManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var sut = CreateSut();

            var result = await sut.Handle(MiscTypeMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DeleteFails_ReturnsFailure()
        {
            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<DeleteMiscTypeMasterCommand>()))
                .Returns(MiscTypeMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BudgetManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(false);

            var sut = CreateSut();

            var result = await sut.Handle(MiscTypeMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(MiscTypeMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Delete"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

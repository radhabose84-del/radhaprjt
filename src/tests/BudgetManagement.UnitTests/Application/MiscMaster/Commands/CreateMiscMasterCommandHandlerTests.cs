using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.CreateMiscMaster;
using BudgetManagement.Application.MiscMaster.Queries.GetMiscMaster;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using Contracts.Common;
using MediatR;

namespace BudgetManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class CreateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = MiscMasterBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.MiscMaster>(It.IsAny<CreateMiscMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BudgetManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(It.IsAny<BudgetManagement.Domain.Entities.MiscMaster>()))
                .Returns(MiscMasterBuilders.ValidDto(newId));
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath(5);
            var sut = CreateSut();

            var result = await sut.Handle(MiscMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(MiscMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<BudgetManagement.Domain.Entities.MiscMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(MiscMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZeroId_ThrowsException()
        {
            var entity = MiscMasterBuilders.ValidEntity(0);

            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.MiscMaster>(It.IsAny<CreateMiscMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BudgetManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(0))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(It.IsAny<BudgetManagement.Domain.Entities.MiscMaster>()))
                .Returns(MiscMasterBuilders.ValidDto(0));

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(MiscMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}

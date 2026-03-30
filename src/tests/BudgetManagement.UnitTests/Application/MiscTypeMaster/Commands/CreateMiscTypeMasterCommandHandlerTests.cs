using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using MediatR;

namespace BudgetManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class CreateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<CreateMiscTypeMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BudgetManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<BudgetManagement.Domain.Entities.MiscTypeMaster>()))
                .Returns(MiscTypeMasterBuilders.ValidDto(newId));
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var sut = CreateSut();

            var result = await sut.Handle(MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCorrectData()
        {
            SetupHappyPath(5);
            var sut = CreateSut();

            var result = await sut.Handle(MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<BudgetManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZeroId_ReturnsFailure()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(0);

            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<CreateMiscTypeMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BudgetManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(entity);

            var sut = CreateSut();

            var result = await sut.Handle(MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}

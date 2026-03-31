using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class CreateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(CreateMiscTypeMasterCommand command, int newId = 1)
        {
            var entity = new UserManagement.Domain.Entities.MiscTypeMaster
            {
                Id = newId,
                MiscTypeCode = command.MiscTypeCode,
                Description = command.Description
            };

            var dto = new GetMiscTypeMasterDto { Id = newId, MiscTypeCode = command.MiscTypeCode, Description = command.Description };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(newId))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = new CreateMiscTypeMasterCommand { MiscTypeCode = "MISC01", Description = "Test Misc Type" };
            SetupHappyPath(command, newId: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCorrectDto()
        {
            var command = new CreateMiscTypeMasterCommand { MiscTypeCode = "MISC01", Description = "Test Misc Type" };
            SetupHappyPath(command, newId: 5);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(5);
            result.Data.MiscTypeCode.Should().Be("MISC01");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = new CreateMiscTypeMasterCommand { MiscTypeCode = "MISC01", Description = "Test Misc Type" };
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = new CreateMiscTypeMasterCommand { MiscTypeCode = "MISC01", Description = "Test Misc Type" };
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "MiscTypeMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateFails_ReturnsFailureResponse()
        {
            var command = new CreateMiscTypeMasterCommand { MiscTypeCode = "MISC01", Description = "Test Misc Type" };
            var entity = new UserManagement.Domain.Entities.MiscTypeMaster { Id = 0, MiscTypeCode = command.MiscTypeCode };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(entity);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}

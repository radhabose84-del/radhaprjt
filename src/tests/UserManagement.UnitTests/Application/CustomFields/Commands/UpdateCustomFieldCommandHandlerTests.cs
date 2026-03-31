using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Application.CustomFields.Commands.UpdateCustomField;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Application.CustomFields.Commands
{
    public sealed class UpdateCustomFieldCommandHandlerTests
    {
        private readonly Mock<ICustomFieldCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateCustomFieldCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UpdateCustomFieldCommand ValidCommand() =>
            new()
            {
                Id = 1,
                LabelName = "Updated Label",
                Length = 100,
                DataTypeId = 1,
                LabelTypeId = 1,
                IsRequired = 1,
                IsActive = 1
            };

        private static CustomField ValidEntity(int id = 1) =>
            new() { Id = id };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<CustomField>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<CustomField>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<CustomField>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<CustomField>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<CustomField>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<CustomField>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<CustomField>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMapper.Verify(m => m.Map<CustomField>(command), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsException()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<CustomField>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<CustomField>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not updated*");
        }
    }
}

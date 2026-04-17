using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Application.CustomFields.Commands.CreateCustomField;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Application.CustomFields.Commands;

public sealed class CreateCustomFieldCommandHandlerTests
{
    private readonly Mock<ICustomFieldCommand> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private CreateCustomFieldCommandhandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewId()
    {
        var command = new CreateCustomFieldCommand
        {
            LabelName = "TestField",
            Length = 100,
            DataTypeId = 1,
            LabelTypeId = 1,
            IsRequired = 1
        };

        _mockMapper.Setup(m => m.Map<CustomField>(It.IsAny<CreateCustomFieldCommand>()))
            .Returns(new CustomField { LabelName = "TestField" });

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<CustomField>()))
            .ReturnsAsync(42);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateOnce()
    {
        var command = new CreateCustomFieldCommand { LabelName = "Test", Length = 50, DataTypeId = 1, LabelTypeId = 1 };

        _mockMapper.Setup(m => m.Map<CustomField>(It.IsAny<CreateCustomFieldCommand>()))
            .Returns(new CustomField());

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<CustomField>()))
            .ReturnsAsync(1);

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<CustomField>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepoReturnsZero_ThrowsValidationException()
    {
        var command = new CreateCustomFieldCommand { LabelName = "Fail", Length = 10, DataTypeId = 1, LabelTypeId = 1 };

        _mockMapper.Setup(m => m.Map<CustomField>(It.IsAny<CreateCustomFieldCommand>()))
            .Returns(new CustomField());

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<CustomField>()))
            .ReturnsAsync(0);

        Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*not created*");
    }
}

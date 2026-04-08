using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IPasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Commands.CreatePasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Queries;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.PasswordComplexityRule.Commands
{
    public sealed class CreatePasswordComplexityRuleCommandHandlerTests
    {
        private readonly Mock<IPasswordComplexityRuleCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreatePasswordComplexityRuleCommandHandler>> _mockLogger = new();

        private CreatePasswordComplexityRuleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            var command = new CreatePasswordComplexityRuleCommand { PwdComplexityRule = "TestRule" };
            var entity = new UserManagement.Domain.Entities.PasswordComplexityRule { Id = 1 };
            var dto = new PwdRuleDto { Id = 1 };

            _mockCommandRepo.Setup(r => r.ExistsByCodeAsync("TestRule")).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.PasswordComplexityRule>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.PasswordComplexityRule>())).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<PwdRuleDto>(entity)).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_DuplicateRule_ThrowsValidationException()
        {
            var command = new CreatePasswordComplexityRuleCommand { PwdComplexityRule = "Existing" };
            _mockCommandRepo.Setup(r => r.ExistsByCodeAsync("Existing")).ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}

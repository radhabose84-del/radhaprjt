using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IPasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Commands.DeletePasswordComplexityRule;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.PasswordComplexityRule.Commands
{
    public sealed class DeletePasswordComplexityRuleCommandHandlerTests
    {
        private readonly Mock<IPasswordComplexityRuleCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPasswordComplexityRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<DeletePasswordComplexityRuleCommandHandler>> _mockLogger = new();

        private DeletePasswordComplexityRuleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsPositiveResult()
        {
            var command = new DeletePasswordComplexityRuleCommand { Id = 1 };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.PasswordComplexityRule>(command))
                .Returns(new UserManagement.Domain.Entities.PasswordComplexityRule());
            _mockCommandRepo.Setup(r => r.DeleteAsync(1, It.IsAny<UserManagement.Domain.Entities.PasswordComplexityRule>())).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(0);
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsException()
        {
            var command = new DeletePasswordComplexityRuleCommand { Id = 99 };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.PasswordComplexityRule>(command))
                .Returns(new UserManagement.Domain.Entities.PasswordComplexityRule());
            _mockCommandRepo.Setup(r => r.DeleteAsync(99, It.IsAny<UserManagement.Domain.Entities.PasswordComplexityRule>())).ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}

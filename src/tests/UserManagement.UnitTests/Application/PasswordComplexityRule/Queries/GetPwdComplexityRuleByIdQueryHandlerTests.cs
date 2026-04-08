using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IPasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRule;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.PasswordComplexityRule.Queries
{
    public sealed class GetPwdComplexityRuleByIdQueryHandlerTests
    {
        private readonly Mock<IPasswordComplexityRuleCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IPasswordComplexityRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetPwdComplexityRuleByIdQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetPwdComplexityRuleByIdQueryHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ExistingRule_ReturnsMappedDto()
        {
            var entity = new UserManagement.Domain.Entities.PasswordComplexityRule { Id = 1 };
            var dto = new GetPwdRuleDto { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetPwdRuleDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetPwdComplexityRuleByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.PasswordComplexityRule?)null!);

            Func<Task> act = () => CreateSut().Handle(
                new GetPwdComplexityRuleByIdQuery { Id = 999 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}

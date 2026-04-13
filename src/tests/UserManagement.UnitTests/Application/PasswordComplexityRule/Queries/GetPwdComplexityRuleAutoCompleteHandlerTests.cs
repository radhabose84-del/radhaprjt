using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IPasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleAutoComplete;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.PasswordComplexityRule.Queries
{
    public sealed class GetPwdComplexityRuleAutoCompleteHandlerTests
    {
        private readonly Mock<IPasswordComplexityRuleCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IPasswordComplexityRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetPwdComplexityRuleAutoCompleteHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetPwdComplexityRuleAutoCompleteHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        private void SetupHappyPath(
            string searchTerm,
            List<UserManagement.Domain.Entities.PasswordComplexityRule> entities,
            List<PwdComplexityRuleAutoCompleteDto> dtos)
        {
            _mockQueryRepo
                .Setup(r => r.GetpwdautocompleteAsync(searchTerm))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<PwdComplexityRuleAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsMappedList()
        {
            var entities = new List<UserManagement.Domain.Entities.PasswordComplexityRule>
            {
                new() { Id = 1, PwdComplexityRule = "MinLength" }
            };
            var dtos = new List<PwdComplexityRuleAutoCompleteDto>
            {
                new() { Id = 1, PwdComplexityRule = "MinLength" }
            };
            SetupHappyPath("Min", entities, dtos);

            var result = await CreateSut().Handle(
                new GetPwdComplexityRuleAutoComplete { SearchTerm = "Min" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].PwdComplexityRule.Should().Be("MinLength");
        }

        [Fact]
        public async Task Handle_EmptyResults_ReturnsEmptyList()
        {
            var entities = new List<UserManagement.Domain.Entities.PasswordComplexityRule>();
            var dtos = new List<PwdComplexityRuleAutoCompleteDto>();
            SetupHappyPath("NONE", entities, dtos);

            var result = await CreateSut().Handle(
                new GetPwdComplexityRuleAutoComplete { SearchTerm = "NONE" },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsQueryRepositoryOnce()
        {
            var entities = new List<UserManagement.Domain.Entities.PasswordComplexityRule>();
            var dtos = new List<PwdComplexityRuleAutoCompleteDto>();
            SetupHappyPath("test", entities, dtos);

            await CreateSut().Handle(
                new GetPwdComplexityRuleAutoComplete { SearchTerm = "test" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetpwdautocompleteAsync("test"), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var entities = new List<UserManagement.Domain.Entities.PasswordComplexityRule>
            {
                new() { Id = 1 }
            };
            var dtos = new List<PwdComplexityRuleAutoCompleteDto> { new() { Id = 1 } };
            SetupHappyPath("Min", entities, dtos);

            await CreateSut().Handle(
                new GetPwdComplexityRuleAutoComplete { SearchTerm = "Min" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

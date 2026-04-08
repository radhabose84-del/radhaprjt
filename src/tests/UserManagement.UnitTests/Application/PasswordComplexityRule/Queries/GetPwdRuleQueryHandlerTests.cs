using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IPasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Queries;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRule;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.PasswordComplexityRule.Queries
{
    public sealed class GetPwdRuleQueryHandlerTests
    {
        private readonly Mock<IPasswordComplexityRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetPwdRuleQueryHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPwdRuleQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.PasswordComplexityRule> { new() { Id = 1 } };
            var dtoList = new List<GetPwdRuleDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetPasswordComplexityAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetPwdRuleDto>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetPwdRuleQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NullResult_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetPasswordComplexityAsync(1, 10, null))
                .ReturnsAsync(((List<UserManagement.Domain.Entities.PasswordComplexityRule>?)null!, 0));

            var result = await CreateSut().Handle(
                new GetPwdRuleQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}

using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.ILanguage;
using UserManagement.Application.Language.Queries.GetLanguages;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Language.Queries
{
    public sealed class GetLanguageQueryHandlerTests
    {
        private readonly Mock<ILanguageCommand> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<ILanguageQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetLanguageQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.Language> { new() { Id = 1 } };
            var dtoList = new List<LanguageDTO> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllLanguageAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<LanguageDTO>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetLanguageQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<UserManagement.Domain.Entities.Language>();

            _mockQueryRepo
                .Setup(r => r.GetAllLanguageAsync(2, 5, "test"))
                .ReturnsAsync((entities, 0));

            _mockMapper
                .Setup(m => m.Map<List<LanguageDTO>>(entities))
                .Returns(new List<LanguageDTO>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetLanguageQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }
    }
}

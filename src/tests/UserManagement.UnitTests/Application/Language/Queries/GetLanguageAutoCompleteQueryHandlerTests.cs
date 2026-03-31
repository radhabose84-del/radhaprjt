using AutoMapper;
using UserManagement.Application.Common.Interfaces.ILanguage;
using UserManagement.Application.Language.Queries.GetLanguageAutoComplete;
using UserManagement.Application.Language.Queries.GetLanguages;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using MediatR;

namespace UserManagement.UnitTests.Application.Language.Queries
{
    public class GetLanguageAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<ILanguageQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetLanguageAutoCompleteQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<UserManagement.Domain.Entities.Language>
            {
                LanguageBuilders.ValidEntity()
            };
            var dtos = new List<LanguageAutoCompleteDTO> { LanguageBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetLanguage("Eng"))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<LanguageAutoCompleteDTO>>(entities))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetLanguageAutoCompleteQuery { SearchPattern = "Eng" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("English");
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var entities = new List<UserManagement.Domain.Entities.Language>();
            var dtos = new List<LanguageAutoCompleteDTO>();

            _mockQueryRepo
                .Setup(r => r.GetLanguage("xyz"))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<LanguageAutoCompleteDTO>>(entities))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetLanguageAutoCompleteQuery { SearchPattern = "xyz" },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}

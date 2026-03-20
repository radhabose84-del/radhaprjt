using AutoMapper;
using UserManagement.Application.Common.Interfaces.ILanguage;
using UserManagement.Application.Language.Queries.GetLanguages;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using MediatR;

namespace UserManagement.UnitTests.Application.Language.Queries
{
    public class GetAllLanguageQueryHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<ILanguageCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILanguageQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private GetLanguageQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var languages = new List<UserManagement.Domain.Entities.Language>
            {
                LanguageBuilders.ValidEntity()
            };
            var dtos = new List<LanguageDTO> { LanguageBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllLanguageAsync(1, 10, null))
                .ReturnsAsync((languages, 1));
            _mockMapper
                .Setup(m => m.Map<List<LanguageDTO>>(languages))
                .Returns(dtos);
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
            var languages = new List<UserManagement.Domain.Entities.Language>
            {
                LanguageBuilders.ValidEntity()
            };
            var dtos = new List<LanguageDTO> { LanguageBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllLanguageAsync(2, 5, "eng"))
                .ReturnsAsync((languages, 11));
            _mockMapper
                .Setup(m => m.Map<List<LanguageDTO>>(languages))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetLanguageQuery { PageNumber = 2, PageSize = 5, SearchTerm = "eng" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllLanguageAsync(1, 15, null))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.Language>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<LanguageDTO>>(It.IsAny<List<UserManagement.Domain.Entities.Language>>()))
                .Returns(new List<LanguageDTO>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetLanguageQuery(),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using UserManagement.Application.IconMaster.Queries.GetIconMaster;
using UserManagement.Application.IconMaster.Queries.GetIconMasterAutoComplete;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.IconMaster.Queries
{
    public sealed class GetIconMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IIconMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetIconMasterAutocompleteQueryHandler>> _mockLogger = new();

        private GetIconMasterAutocompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ReturnsListWithAllFields()
        {
            var entities = new List<UserManagement.Domain.Entities.IconMaster> { IconMasterBuilders.ValidEntity() };
            var dtos = new List<IconMasterAutoCompleteDto> { IconMasterBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo.Setup(r => r.GetByKeywordAsync("set")).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<IconMasterAutoCompleteDto>>(entities)).Returns(dtos);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetIconMasterAutocompleteQuery { SearchPattern = "set" }, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].IconName.Should().Be("SlSettings");
            result[0].IconLibrary.Should().Be("sl");
            result[0].Size.Should().Be(18);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var entities = new List<UserManagement.Domain.Entities.IconMaster>();
            var dtos = new List<IconMasterAutoCompleteDto>();

            _mockQueryRepo.Setup(r => r.GetByKeywordAsync("zzz")).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<IconMasterAutoCompleteDto>>(entities)).Returns(dtos);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetIconMasterAutocompleteQuery { SearchPattern = "zzz" }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}

using AutoMapper;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<PartyManagement.Domain.Entities.MiscTypeMaster> { MiscTypeMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscTypeMasterAutocompleteDto> { new() { Id = 1, MiscTypeCode = "MTY001" } };

            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster("MTY"))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "MTY" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster(It.IsAny<string>()))
                .ReturnsAsync(new List<PartyManagement.Domain.Entities.MiscTypeMaster>());

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "NONE" }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}

using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster(It.IsAny<string>()))
                .ReturnsAsync(new List<PurchaseManagement.Domain.Entities.MiscTypeMaster>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "TYP" },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<PurchaseManagement.Domain.Entities.MiscTypeMaster>
            {
                new() { Id = 1, MiscTypeCode = "TYP001" }
            };
            var dtos = new List<GetMiscTypeMasterAutocompleteDto>
            {
                new() { Id = 1, MiscTypeCode = "TYP001" }
            };
            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster(It.IsAny<string>()))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "TYP" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }
    }
}

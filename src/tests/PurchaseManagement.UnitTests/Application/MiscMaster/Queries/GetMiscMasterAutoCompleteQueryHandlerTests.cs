using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMaster;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<PurchaseManagement.Domain.Entities.MiscMaster>());
            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "MSC", MiscTypeCode = "TYPE" },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsItems()
        {
            var entities = new List<PurchaseManagement.Domain.Entities.MiscMaster> { new() { Id = 1, Code = "MSC001" } };
            var dtos = new List<GetMiscMasterAutoCompleteDto> { new() { Id = 1, Code = "MSC001" } };
            _mockQueryRepo
                .Setup(r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "MSC", MiscTypeCode = "TYPE" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}

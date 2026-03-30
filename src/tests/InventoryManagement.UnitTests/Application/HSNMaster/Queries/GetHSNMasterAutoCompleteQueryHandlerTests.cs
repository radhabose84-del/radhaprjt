using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterAutoComplete;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.HSNMaster.Queries
{
    public sealed class GetHSNMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IHSNMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetHSNMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsAutoCompleteList()
        {
            var autoCompleteList = HSNMasterBuilders.ValidAutoCompleteList();
            _mockQueryRepo
                .Setup(r => r.GetHSNMasterAutoCompleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(autoCompleteList);
            _mockMapper
                .Setup(m => m.Map<List<GetHSNMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(autoCompleteList);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetHSNMasterAutoCompleteQuery { SearchPattern = "100", TypeCode = "GOODS" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptySearchPattern_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetHSNMasterAutoCompleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<GetHSNMasterAutoCompleteDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetHSNMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<GetHSNMasterAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetHSNMasterAutoCompleteQuery { SearchPattern = null, TypeCode = null },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            var autoCompleteList = HSNMasterBuilders.ValidAutoCompleteList();
            _mockQueryRepo
                .Setup(r => r.GetHSNMasterAutoCompleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(autoCompleteList);
            _mockMapper
                .Setup(m => m.Map<List<GetHSNMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(autoCompleteList);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetHSNMasterAutoCompleteQuery { SearchPattern = "1" }, CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetHSNMasterAutoCompleteAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}

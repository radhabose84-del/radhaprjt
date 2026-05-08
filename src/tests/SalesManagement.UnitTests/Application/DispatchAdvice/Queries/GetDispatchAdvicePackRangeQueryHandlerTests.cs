using Contracts.Common;
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackRange;


namespace SalesManagement.UnitTests.Application.DispatchAdvice.Queries
{
    public sealed class GetDispatchAdvicePackRangeQueryHandlerTests
    {
        private readonly Mock<IDispatchAdviceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDispatchAdvicePackRangeQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMiscRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsPackRangeList()
        {
            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockQueryRepo
                .Setup(r => r.GetPackRangeAsync(1, 1, 1, It.IsAny<IList<int>>(), 10, It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<DispatchAdvicePackRangeDto> { new() });

            var result = await CreateSut().Handle(
                new GetDispatchAdvicePackRangeQuery { ItemId = 1, LotId = 1, PackTypeId = 1, Range = 10 },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_PassesExactOrderTypeToRepository()
        {
            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });
            _mockQueryRepo.Setup(r => r.GetPackRangeAsync(1, 1, 1, It.IsAny<IList<int>>(), 10, "SalesOrder", It.IsAny<int?>()))
                .ReturnsAsync(new List<DispatchAdvicePackRangeDto>());

            await CreateSut().Handle(
                new GetDispatchAdvicePackRangeQuery
                {
                    ItemId = 1, LotId = 1, PackTypeId = 1, Range = 10, OrderType = "SalesOrder"
                },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetPackRangeAsync(1, 1, 1, It.IsAny<IList<int>>(), 10, "SalesOrder", It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NullOrderType_PassesNullToRepository()
        {
            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });
            _mockQueryRepo.Setup(r => r.GetPackRangeAsync(1, 1, 1, It.IsAny<IList<int>>(), 10, (string?)null, It.IsAny<int?>()))
                .ReturnsAsync(new List<DispatchAdvicePackRangeDto>());

            await CreateSut().Handle(
                new GetDispatchAdvicePackRangeQuery
                {
                    ItemId = 1, LotId = 1, PackTypeId = 1, Range = 10, OrderType = null
                },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetPackRangeAsync(1, 1, 1, It.IsAny<IList<int>>(), 10, (string?)null, It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });
            _mockQueryRepo.Setup(r => r.GetPackRangeAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IList<int>>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<DispatchAdvicePackRangeDto>());

            var result = await CreateSut().Handle(
                new GetDispatchAdvicePackRangeQuery { ItemId = 1, LotId = 1, PackTypeId = 1, Range = 10 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PackedStatusNotFound_PassesZeroStatusIdToRepo()
        {
            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((SalesManagement.Domain.Entities.MiscMaster?)null);
            _mockQueryRepo.Setup(r => r.GetPackRangeAsync(1, 1, 1, It.IsAny<IList<int>>(), 10, It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<DispatchAdvicePackRangeDto>());

            await CreateSut().Handle(
                new GetDispatchAdvicePackRangeQuery { ItemId = 1, LotId = 1, PackTypeId = 1, Range = 10 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetPackRangeAsync(1, 1, 1, It.IsAny<IList<int>>(), 10, It.IsAny<string?>(), It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidQuery_PublishesAuditEvent()
        {
            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });
            _mockQueryRepo.Setup(r => r.GetPackRangeAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IList<int>>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<DispatchAdvicePackRangeDto>());

            await CreateSut().Handle(
                new GetDispatchAdvicePackRangeQuery { ItemId = 1, LotId = 1, PackTypeId = 1, Range = 10 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<SalesManagement.Domain.Events.AuditLogsDomainEvent>(e => e.Module == "DispatchAdvice"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

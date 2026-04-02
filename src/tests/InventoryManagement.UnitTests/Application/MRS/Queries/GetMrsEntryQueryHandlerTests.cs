using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Application.MRS.Queries.GetMrsEntry;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MRS.Queries
{
    public sealed class GetMrsEntryQueryHandlerTests
    {
        private readonly Mock<IMrsEntryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWHLookup = new(MockBehavior.Loose);

        private GetMrsEntryQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockDeptLookup.Object, _mockWHLookup.Object);

        [Fact]
        public async Task Handle_ReturnsPaginatedResult()
        {
            var dtos = new List<GetMrsEntryDto> { new() };
            _mockQueryRepo.Setup(r => r.GetMrsEntryDetails(1, 10, null, null, null))
                .ReturnsAsync((dtos, 1));
            _mockMapper.Setup(m => m.Map<List<GetMrsEntryDto>>(It.IsAny<object>())).Returns(dtos);
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMrsEntryQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResults_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetMrsEntryDetails(1, 10, null, null, null))
                .ReturnsAsync((new List<GetMrsEntryDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<GetMrsEntryDto>>(It.IsAny<object>()))
                .Returns(new List<GetMrsEntryDto>());
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMrsEntryQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}

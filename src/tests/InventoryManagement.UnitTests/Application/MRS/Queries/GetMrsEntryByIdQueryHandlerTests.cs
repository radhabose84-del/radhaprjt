using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Application.MRS.Queries.GetMrsEntryById;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MRS.Queries
{
    public sealed class GetMrsEntryByIdQueryHandlerTests
    {
        private readonly Mock<IMrsEntryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetMrsEntryByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new GetMrsEntryByIdDto();
            _mockQueryRepo.Setup(r => r.GetMrsDetailsById(1)).ReturnsAsync(dto);
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMrsEntryByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            var dto = new GetMrsEntryByIdDto();
            _mockQueryRepo.Setup(r => r.GetMrsDetailsById(5)).ReturnsAsync(dto);
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMrsEntryByIdQuery { Id = 5 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetMrsDetailsById(5), Times.Once);
        }
    }
}

using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentById;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Application.PurchaseIndent.Queries
{
    public sealed class GetPurchaseIndentByIdQueryHandlerTests
    {
        private readonly Mock<IPurchaseIndentQuery> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IInventoryCategoryLookup> _mockCategoryLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetPurchaseIndentByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockItemLookup.Object, _mockUomLookup.Object, _mockCategoryLookup.Object,
                _mockDeptLookup.Object, _mockUnitLookup.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsMappedDto()
        {
            var header = new IndentHeader
            {
                Id = 1,
                IndentNumber = "IND001",
                DepartmentId = 1,
                UnitId = 1,
                IndentDetails = new List<IndentDetail>()
            };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(header);

            _mockMapper
                .Setup(m => m.Map<IndentByIdDto>(It.IsAny<object>()))
                .Returns(new IndentByIdDto
                {
                    Id = 1,
                    IndentNumber = "IND001",
                    DepartmentId = 1,
                    UnitId = 1,
                    IndentDetails = new List<IndentDetailByIdDto>()
                });

            _mockDeptLookup
                .Setup(l => l.GetAllDepartmentAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.DepartmentLookupDto>());

            _mockUnitLookup
                .Setup(l => l.GetAllUnitAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>());

            var result = await CreateSut().Handle(
                new GetPurchaseIndentByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }
    }
}

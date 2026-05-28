using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Dto;
using PurchaseManagement.Application.VendorRatingGrade.Queries.GetAllVendorRatingGrade;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.VendorRatingGrade.Queries
{
    public sealed class GetAllVendorRatingGradeQueryHandlerTests
    {
        private readonly Mock<IVendorRatingGradeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllVendorRatingGradeQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<VendorRatingGradeDto> { VendorRatingGradeBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<VendorRatingGradeDto>>(It.IsAny<object>())).Returns(dtoList);
            var result = await CreateSut().Handle(new GetAllVendorRatingGradeQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<VendorRatingGradeDto> { VendorRatingGradeBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "Excellent")).ReturnsAsync((dtoList, 11));
            _mockMapper.Setup(m => m.Map<List<VendorRatingGradeDto>>(It.IsAny<object>())).Returns(dtoList);
            var result = await CreateSut().Handle(new GetAllVendorRatingGradeQuery { PageNumber = 2, PageSize = 5, SearchTerm = "Excellent" }, CancellationToken.None);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<VendorRatingGradeDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<VendorRatingGradeDto>>(It.IsAny<object>())).Returns(new List<VendorRatingGradeDto>());
            var result = await CreateSut().Handle(new GetAllVendorRatingGradeQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}

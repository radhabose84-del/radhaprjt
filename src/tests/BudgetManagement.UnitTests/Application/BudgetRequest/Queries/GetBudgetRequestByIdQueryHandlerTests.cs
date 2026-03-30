using BudgetManagement.Application.BudgetRequest;
using BudgetManagement.Application.BudgetRequest.Queries.GetById;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.UnitTests.TestData;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;

namespace BudgetManagement.UnitTests.Application.BudgetRequest.Queries
{
    public sealed class GetBudgetRequestByIdQueryHandlerTests
    {
        private readonly Mock<IBudgetRequestQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private GetBudgetRequestByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockCompanyLookup.Object, _mockUnitLookup.Object, _mockIp.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = BudgetRequestBuilders.ValidDto(5);
            dto.ImagePath = null;

            _mockRepo
                .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetBudgetRequestByIdQuery { Id = 5 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(5);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsException()
        {
            _mockRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetRequestDto?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetBudgetRequestByIdQuery { Id = 99 },
                CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*99*");
        }

        [Fact]
        public async Task Handle_NoImagePath_DoesNotCallCompanyLookup()
        {
            var dto = BudgetRequestBuilders.ValidDto(1);
            dto.ImagePath = null;

            _mockRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            await CreateSut().Handle(
                new GetBudgetRequestByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockCompanyLookup.Verify(
                l => l.GetAllCompanyAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WithImagePath_BuildsImageUrl()
        {
            var dto = BudgetRequestBuilders.ValidDto(1);
            dto.ImagePath = "request.jpg";

            _mockRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            _mockRepo
                .Setup(r => r.GetBaseDirectoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("http://base.dir");

            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);

            _mockCompanyLookup
                .Setup(l => l.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto>
                {
                    new CompanyLookupDto { CompanyId = 1, CompanyName = "TestCo" }
                });

            _mockUnitLookup
                .Setup(l => l.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitLookupDto { UnitId = 1, ShortName = "TU", UnitName = "Test Unit" });

            var result = await CreateSut().Handle(
                new GetBudgetRequestByIdQuery { Id = 1 },
                CancellationToken.None);

            result.ImageUrl.Should().Contain("request.jpg");
        }
    }
}

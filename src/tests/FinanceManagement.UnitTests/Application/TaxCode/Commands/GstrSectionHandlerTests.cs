using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionAccountLinkage;
using FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionMaster;

namespace FinanceManagement.UnitTests.Application.TaxCode.Commands
{
    public sealed class GstrSectionHandlerTests
    {
        private readonly Mock<IGstrSectionCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        public GstrSectionHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
        }

        [Fact]
        public async Task CreateSection_ReturnsNewId_AndSetsCompanyFromToken()
        {
            FinanceManagement.Domain.Entities.GstrSectionMaster? captured = null;
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.GstrSectionMaster>(It.IsAny<CreateGstrSectionMasterCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.GstrSectionMaster());
            _mockCommandRepo.Setup(r => r.CreateSectionAsync(It.IsAny<FinanceManagement.Domain.Entities.GstrSectionMaster>()))
                .Callback<FinanceManagement.Domain.Entities.GstrSectionMaster>(e => captured = e)
                .ReturnsAsync(11);

            var sut = new CreateGstrSectionMasterCommandHandler(_mockCommandRepo.Object, _mockIp.Object, _mockMediator.Object, _mockMapper.Object);
            var result = await sut.Handle(new CreateGstrSectionMasterCommand { ReportTypeId = 36, SectionCode = "4A", SectionName = "Taxable outward supplies (B2B)" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(11);
            captured!.CompanyId.Should().Be(1);
        }

        [Fact]
        public async Task CreateLinkage_ReturnsNewId()
        {
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.GstrSectionAccountLinkage>(It.IsAny<CreateGstrSectionAccountLinkageCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.GstrSectionAccountLinkage());
            _mockCommandRepo.Setup(r => r.CreateLinkageAsync(It.IsAny<FinanceManagement.Domain.Entities.GstrSectionAccountLinkage>()))
                .ReturnsAsync(7);

            var sut = new CreateGstrSectionAccountLinkageCommandHandler(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);
            var result = await sut.Handle(new CreateGstrSectionAccountLinkageCommand
            {
                SectionMasterId = 11,
                AccountRangeFrom = "6100101",
                AccountRangeTo = "6100199",
                ExpectedValue = 8_400_000m,
                TolerancePercent = 1m
            }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(7);
        }
    }
}

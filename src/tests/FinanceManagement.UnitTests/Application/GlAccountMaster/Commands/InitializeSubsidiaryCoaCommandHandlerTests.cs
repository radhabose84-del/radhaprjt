using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.InitializeSubsidiaryCoa;

namespace FinanceManagement.UnitTests.Application.GlAccountMaster.Commands
{
    public sealed class InitializeSubsidiaryCoaCommandHandlerTests
    {
        private readonly Mock<IGlobalCoaPropagationService> _mockPropagation = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private InitializeSubsidiaryCoaCommandHandler CreateSut() =>
            new(_mockPropagation.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_InheritsForCompany_AndReturnsCreatedCount()
        {
            _mockPropagation.Setup(p => p.InheritForCompanyAsync(8, It.IsAny<CancellationToken>())).ReturnsAsync(12);

            var result = await CreateSut().Handle(
                new InitializeSubsidiaryCoaCommand { CompanyId = 8 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(12);
            _mockPropagation.Verify(p => p.InheritForCompanyAsync(8, It.IsAny<CancellationToken>()), Times.Once);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

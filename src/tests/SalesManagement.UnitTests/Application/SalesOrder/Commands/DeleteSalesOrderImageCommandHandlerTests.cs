using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrderImage;

namespace SalesManagement.UnitTests.Application.SalesOrder.Commands;

public sealed class DeleteSalesOrderImageCommandHandlerTests
{
    private readonly Mock<IFileUploadService> _mockFileUpload = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
    private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
    private readonly Mock<ILogger<DeleteSalesOrderImageCommandHandler>> _mockLogger = new(MockBehavior.Loose);

    private DeleteSalesOrderImageCommandHandler CreateSut() =>
        new(_mockFileUpload.Object, _mockMediator.Object, _mockIpService.Object,
            _mockCompanyLookup.Object, _mockUnitLookup.Object, _mockLogger.Object);

    [Fact]
    public async Task Handle_NullFilePath_ReturnsFalse()
    {
        var command = new DeleteSalesOrderImageCommand { FilePath = null };
        var result = await CreateSut().Handle(command, CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ValidFilePath_CallsDeleteFileAsync()
    {
        _mockIpService.Setup(s => s.GetCompanyId()).Returns(1);
        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
        _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
            .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.CompanyLookupDto>
            {
                new() { CompanyId = 1, CompanyName = "TestCo" }
            });
        _mockUnitLookup.Setup(u => u.GetAllUnitAsync())
            .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>
            {
                new() { UnitId = 1, UnitName = "TestUnit" }
            });
        _mockFileUpload.Setup(f => f.DeleteFileAsync(It.IsAny<string>())).ReturnsAsync(true);

        var command = new DeleteSalesOrderImageCommand { FilePath = "test.jpg" };
        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().BeTrue();
    }
}

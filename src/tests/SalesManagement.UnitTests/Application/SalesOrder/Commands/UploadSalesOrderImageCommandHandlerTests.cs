using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.SalesOrder.Commands.UploadSalesOrderImage;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrder.Commands;

public sealed class UploadSalesOrderImageCommandHandlerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
    private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
    private readonly Mock<ILogger<UploadSalesOrderImageCommandHandler>> _mockLogger = new(MockBehavior.Loose);

    private UploadSalesOrderImageCommandHandler CreateSut() =>
        new(_mockMediator.Object, _mockIpService.Object,
            _mockCompanyLookup.Object, _mockUnitLookup.Object, _mockLogger.Object);

    [Fact]
    public async Task Handle_NullFile_ThrowsExceptionRules()
    {
        var command = new UploadSalesOrderImageCommand { File = null };

        Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*File is required*");
    }

    [Fact]
    public async Task Handle_EmptyFile_ThrowsExceptionRules()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);

        var command = new UploadSalesOrderImageCommand { File = mockFile.Object };

        Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*File is required*");
    }
}

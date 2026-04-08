using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UploadAttachment;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintDepartmentFeedback.Commands;

public sealed class UploadFeedbackAttachmentCommandHandlerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
    private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
    private readonly Mock<ILogger<UploadFeedbackAttachmentCommandHandler>> _mockLogger = new(MockBehavior.Loose);

    private UploadFeedbackAttachmentCommandHandler CreateSut() =>
        new(_mockMediator.Object, _mockIpService.Object,
            _mockCompanyLookup.Object, _mockUnitLookup.Object, _mockLogger.Object);

    [Fact]
    public async Task Handle_NullFile_ThrowsExceptionRules()
    {
        var command = new UploadFeedbackAttachmentCommand { File = null };

        Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*File is required*");
    }

    [Fact]
    public async Task Handle_EmptyFile_ThrowsExceptionRules()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);

        var command = new UploadFeedbackAttachmentCommand { File = mockFile.Object };

        Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*File is required*");
    }
}

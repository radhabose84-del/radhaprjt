using Contracts.Common;

namespace Shared.Infrastructure.UnitTests.Exceptions;

public sealed class ForbiddenExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsStandardMessage()
    {
        var ex = new ForbiddenException();
        ex.Message.Should().Be("You do not have permission to perform this action.");
    }

    [Fact]
    public void CustomMessage_Constructor_SetsProvidedMessage()
    {
        var ex = new ForbiddenException("CanAdd permission required.");
        ex.Message.Should().Be("CanAdd permission required.");
    }

    [Fact]
    public void ForbiddenException_IsException()
    {
        typeof(Exception).IsAssignableFrom(typeof(ForbiddenException)).Should().BeTrue();
    }

    [Fact]
    public void ForbiddenException_CanBeCaughtAsException()
    {
        Exception? caught = null;
        try { throw new ForbiddenException(); }
        catch (Exception ex) { caught = ex; }

        caught.Should().BeOfType<ForbiddenException>();
    }
}

using Shared.Infrastructure.Resilience;

namespace Shared.Infrastructure.UnitTests.Resilience;

public sealed class TransientExceptionDetectionTests
{
    [Fact]
    public void IsTransientMongoException_TimeoutException_True()
    {
        ResiliencePipelineProvider.IsTransientMongoException(new TimeoutException()).Should().BeTrue();
    }

    [Fact]
    public void IsTransientMongoException_IOException_True()
    {
        ResiliencePipelineProvider.IsTransientMongoException(new IOException()).Should().BeTrue();
    }

    [Fact]
    public void IsTransientMongoException_GenericException_False()
    {
        ResiliencePipelineProvider.IsTransientMongoException(new InvalidOperationException()).Should().BeFalse();
    }

    [Fact]
    public void IsTransientSqlException_TimeoutException_True()
    {
        ResiliencePipelineProvider.IsTransientSqlException(new TimeoutException()).Should().BeTrue();
    }

    [Fact]
    public void IsTransientSqlException_IOException_True()
    {
        ResiliencePipelineProvider.IsTransientSqlException(new IOException()).Should().BeTrue();
    }

    [Fact]
    public void IsTransientSqlException_RandomException_False()
    {
        ResiliencePipelineProvider.IsTransientSqlException(new ArgumentException("bad input")).Should().BeFalse();
    }

    [Fact]
    public void IsTransientSignalRException_HttpRequestException_True()
    {
        ResiliencePipelineProvider.IsTransientSignalRException(new HttpRequestException()).Should().BeTrue();
    }

    [Fact]
    public void IsTransientSignalRException_TimeoutException_True()
    {
        ResiliencePipelineProvider.IsTransientSignalRException(new TimeoutException()).Should().BeTrue();
    }

    [Fact]
    public void IsTransientSignalRException_NullReference_False()
    {
        ResiliencePipelineProvider.IsTransientSignalRException(new NullReferenceException()).Should().BeFalse();
    }
}

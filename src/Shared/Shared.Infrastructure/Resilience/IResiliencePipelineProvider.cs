using Polly;

namespace Shared.Infrastructure.Resilience;

public interface IResiliencePipelineProvider
{
    ResiliencePipeline GetMongoPipeline(string profileName);

    ResiliencePipeline GetSqlPipeline(string profileName);

    ResiliencePipeline GetSignalRPipeline(string profileName);

    ResiliencePipeline GetGenericPipeline(string profileName, Func<Exception, bool> isTransient);

    ResilienceProfile GetProfile(string profileName);
}

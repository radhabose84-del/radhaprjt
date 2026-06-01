namespace Shared.QAInfrastructure.Infrastructure;

[AttributeUsage(AttributeTargets.Method)]
public sealed class TestPriorityAttribute : Attribute
{
    public TestPriorityAttribute(int priority) => Priority = priority;
    public int Priority { get; }
}

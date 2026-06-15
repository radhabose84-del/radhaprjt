namespace Contracts.Common;

public class ExceptionRules : Exception
{
    public ExceptionRules(string message) : base(message) { }
}

public class EntityAlreadyExistsException : ExceptionRules
{
    public string? Entity { get; }
    public string? Field { get; }
    public string? Value { get; }

    public EntityAlreadyExistsException(string entity, string field, string value)
        : base($"{entity} already exists with {field} = '{value}'.")
    {
        Entity = entity;
        Field = field;
        Value = value;
    }

    public EntityAlreadyExistsException(string message) : base(message) { }
}

public class EntityNotFoundException : ExceptionRules
{
    public string? Entity { get; }
    public string? Field { get; }
    public string? Value { get; }

    public EntityNotFoundException(string entity, string field, string value)
        : base($"{entity} with {field} '{value}' was not found.")
    {
        Entity = entity;
        Field = field;
        Value = value;
    }

    public EntityNotFoundException(string entity, object key)
        : this(entity, "Id", key?.ToString() ?? "NULL") { }

    public EntityNotFoundException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("You do not have permission to perform this action.") { }
    public ForbiddenException(string message) : base(message) { }
}

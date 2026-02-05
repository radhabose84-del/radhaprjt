
namespace InventoryManagement.Application.Common.Exceptions
{
    public class ExceptionRules : Exception
    {
        public ExceptionRules(string message) : base(message) { }
    }

    /*     public class EntityAlreadyExistsException : ExceptionRules
        {
            public EntityAlreadyExistsException(string entity, string field, string value)
                : base($"{entity} already exists with {field} = '{value}'.") { }
        } */
    public class EntityAlreadyExistsException : Exception
    {
        public string? Entity { get; }
        public string? Field { get; }
        public string? Value { get; }

        public EntityAlreadyExistsException(string entity, string field, string value)
            : base($"{entity} with {field} '{value}' already exists.")
        {
            Entity = entity; Field = field; Value = value;
        }

        // Convenience overload:
        public EntityAlreadyExistsException(string message) : base(message) { }
    }
    public class EntityNotFoundException : Exception
    {
        public string? Entity { get; }
        public string? Field  { get; }
        public string? Value  { get; }

        // Primary ctor (rich, structured)
        public EntityNotFoundException(string entity, string field, string value)
            : base($"{entity} with {field} '{value}' was not found.")
        {
            Entity = entity;
            Field  = field;
            Value  = value;
        }

        // Convenience: Id-based
        public EntityNotFoundException(string entity, object id)
            : this(entity, "Id", id?.ToString() ?? "NULL") { }

        // Convenience: plain message (lets you do throw new EntityNotFoundException("..."))
        public EntityNotFoundException(string message) : base(message) { }
    }

   /*  public class EntityNotFoundException : ExceptionRules
     {
         public EntityNotFoundException(string entity, object key)
             : base($"{entity} with ID '{key}' was not found.") { }
     } */
}
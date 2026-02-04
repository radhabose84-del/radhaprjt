// tests/UserManagement.UnitTests/_Common/TestData/UserBuilder.cs
using System;
using UserManagement.Domain.Entities;
// if you need to set Status.Active, uncomment the next line:
// using static Core.Domain.Enums.Common.Enums;

public class UserBuilder
{
    private readonly User _u = new()
    {
        Id = Guid.NewGuid(),      // Guid instead of int
        UserName = "john",
        EmailId = "john@acme.com" // correct property name
        // If your User inherits BaseEntity with Status enum and you want it set:
        // IsActive = Status.Active
    };

    public UserBuilder WithId(Guid id) { _u.Id = id; return this; }
    public UserBuilder WithUserName(string name) { _u.UserName = name; return this; }
    public UserBuilder WithEmail(string email) { _u.EmailId = email; return this; } // EmailId
    public UserBuilder Locked(bool locked = true) { _u.IsLocked = (byte)(locked ? 1 : 0); return this; }

    public User Build() => _u;
}

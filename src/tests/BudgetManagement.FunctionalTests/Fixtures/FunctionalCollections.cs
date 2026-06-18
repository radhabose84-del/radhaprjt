namespace BudgetManagement.FunctionalTests.Fixtures;

// One collection per workflow story; each shares one QAServerFixture across its ordered steps.

[CollectionDefinition("US-BUD-01-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-BUD-02-BudgetLifecycle")]
public sealed class BudgetLifecycleCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-BUD-03-AuditTrail")]
public sealed class AuditTrailCollection : ICollectionFixture<QAServerFixture> { }

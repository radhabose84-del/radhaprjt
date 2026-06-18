namespace BackgroundService.FunctionalTests.Fixtures;

// One collection per workflow story; each shares one QAServerFixture across its ordered steps.

[CollectionDefinition("US-BGS-01-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-BGS-02-NotificationSetup")]
public sealed class NotificationSetupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-BGS-03-ApprovalWorkflowReadiness")]
public sealed class ApprovalWorkflowReadinessCollection : ICollectionFixture<QAServerFixture> { }

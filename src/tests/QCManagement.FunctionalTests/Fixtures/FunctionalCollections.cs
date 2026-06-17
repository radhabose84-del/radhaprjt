namespace QCManagement.FunctionalTests.Fixtures;

// One collection per workflow story; each shares one QAServerFixture across its ordered steps.

[CollectionDefinition("US-QC-01-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-QC-02-QualityDefinition")]
public sealed class QualityDefinitionCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-QC-03-AuditTrail")]
public sealed class AuditTrailCollection : ICollectionFixture<QAServerFixture> { }

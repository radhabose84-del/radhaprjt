namespace QCManagement.QATests.Fixtures;

// One collection per entity. All bind the single QAServerFixture; cross-collection
// parallelization is disabled (AssemblyInfo.cs) — every collection shares the one `testsales` session.

[CollectionDefinition("QcMiscMasterCollection")]
public sealed class QcMiscMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("QcMiscTypeMasterCollection")]
public sealed class QcMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("QualityParameterCollection")]
public sealed class QualityParameterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("QualityTemplateCollection")]
public sealed class QualityTemplateCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("QualitySpecificationCollection")]
public sealed class QualitySpecificationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("QcInspectionCollection")]
public sealed class QcInspectionCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("QcAuditLogCollection")]
public sealed class QcAuditLogCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }

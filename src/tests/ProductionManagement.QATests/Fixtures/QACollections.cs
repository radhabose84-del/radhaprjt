namespace ProductionManagement.QATests.Fixtures;

// One collection per entity. All bind the single QAServerFixture; cross-collection
// parallelization is disabled (AssemblyInfo.cs) — every collection shares the one `testsales` session.

[CollectionDefinition("ProdMiscMasterCollection")]
public sealed class ProdMiscMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ProdMiscTypeMasterCollection")]
public sealed class ProdMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CertificationMasterCollection")]
public sealed class CertificationMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CountGroupCollection")]
public sealed class CountGroupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CountMasterCollection")]
public sealed class CountMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("PackTypeCollection")]
public sealed class PackTypeCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ProcessMasterCollection")]
public sealed class ProcessMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("QualityMasterCollection")]
public sealed class QualityMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("RawMaterialTypeCollection")]
public sealed class RawMaterialTypeCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("YarnTwistMasterCollection")]
public sealed class YarnTwistMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("YarnTypeCollection")]
public sealed class YarnTypeCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("LotMasterCollection")]
public sealed class LotMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ProductionPackEntryCollection")]
public sealed class ProductionPackEntryCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("RepackingHeaderCollection")]
public sealed class RepackingHeaderCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ProdAuditLogCollection")]
public sealed class ProdAuditLogCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }

namespace InventoryManagement.FunctionalTests.Fixtures;

// One collection per workflow story; each shares one QAServerFixture across its ordered steps.

[CollectionDefinition("US-INV-01-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-INV-02-ItemCatalogue")]
public sealed class ItemCatalogueCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-INV-03-MaterialFlowAudit")]
public sealed class MaterialFlowAuditCollection : ICollectionFixture<QAServerFixture> { }

namespace PurchaseManagement.FunctionalTests.Fixtures;

// One collection per workflow story; each shares one QAServerFixture across its ordered steps.

[CollectionDefinition("US-PUR-01-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-PUR-02-VendorTermsChain")]
public sealed class VendorTermsChainCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-PUR-03-ReturnPolicyChain")]
public sealed class ReturnPolicyChainCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-PUR-04-ProcureToReceiptReadiness")]
public sealed class ProcureToReceiptReadinessCollection : ICollectionFixture<QAServerFixture> { }

namespace Store.FunctionalTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class FunctionalTestCollection : ICollectionFixture<FunctionalTestFixture>
{
    public const string Name = "Functional tests";
}

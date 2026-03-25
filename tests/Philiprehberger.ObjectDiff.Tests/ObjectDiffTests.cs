using Xunit;
namespace Philiprehberger.ObjectDiff.Tests;

public class ObjectDiffTests
{
    private class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string? Email { get; set; }
    }

    private class PersonWithAddress
    {
        public string Name { get; set; } = "";
        public Address Address { get; set; } = new();
    }

    private class Address
    {
        public string City { get; set; } = "";
        public string Zip { get; set; } = "";
    }

    [Fact]
    public void Compare_IdenticalObjects_ReturnsNoChanges()
    {
        var obj = new Person { Name = "Alice", Age = 30 };

        var result = ObjectDiff.Compare(obj, obj);

        Assert.False(result.HasChanges);
        Assert.Empty(result.Changes);
    }

    [Fact]
    public void Compare_DifferentStringProperty_DetectsChange()
    {
        var oldObj = new Person { Name = "Alice", Age = 30 };
        var newObj = new Person { Name = "Bob", Age = 30 };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.True(result.HasChanges);
        Assert.Single(result.Changes);
        Assert.Equal("Name", result.Changes[0].PropertyName);
        Assert.Equal("Alice", result.Changes[0].OldValue);
        Assert.Equal("Bob", result.Changes[0].NewValue);
    }

    [Fact]
    public void Compare_MultipleChanges_DetectsAll()
    {
        var oldObj = new Person { Name = "Alice", Age = 30, Email = "a@test.com" };
        var newObj = new Person { Name = "Bob", Age = 25, Email = "b@test.com" };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.Equal(3, result.Changes.Count);
    }

    [Fact]
    public void Compare_NullToValue_DetectsChange()
    {
        var oldObj = new Person { Name = "Alice", Email = null };
        var newObj = new Person { Name = "Alice", Email = "a@test.com" };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.True(result.HasChanges);
        var emailChange = result.Changes.Single(c => c.PropertyName == "Email");
        Assert.Null(emailChange.OldValue);
        Assert.Equal("a@test.com", emailChange.NewValue);
    }

    [Fact]
    public void Compare_BothNull_ReturnsNoChanges()
    {
        var result = ObjectDiff.Compare<Person>(null, null);

        Assert.False(result.HasChanges);
    }

    [Fact]
    public void Compare_WithIgnoreProperties_ExcludesSpecifiedProperty()
    {
        var oldObj = new Person { Name = "Alice", Age = 30 };
        var newObj = new Person { Name = "Bob", Age = 25 };
        var options = new DiffOptions();
        options.IgnoreProperties.Add("Age");

        var result = ObjectDiff.Compare(oldObj, newObj, options);

        Assert.Single(result.Changes);
        Assert.Equal("Name", result.Changes[0].PropertyName);
    }
}

public class ObjectDiffDeepCompareTests
{
    private class PersonWithAddress
    {
        public string Name { get; set; } = "";
        public Address Address { get; set; } = new();
    }

    private class Address
    {
        public string City { get; set; } = "";
        public string Zip { get; set; } = "";
    }

    [Fact]
    public void Compare_DeepCompare_DetectsNestedChanges()
    {
        var oldObj = new PersonWithAddress { Name = "Alice", Address = new Address { City = "NYC", Zip = "10001" } };
        var newObj = new PersonWithAddress { Name = "Alice", Address = new Address { City = "LA", Zip = "90001" } };
        var options = new DiffOptions { DeepCompare = true };

        var result = ObjectDiff.Compare(oldObj, newObj, options);

        Assert.True(result.HasChanges);
        Assert.Contains(result.Changes, c => c.PropertyName == "Address.City");
        Assert.Contains(result.Changes, c => c.PropertyName == "Address.Zip");
    }

    [Fact]
    public void Compare_ShallowCompare_TreatsNestedAsWholeObject()
    {
        var oldObj = new PersonWithAddress { Name = "Alice", Address = new Address { City = "NYC", Zip = "10001" } };
        var newObj = new PersonWithAddress { Name = "Alice", Address = new Address { City = "LA", Zip = "90001" } };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.True(result.HasChanges);
        Assert.Single(result.Changes);
        Assert.Equal("Address", result.Changes[0].PropertyName);
    }

    [Fact]
    public void Compare_DeepCompare_RespectsMaxDepth()
    {
        var options = new DiffOptions { DeepCompare = true, MaxDepth = 0 };
        var oldObj = new PersonWithAddress { Name = "Alice", Address = new Address { City = "NYC" } };
        var newObj = new PersonWithAddress { Name = "Alice", Address = new Address { City = "LA" } };

        var result = ObjectDiff.Compare(oldObj, newObj, options);

        Assert.Single(result.Changes);
        Assert.Equal("Address", result.Changes[0].PropertyName);
    }
}

public class ObjectDiffToJsonTests
{
    private class Simple
    {
        public string Value { get; set; } = "";
    }

    [Fact]
    public void ToJson_WithChanges_ReturnsValidJson()
    {
        var oldObj = new Simple { Value = "old" };
        var newObj = new Simple { Value = "new" };
        var result = ObjectDiff.Compare(oldObj, newObj);

        var json = ObjectDiff.ToJson(result);

        Assert.Contains("\"property\"", json);
        Assert.Contains("\"old\"", json);
        Assert.Contains("\"new\"", json);
    }

    [Fact]
    public void ToJson_NoChanges_ReturnsEmptyChangesArray()
    {
        var obj = new Simple { Value = "same" };
        var result = ObjectDiff.Compare(obj, obj);

        var json = ObjectDiff.ToJson(result);

        Assert.Contains("\"changes\": []", json);
    }

    [Fact]
    public void ToJson_ReturnsDeserializableJson()
    {
        var oldObj = new Simple { Value = "a" };
        var newObj = new Simple { Value = "b" };
        var result = ObjectDiff.Compare(oldObj, newObj);

        var json = ObjectDiff.ToJson(result);

        var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.NotNull(doc);
    }
}

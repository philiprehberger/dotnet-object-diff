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

public class CollectionDiffTests
{
    private class Order
    {
        public string Id { get; set; } = "";
        public List<string> Tags { get; set; } = new();
    }

    private class Container
    {
        public int[] Numbers { get; set; } = Array.Empty<int>();
    }

    private class ObjectWithList
    {
        public List<int> Values { get; set; } = new();
    }

    [Fact]
    public void Compare_CollectionElementChanged_DetectsModification()
    {
        var oldObj = new Order { Id = "1", Tags = new List<string> { "urgent", "review" } };
        var newObj = new Order { Id = "1", Tags = new List<string> { "urgent", "approved" } };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.True(result.HasChanges);
        Assert.Single(result.Changes);
        Assert.Equal("Tags[1]", result.Changes[0].PropertyName);
        Assert.Equal("review", result.Changes[0].OldValue);
        Assert.Equal("approved", result.Changes[0].NewValue);
    }

    [Fact]
    public void Compare_CollectionElementAdded_DetectsAddition()
    {
        var oldObj = new Order { Id = "1", Tags = new List<string> { "urgent" } };
        var newObj = new Order { Id = "1", Tags = new List<string> { "urgent", "new-tag" } };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.True(result.HasChanges);
        Assert.Single(result.Changes);
        Assert.Equal("Tags[1]", result.Changes[0].PropertyName);
        Assert.Null(result.Changes[0].OldValue);
        Assert.Equal("new-tag", result.Changes[0].NewValue);
    }

    [Fact]
    public void Compare_CollectionElementRemoved_DetectsRemoval()
    {
        var oldObj = new Order { Id = "1", Tags = new List<string> { "urgent", "review" } };
        var newObj = new Order { Id = "1", Tags = new List<string> { "urgent" } };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.True(result.HasChanges);
        Assert.Single(result.Changes);
        Assert.Equal("Tags[1]", result.Changes[0].PropertyName);
        Assert.Equal("review", result.Changes[0].OldValue);
        Assert.Null(result.Changes[0].NewValue);
    }

    [Fact]
    public void Compare_IdenticalCollections_ReturnsNoChanges()
    {
        var oldObj = new Order { Id = "1", Tags = new List<string> { "a", "b" } };
        var newObj = new Order { Id = "1", Tags = new List<string> { "a", "b" } };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.False(result.HasChanges);
    }

    [Fact]
    public void Compare_ArrayProperty_DetectsChanges()
    {
        var oldObj = new Container { Numbers = new[] { 1, 2, 3 } };
        var newObj = new Container { Numbers = new[] { 1, 5, 3 } };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.True(result.HasChanges);
        Assert.Single(result.Changes);
        Assert.Equal("Numbers[1]", result.Changes[0].PropertyName);
    }

    [Fact]
    public void Compare_NullCollectionToPopulated_DetectsAdditions()
    {
        var oldObj = new ObjectWithList { Values = null! };
        var newObj = new ObjectWithList { Values = new List<int> { 1, 2 } };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.Equal(2, result.Changes.Count);
        Assert.Equal("Values[0]", result.Changes[0].PropertyName);
        Assert.Equal("Values[1]", result.Changes[1].PropertyName);
    }
}

public class DictionaryDiffTests
{
    private class Config
    {
        public Dictionary<string, string> Settings { get; set; } = new();
    }

    private class IntKeyConfig
    {
        public Dictionary<int, string> Lookup { get; set; } = new();
    }

    [Fact]
    public void Compare_DictionaryValueChanged_DetectsModification()
    {
        var oldObj = new Config { Settings = new Dictionary<string, string> { ["theme"] = "dark", ["lang"] = "en" } };
        var newObj = new Config { Settings = new Dictionary<string, string> { ["theme"] = "light", ["lang"] = "en" } };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.True(result.HasChanges);
        Assert.Single(result.Changes);
        Assert.Equal("Settings[theme]", result.Changes[0].PropertyName);
        Assert.Equal("dark", result.Changes[0].OldValue);
        Assert.Equal("light", result.Changes[0].NewValue);
    }

    [Fact]
    public void Compare_DictionaryKeyAdded_DetectsAddition()
    {
        var oldObj = new Config { Settings = new Dictionary<string, string> { ["theme"] = "dark" } };
        var newObj = new Config { Settings = new Dictionary<string, string> { ["theme"] = "dark", ["lang"] = "en" } };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.True(result.HasChanges);
        Assert.Single(result.Changes);
        Assert.Equal("Settings[lang]", result.Changes[0].PropertyName);
        Assert.Null(result.Changes[0].OldValue);
        Assert.Equal("en", result.Changes[0].NewValue);
    }

    [Fact]
    public void Compare_DictionaryKeyRemoved_DetectsRemoval()
    {
        var oldObj = new Config { Settings = new Dictionary<string, string> { ["theme"] = "dark", ["lang"] = "en" } };
        var newObj = new Config { Settings = new Dictionary<string, string> { ["theme"] = "dark" } };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.True(result.HasChanges);
        Assert.Single(result.Changes);
        Assert.Equal("Settings[lang]", result.Changes[0].PropertyName);
        Assert.Equal("en", result.Changes[0].OldValue);
        Assert.Null(result.Changes[0].NewValue);
    }

    [Fact]
    public void Compare_IdenticalDictionaries_ReturnsNoChanges()
    {
        var oldObj = new Config { Settings = new Dictionary<string, string> { ["a"] = "1" } };
        var newObj = new Config { Settings = new Dictionary<string, string> { ["a"] = "1" } };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.False(result.HasChanges);
    }

    [Fact]
    public void Compare_NullDictionaryToPopulated_DetectsAdditions()
    {
        var oldObj = new Config { Settings = null! };
        var newObj = new Config { Settings = new Dictionary<string, string> { ["key"] = "val" } };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.True(result.HasChanges);
        Assert.Single(result.Changes);
        Assert.Equal("Settings[key]", result.Changes[0].PropertyName);
    }
}

public class DiffIgnoreAttributeTests
{
    private class AuditEntry
    {
        public string Action { get; set; } = "";

        [DiffIgnore]
        public DateTime Timestamp { get; set; }

        [DiffIgnore]
        public string InternalId { get; set; } = "";
    }

    private class SimpleEntity
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }

    [Fact]
    public void Compare_DiffIgnoreAttribute_ExcludesMarkedProperties()
    {
        var oldObj = new AuditEntry { Action = "create", Timestamp = new DateTime(2025, 1, 1), InternalId = "abc" };
        var newObj = new AuditEntry { Action = "update", Timestamp = new DateTime(2025, 6, 1), InternalId = "xyz" };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.Single(result.Changes);
        Assert.Equal("Action", result.Changes[0].PropertyName);
    }

    [Fact]
    public void Compare_DiffIgnoreAttribute_StillDetectsNonIgnoredChanges()
    {
        var oldObj = new AuditEntry { Action = "create", Timestamp = DateTime.Now };
        var newObj = new AuditEntry { Action = "create", Timestamp = DateTime.Now.AddHours(1) };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.False(result.HasChanges);
    }

    [Fact]
    public void Compare_NoDiffIgnoreAttribute_DetectsAllChanges()
    {
        var oldObj = new SimpleEntity { Name = "a", Value = 1 };
        var newObj = new SimpleEntity { Name = "b", Value = 2 };

        var result = ObjectDiff.Compare(oldObj, newObj);

        Assert.Equal(2, result.Changes.Count);
    }

    [Fact]
    public void Compare_DiffIgnoreWithStringIgnore_BothApply()
    {
        var oldObj = new AuditEntry { Action = "create", Timestamp = DateTime.Now, InternalId = "abc" };
        var newObj = new AuditEntry { Action = "update", Timestamp = DateTime.Now.AddHours(1), InternalId = "xyz" };
        var options = new DiffOptions();
        options.IgnoreProperties.Add("Action");

        var result = ObjectDiff.Compare(oldObj, newObj, options);

        Assert.False(result.HasChanges);
    }
}

public class GetSummaryTests
{
    private class User
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string? Email { get; set; }
    }

    [Fact]
    public void GetSummary_SingleChange_ReturnsHumanReadableDescription()
    {
        var oldObj = new User { Name = "Alice", Age = 30 };
        var newObj = new User { Name = "Bob", Age = 30 };

        var result = ObjectDiff.Compare(oldObj, newObj);
        var summary = result.GetSummary();

        Assert.Single(summary);
        Assert.Equal("Name changed from 'Alice' to 'Bob'", summary[0]);
    }

    [Fact]
    public void GetSummary_MultipleChanges_ReturnsOneEntryPerChange()
    {
        var oldObj = new User { Name = "Alice", Age = 30, Email = "old@test.com" };
        var newObj = new User { Name = "Bob", Age = 25, Email = "new@test.com" };

        var result = ObjectDiff.Compare(oldObj, newObj);
        var summary = result.GetSummary();

        Assert.Equal(3, summary.Count);
        Assert.Contains(summary, s => s.Contains("Name"));
        Assert.Contains(summary, s => s.Contains("Age"));
        Assert.Contains(summary, s => s.Contains("Email"));
    }

    [Fact]
    public void GetSummary_NullValues_DisplaysNullLiteral()
    {
        var oldObj = new User { Name = "Alice", Email = null };
        var newObj = new User { Name = "Alice", Email = "a@test.com" };

        var result = ObjectDiff.Compare(oldObj, newObj);
        var summary = result.GetSummary();

        Assert.Single(summary);
        Assert.Equal("Email changed from null to 'a@test.com'", summary[0]);
    }

    [Fact]
    public void GetSummary_NoChanges_ReturnsEmptyList()
    {
        var obj = new User { Name = "Alice", Age = 30 };

        var result = ObjectDiff.Compare(obj, obj);
        var summary = result.GetSummary();

        Assert.Empty(summary);
    }
}

# Philiprehberger.ObjectDiff

[![CI](https://github.com/philiprehberger/dotnet-object-diff/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-object-diff/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.ObjectDiff.svg)](https://www.nuget.org/packages/Philiprehberger.ObjectDiff)
[![GitHub release](https://img.shields.io/github/v/release/philiprehberger/dotnet-object-diff)](https://github.com/philiprehberger/dotnet-object-diff/releases)
[![Last updated](https://img.shields.io/github/last-commit/philiprehberger/dotnet-object-diff)](https://github.com/philiprehberger/dotnet-object-diff/commits/main)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-object-diff)](LICENSE)
[![Bug Reports](https://img.shields.io/github/issues/philiprehberger/dotnet-object-diff/bug)](https://github.com/philiprehberger/dotnet-object-diff/issues?q=is%3Aissue+is%3Aopen+label%3Abug)
[![Feature Requests](https://img.shields.io/github/issues/philiprehberger/dotnet-object-diff/enhancement)](https://github.com/philiprehberger/dotnet-object-diff/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)
[![Sponsor](https://img.shields.io/badge/sponsor-GitHub%20Sponsors-ec6cb9)](https://github.com/sponsors/philiprehberger)

Compare two objects and return a list of property-level changes.

## Installation

```bash
dotnet add package Philiprehberger.ObjectDiff
```

## Usage

```csharp
using Philiprehberger.ObjectDiff;

var old = new User { Name = "Alice", Email = "alice@old.com" };
var updated = new User { Name = "Alice", Email = "alice@new.com" };

var result = ObjectDiff.Compare(old, updated);

foreach (var change in result.Changes)
    Console.WriteLine($"{change.PropertyName}: {change.OldValue} -> {change.NewValue}");
// Email: alice@old.com -> alice@new.com
```

### Ignore properties

```csharp
var options = new DiffOptions();
options.IgnoreProperties.Add("UpdatedAt");

var result = ObjectDiff.Compare(old, updated, options);
```

### DiffIgnore attribute

```csharp
using Philiprehberger.ObjectDiff;

public class AuditEntry
{
    public string Action { get; set; } = "";

    [DiffIgnore]
    public DateTime Timestamp { get; set; }
}

var result = ObjectDiff.Compare(oldEntry, newEntry);
// Timestamp is excluded from comparison
```

### Deep compare (nested objects)

```csharp
var options = new DiffOptions { DeepCompare = true, MaxDepth = 3 };

var result = ObjectDiff.Compare(oldOrder, newOrder, options);
// Changes include nested paths like "Address.City"
```

### Collection diffing

```csharp
using Philiprehberger.ObjectDiff;

var old = new Order { Tags = new List<string> { "urgent", "review" } };
var updated = new Order { Tags = new List<string> { "urgent", "approved" } };

var result = ObjectDiff.Compare(old, updated);
// Tags[1] changed from "review" to "approved"
```

### Dictionary diffing

```csharp
using Philiprehberger.ObjectDiff;

var old = new Config { Settings = new Dictionary<string, string> { ["theme"] = "dark" } };
var updated = new Config { Settings = new Dictionary<string, string> { ["theme"] = "light", ["lang"] = "en" } };

var result = ObjectDiff.Compare(old, updated);
// Settings[theme] changed from "dark" to "light"
// Settings[lang] added with value "en"
```

### Human-readable summaries

```csharp
using Philiprehberger.ObjectDiff;

var result = ObjectDiff.Compare(old, updated);
IReadOnlyList<string> summary = result.GetSummary();

foreach (var line in summary)
    Console.WriteLine(line);
// Name changed from 'Alice' to 'Bob'
// Email changed from 'alice@old.com' to 'alice@new.com'
```

### Serialize to JSON (audit logging)

```csharp
var result = ObjectDiff.Compare(old, updated);
string json = ObjectDiff.ToJson(result);
// {
//   "changes": [
//     {
//       "property": "Email",
//       "old": "alice@old.com",
//       "new": "alice@new.com"
//     }
//   ]
// }
```

## API

| Member | Description |
|---|---|
| `ObjectDiff.Compare<T>(T?, T?, DiffOptions?)` | Compare two objects and return property-level changes |
| `ObjectDiff.ToJson(DiffResult)` | Serialize a diff result to JSON |
| `DiffResult.Changes` | List of `PropertyChange` records |
| `DiffResult.HasChanges` | `true` if any properties differ |
| `DiffResult.GetSummary()` | Returns `IReadOnlyList<string>` of human-readable change descriptions |
| `PropertyChange.PropertyName` | Name of the changed property (dot-notation for nested, bracket-notation for collections/dictionaries) |
| `PropertyChange.OldValue` | Previous value |
| `PropertyChange.NewValue` | Updated value |
| `DiffOptions.IgnoreProperties` | Set of property names to skip |
| `DiffOptions.DeepCompare` | Enable recursive nested comparison (default: `false`) |
| `DiffOptions.MaxDepth` | Max recursion depth for deep compare (default: `3`) |
| `[DiffIgnore]` | Attribute to exclude a property from comparison |

## Development

```bash
dotnet build src/Philiprehberger.ObjectDiff.csproj --configuration Release
```

## Support

If you find this package useful, consider giving it a star on GitHub — it helps motivate continued maintenance and development.

[![LinkedIn](https://img.shields.io/badge/Philip%20Rehberger-LinkedIn-0A66C2?logo=linkedin)](https://www.linkedin.com/in/philiprehberger)
[![More packages](https://img.shields.io/badge/more-open%20source%20packages-blue)](https://philiprehberger.com/open-source-packages)

## License

[MIT](LICENSE)

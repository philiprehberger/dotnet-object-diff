# Philiprehberger.ObjectDiff

[![CI](https://github.com/philiprehberger/dotnet-object-diff/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-object-diff/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.ObjectDiff.svg)](https://www.nuget.org/packages/Philiprehberger.ObjectDiff)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-object-diff)](LICENSE)

Compare two objects and return a list of property-level changes. Zero external dependencies.

## Install

```bash
dotnet add package Philiprehberger.ObjectDiff
```

## Usage

### Basic comparison

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

### Deep compare (nested objects)

```csharp
var options = new DiffOptions { DeepCompare = true, MaxDepth = 3 };

var result = ObjectDiff.Compare(oldOrder, newOrder, options);
// Changes include nested paths like "Address.City"
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
| `PropertyChange.PropertyName` | Name of the changed property (dot-notation for nested) |
| `PropertyChange.OldValue` | Previous value |
| `PropertyChange.NewValue` | Updated value |
| `DiffOptions.IgnoreProperties` | Set of property names to skip |
| `DiffOptions.DeepCompare` | Enable recursive nested comparison (default: `false`) |
| `DiffOptions.MaxDepth` | Max recursion depth for deep compare (default: `3`) |

## Development

```bash
dotnet build src/Philiprehberger.ObjectDiff.csproj --configuration Release
```

## License

[MIT](LICENSE)

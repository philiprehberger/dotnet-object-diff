# Changelog

## 0.2.0 (2026-03-28)

- Add collection/list diffing with index-level change detection for `IEnumerable` properties
- Add `[DiffIgnore]` attribute for excluding properties from comparison
- Add dictionary diffing with key-level change detection for `IDictionary` properties
- Add `GetSummary()` method on `DiffResult` for human-readable change descriptions
- Add missing GitHub issue templates, dependabot config, and PR template
- Add missing README badges (GitHub release, Last updated, Bug Reports, Feature Requests)
- Add Support section to README

## 0.1.8 (2026-03-26)

- Add Sponsor badge to README
- Fix License section format
- Sync README description with .csproj

## 0.1.7 (2026-03-24)

- Add unit tests
- Add test step to CI workflow

## 0.1.6 (2026-03-23)

- Sync .csproj description with README

## 0.1.5 (2026-03-22)

- Add dates to changelog entries

## 0.1.4 (2026-03-16)

- Add Development section to README
- Add GenerateDocumentationFile and RepositoryType to .csproj

## 0.1.1 (2026-03-13)

- Include README in NuGet package

## 0.1.0 (2026-03-13)

### Added
- `ObjectDiff.Compare<T>()` for property-level object comparison
- `DiffResult` with `HasChanges` and list of `PropertyChange` records
- `DiffOptions` for ignoring properties and enabling deep comparison
- `ToJson()` for serializing diff results
- Support for nested object comparison with configurable max depth

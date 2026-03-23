# Changelog

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

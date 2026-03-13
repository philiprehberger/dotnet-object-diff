# Changelog

## 0.1.0 (2026-03-13)

### Added
- `ObjectDiff.Compare<T>()` for property-level object comparison
- `DiffResult` with `HasChanges` and list of `PropertyChange` records
- `DiffOptions` for ignoring properties and enabling deep comparison
- `ToJson()` for serializing diff results
- Support for nested object comparison with configurable max depth

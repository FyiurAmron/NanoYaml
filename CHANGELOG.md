# Changelog

## [0.1.1] - 2026-10-31

### Added

- `[Serialize]` opts non-public fields and properties into serialization.
- Deserialization into generic collections compatible with `ICollection<>`
  and `IDictionary<,>`.

### Fixed

- `ISet<>` targets now deserialize into `HashSet<>` instead of throwing.
- Flow-style decisions now count entries of generic maps too.

## [0.1.0] - 2026-10-21

### Added

- Initial release.

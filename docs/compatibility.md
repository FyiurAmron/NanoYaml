# YAML compatibility: 1.2.2 vs 1.1 vs YamlDotNet vs StrictYAML vs NanoYaml

This document compares three YAML processors across the key features of the
YAML 1.2.2 specification, with additional rows for YAML 1.1 behaviors that
remain relevant in practice. The specification texts are included as
[spec-YAML-1.1.md](spec-YAML-1.1.md) and
[spec-YAML-1.2.2.md](spec-YAML-1.2.2.md) for reference.

Columns based on YamlDotNet 18.1.0 and StrictYAML 1.7.3 on Python 3.14.4.
The NanoYaml columns describe profile `nanoyaml:0.1.0:` in its two parsing
modes.

`NYE` is NanoYamlException.

## 1.2 compliance and good practices

`✓` spec-conformant

`✗` differs (stricter or looser)

`~` if conformant only conditionally

### Structure

| Feature | YamlDotNet | StrictYAML | NanoYaml<br>Compatible | NanoYaml<br>Strict |
|---|---|---|---|---|
| Typing model | ✗ 1.1-flavored implicit | ✗ strings until cast | ✗ string until typed | ✗ string until typed |
| Multiple documents per stream | ✓ | ✗ | ✗ NYE | ✗ NYE |
| `---` document start | ✓ | ✓ | ~ ignored | ✗ NYE <a href="#note-1">*1</a> |
| `...` document end | ✓ | ✓ | ~ ignored | ✗ NYE |
| `%YAML` changes parsing | ✗ | ✗ | ✗ | ✗ NYE |
| `%TAG` support | ✓ | ✗ | ~ ignored | ✗ NYE <a href="#note-1">*1</a> |
| Unknown directives | ~ no warning | ~ maps/seqs only | ~ ignored | ✗ NYE |
| Empty doc as null | ✓ | ✗ reads `""` | ✓ | ✗ NYE |
| Empty block value (`k:`) | ✓ null | ~ <a href="#note-2">*2</a> | ✓ null | ✗ NYE |
| Scalar document root | ✓ | ✓ | ✓ | ✓ |
| Flow document root | ✓ | ~ `dirty_load` | ✓ | ✓ |
| BOM stripping | ✗ | ~ stream start, collections only | ✓ | ✓ |
| Line breaks normalize to LF | ✓ | ✓ | ✓ | ✓ |
| Fail on tab indent | ✓ | ✓ | ✓ NYE | ✓ NYE |
| Unescaped tabs in content | ✓ | ✓ | ✓ | ✗ NYE, use `\t` |
| Comment stripping | ✓ | ✗ | ✓ | ✓ |
| Tunable nesting cap | ✓ | ✗ | ✓ | ✓ |

### Collections

| Feature | YamlDotNet | StrictYAML | NanoYaml<br>Compatible | NanoYaml<br>Strict |
|---|---|---|---|---|
| Indented block sequences | ✓ | ✓ | ✓ | ✓ |
| Flat block sequences | ✓ | ✓ | ✓ | ✗ NYE |
| Block mappings | ✓ | ✓ | ✓ | ✓ |
| Flow sequences | ✓ | ~ `dirty_load` | ✓ | ✓ |
| Singleton pair in flow seq | ✓ | ~ `dirty_load` | ✓ | ✓ |
| Nested flow sequences | ✓ | ~ `dirty_load` | ✓ | ✓ |
| Flow mappings | ✓ | ~ `dirty_load` | ✓ | ✓ |
| Trailing comma in flow | ✓ | ~ `dirty_load` | ✓ | ✓ |
| Explicit `?` complex keys | ✓ | ~ scalar only | ✗ NYE | ✗ NYE |
| Quoted keys, URLs with `:` | ✓ | ✓ | ✓ | ✓ |
| No duplicate keys | ✗ opt-in only | ✓ | ✓ NYE | ✓ NYE |

### Scalars and quoting

| Feature | YamlDotNet | StrictYAML | NanoYaml<br>Compatible | NanoYaml<br>Strict |
|---|---|---|---|---|
| Multi-line plain scalars | ✓ | ✓ | ✓ | ✓ |
| Single-quoted, `''` escape | ✓ | ✓ | ✓ | ✓ |
| Double-quoted, `\"` `\\` etc. | ✓ | ✓ | ✓ | ✓ |
| `\x` `\u` `\U` escapes | ✓ | ✓ | ✓ | ✓ |
| Unknown escapes rejected | ✓ | ✓ | ✓ NYE | ✓ NYE |
| Literal block `\|` | ✓ | ✓ | ✓ | ✓ |
| Folded block `>` | ✓ | ✓ | ✓ | ✓ |
| Chomping `-` `+` clip | ✓ | ✓ | ✓ | ✓ |
| Explicit indent indicators | ✓ | ✓ | ✓ | ✓ |
| `""` empty string | ✓ | ✓ | ✓ | ✓ |
| Quoted `"null"` | ✓ | ✓ | ✓ | ✓ |

### Numbers

| Feature | YamlDotNet | StrictYAML | NanoYaml<br>Compatible | NanoYaml<br>Strict |
|---|---|---|---|---|
| Int | ✓ | ~ <a href="#note-2">*2</a> | ✓ | ✓ |
| Hex `0x` | ✓ | ~ <a href="#note-2">*2</a> | ✓ | ✓ |
| Octal `0o17` | ✗ | ✗ | ✓ | ✗ NYE |
| Float `1E.5` `5.2E+5` | ✓ | ~ <a href="#note-2">*2</a> | ✓ | ✓ |
| lowercase `.inf` `.nan` | ✓ | ~ <a href="#note-2">*2</a> | ✓ | ✓ |
| `.Inf` `.INF` `.NaN` `.NAN` | ✓ | ~ <a href="#note-2">*2</a> | ✓ | ✗ NYE |

### Other types

| Feature | YamlDotNet | StrictYAML | NanoYaml<br>Compatible | NanoYaml<br>Strict |
|---|---|---|---|---|
| `true` `false` | ✓ | ~ <a href="#note-2">*2</a> | ✓ | ✓ |
| `True` `TRUE` `False` `FALSE` | ✓ | ~ <a href="#note-2">*2</a> | ✓ | ✗ NYE |
| Untyped bool | ~ opt-in | ✗ <a href="#note-2">*2</a> | ✗ stays string | ✗ stays string |
| Untyped null | ✓ | ✗ <a href="#note-2">*2</a> | ✓ | ✓ |
| `null` | ✓ | ~ <a href="#note-2">*2</a> | ✓ | ✓ |
|  `~` |  ✓ | ✗ | ✓ | ✓ |
| `Null` `NULL` | ✓ | ~ <a href="#note-2">*2</a> | ✓ | ✗ NYE |
| Timestamps string by default | ✓ | ✓ | ✓ | ✓ |
| Timestamp converters | ✓ | ✓ | ✓ | ✓ |

### Tags, anchors, schema extras

| Feature | YamlDotNet | StrictYAML | NanoYaml<br>Compatible | NanoYaml<br>Strict |
|---|---|---|---|---|
| Anchors and aliases | ✓ | ✗ | ✗ NYE | ✗ NYE |
| Explicit tags `!!str` | ✓ | ✗ | ✗ NYE | ✗ NYE |
| Verbatim tags `!<uri>` | ✓ | ✗ | ✗ NYE | ✗ NYE |
| Unknown tags | ~ mapping dependent | ✗ | ✗ NYE | ✗ NYE |
| `*` `&` `!` unquoted | ✓ syntax, sharp edges | ✗ | ✗ NYE | ✗ NYE |

## YAML 1.1 features removed in 1.2

`✓` by default does things like 1.1 even in 1.2 mode (bad)

`✗` by default does things like 1.2 should (good)

`~` does 1.1 handling conditionally

| Feature | YamlDotNet | StrictYAML | NanoYaml<br>Compatible | NanoYaml<br>Strict |
|---|---|---|---|---|
| `<<` merge | ~ opt-in only | ~ nested only | ✗ | ✗ |
| `017` | ✓ 15 | ✗ <a href="#note-2">*2</a> | ✓ 17 | ✗ NYE |
| `0b` binary | ✓ | ✗ | ✗ NYE | ✗ NYE |
| `1_7` | ✓ | ~ <a href="#note-2">*2</a> | ✗ NYE | ✗ NYE |
| `1:20` sexagesimal | ✓ | ✗ | ✗ NYE | ✗ NYE |
| `yes`/`no`/`on` spellings | ✓ | ~ <a href="#note-2">*2</a> | ✗ | ✗ |
| `!!timestamp` | ✓ | ✗ | ✗ NYE | ✗ NYE |
| `!!binary` | ✗ | ✗ | ✗ NYE | ✗ NYE |
| `!!set` `!!omap` `!!pairs` | ✗ | ✗ | ✗ NYE | ✗ NYE |

## Common pitfalls

`✓` problem exists by default (bad)

`✗` no problem by default (good)

`~` problem happens conditionally

`(1.1)` behavior from 1.1 which shouldn't happen in 1.2.2

| Pitfall | YamlDotNet | StrictYAML | NanoYaml<br>Compatible | NanoYaml<br>Strict |
|---|---|---|---|---|
| (1.1) Norway: `NO` -> false | ✓ | ✗ <a href="#note-2">*2</a> | ✗ NYE | ✗ NYE |
| (1.1) `yes` `on` -> true | ✓ | ✗ <a href="#note-2">*2</a> | ✗ | ✗ |
| (1.1) `<<` merges | ✗ | ~ nested only | ✗ | ✗ |
| (1.1) Zip codes `01234` melt | ✓ 668 if int | ✗ | ✓ 1234 if int | ✗ NYE |
| Billion laughs expansion | ✗ | ✗ | ✗ NYE | ✗ NYE |
| Empty node reads as `""` | ✗ | ✓ <a href="#note-2">*2</a> | ✓ | ✗ NYE |
| Empty string dumped as `k:` | ✗  | ✓ | ✗ | ✗ |
| Null-like strings turn to nulls | ✗ | ✓ <a href="#note-2">*2</a> | ~ `Null` `NULL` only | ✗ NYE |
| Null into non-nullable int | ✓ masks as 0 | ✗ | ✗ NYE | ✗ NYE |
| Null-like strings dumped bare | ✓ | ✓ | ✗ | ✗ |
| Number-like strings dumped bare | ✓ <a href="#note-3">*3</a> | ✓ <a href="#note-3">*3</a> | ✗ | ✗ |
| Bool-like strings dumped bare | ✓ <a href="#note-3">*3</a> | ✓ <a href="#note-3">*3</a> | ✗ | ✗ |
| `{a:b}` -> `{"a:b":null}` | ✓ | ~ only with `dirty_load` | ✗ NYE | ✗ NYE |
| YAML key absent in schema loads | ~ opt-in | ✗ | ✗ NYE | ✗ NYE |
| Schema key absent in YAML errors | ✗ | ✓ | ✗ | ✗ |

## Notes

<span id="note-1">*1</span> `%TAG` and `---` are only accepted at the start of
the file as the NanoYaml profile marker. The compatible mode ignores them
anywhere else instead of reading a document stream.

<span id="note-2">*2</span> StrictYAML types by explicit schema casts only
(`Int()`, `Bool()`, `Float()`, `EmptyNone()`, `NullNone()` etc.), untyped
scalars stay string

<span id="note-3">*3</span> A bare number-like string reads back as a string
in a schema-less reader and as a number in a reader with a schema. NanoYaml
quotes those strings to preserve the value against round-trip errors.

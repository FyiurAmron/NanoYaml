*Dedicated to my children, today's and future; let The Sun always shine in
your eyes.*

# NanoYaml

[![NuGet version](https://img.shields.io/nuget/v/NanoYaml.svg)](https://www.nuget.org/packages/NanoYaml)
[![Build status](https://github.com/FyiurAmron/NanoYaml/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/FyiurAmron/NanoYaml/actions/workflows/ci.yml)
[GitHub repository](https://github.com/FyiurAmron/NanoYaml)

A [YAML-1.2.2](https://yaml.org/spec/1.2.2/)-like configuration format (or
"YAML profile" if you like) and a matching C# reference serializer/deserializer,
conformant with an opinionated strict subset of YAML 1.2.2 spec. It's round-trip
safe and simple. You can read the [strict spec](https://github.com/FyiurAmron/NanoYaml/blob/main/docs/spec-NanoYaml-0.1.0-Strict.md)
and the [compatible spec](https://github.com/FyiurAmron/NanoYaml/blob/main/docs/spec-NanoYaml-0.1.0-Compatible.md)
if you want, or you can just use the heuristic: write like you would write
a JSON-in-YAML, i.e. just write data with simple, clean formatting like
YAML, but forget about all the YAML spec shenanigans forever.

It's created for those who need a fast, light and strict format for data storage,
both read-only human-readable local (configs, static data) and automated remote
(transmission, serialization, persistence). It addresses virtually 100% of
YAML's criticism, while retaining full compatibility upstream
(i.e. all NanoYaml files are fully valid YAML 1.1 and 1.2.2 files as well).

Inspired by [StrictYAML](https://hitchdev.com/strictyaml/)
and [YamlRecords](https://www.nuget.org/packages/YamlRecords) projects.
See [`compatibility.md`](https://github.com/FyiurAmron/NanoYaml/blob/main/docs/compatibility.md)
in the repository for the full feature comparison with YamlDotNet and StrictYAML.

`NanoYaml.cs` can be used as a single-file drop-in for projects targeting
.NET 8+ (the source itself uses C# 13), and it's fast:
(or at least as fast as a YAML parser in C# can easily get ) -
[about 5x faster than YamlDotNet, and two orders of magnitude faster than
StrictYAML](https://github.com/FyiurAmron/NanoYaml/blob/main/docs/benchmark.md).

PS NanoYaml is not an acronym; "yaml" is the sound kids do sometimes.

## Install

```sh
dotnet add package NanoYaml
```

## Usage

You can use records, classes and structs, both with properties or bare fields,
and you can also serialize or deserialize data directly if needed:

```csharp
using System.Collections.Generic;
using Vaxquis.NanoYaml;

record ConnectionRecord(string Host, int Port);

class ConnectionClass {
    public string Host { get; set; }
    public int Port { get; set; }
}

struct ConnectionStruct {
    public string Host;
    public int Port;
}

// then, in function
string yaml;

var connectionRaw = new Dictionary<string, object> { ["Host"] = "localhost", ["Port"] = 8080 };
var connectionRecord = new ConnectionRecord("localhost", 8080);
var connectionClass = new ConnectionClass { Host = "localhost", Port = 8080 };
var connectionStruct = new ConnectionStruct { Host = "localhost", Port = 8080 };

yaml = NanoYaml.Serialize(connectionRaw);
var unyamlDict = NanoYaml.Deserialize<Dictionary<string, object>>(yaml);

yaml = NanoYaml.Serialize(connectionRecord);
var unyamlRecord = NanoYaml.Deserialize<ConnectionRecord>(yaml);

yaml = NanoYaml.Serialize(connectionClass);
var unyamlClass = NanoYaml.Deserialize<ConnectionClass>(yaml);

yaml = NanoYaml.Serialize(connectionStruct);
var unyamlStruct = NanoYaml.Deserialize<ConnectionStruct>(yaml);
```

All of them produce (fully-conformant files start with the NanoYaml shibboleth):

```yaml
%TAG ! nanoyaml:0.1.0:
---
Host: localhost
Port: 8080
```

Note that, because it's a strict subset, *not all YAML documents can be
deserialized directly*. If your current data is non-strict and NanoYaml rejects
it, just load your data with another YAML parser and then serialize
that data with NanoYaml. Simple.

## Profiles

NanoYaml has two profiles, and the deserializer picks one per document based on
the preamble: a file that starts with the `%TAG ! nanoyaml:0.1.0:` shibboleth is
treated as conformant with the Strict profile and read in the Strict mode,
and one without it is read in the Compatible mode. You can override the
autodetection via `DeserializerParams.ParsingMode` constructor param.

The serializer always emits Strict profile files.

## Development

The style rules live in the repository, under `_tools`, and one command runs
them all: the `.editorconfig` rules through `dotnet format`, the rest
through a Roslyn lint, and text hygiene through the same code the
pre-commit hook runs.

```sh
bash _tools/gate.sh
```

Install the pre-commit hook once per clone with
`git config core.hooksPath .githooks`.

### FAQ

1.
Q: but why bother? We already have >insert format you consider better than YAML
here<.

A: why bother? because I can. And no, no other format does what this project
does. If such a format would exist, I would gladly use it myself. I think the
core of YAML's syntax is great, just the spec is FUBAR.

2.
Q: Haven't you seen the xkcd about the standards?

A: Yes. That's true about competing standards. Thankfully, this is *not*
a standard, but a subset of a standard, fully compliant with it itself.
(that's obviously a lie, no parser nor serializer for YAML is fully compliant
with its spec :) Also, it's non-competing - the output is valid YAML with no
real caveats. Call it an anti-standard, if you wish.

3.
Q: but YAML is unfixable FUBAR, you said it yourself!

A: I beg to differ. This project is the sole proof needed to prove that YAML
is indeed scavengable. It's the spec that's broken, not the core of the format.

4.
Q: but what's wrong with YAML? YAML is OK, you just don't know how to use it!

A: to know what's wrong with YAML, just do a query for "I hate YAML"
or "YAML is broken". Tl;dr the goals, as provided by the authors, are not only
completely off from what YAML is in reality - but they simply contradict
themselves. Writing a 100% spec-conformant serializer/deserializer is near
impossible by itself, due to all the quirk the spec delineates. Also, it's
a proprietary format that's barely (i.e. not at all nowadays) maintained.

5.
Q: but that's not YAML standard's fault, that's the parsers fault!

A: again, I beg do differ. There's only one person that can remember, understand
and implement YAML 1.2.2 as written, and he's the one who gave us the stone
tablets and black monoliths in the past.

6.
Q: but why not use StrictYAML?

A: Python only.

7.
Q: but why not just port StrictYAML then?

A: because it would be completely non-trivial and in essence be equal to writing
the lib from scratch. Also, StrictYAML has lots of own problems.
It uses `ruamel.yaml`, and is just a wrapper around a non-owned code.
As a result, it still fails in many cases where it would theoretically work.
Also, I think it's too lax in many ways
(see [`compatibility.md`](https://github.com/FyiurAmron/NanoYaml/blob/main/docs/compatibility.md)).

8.
Q: OK, so C#, but why not use YamlDotNet?

A: because it is, while a great effort, a futile and outdated one. Has broken
spec support, doesn't support new C# features (e.g. records, at least as of 2026
it didn't), and has glacial maintenance.
Also, it's huge, and I'd hate to fork it. Not well suited for drop-in game
engine use as well.

9.
Q: OK, so why not YamlRecords?

A: it's a nice lib for trivial cases, but it's broken in tons of ways. Not
production-ready, if you know what I mean.

10.
Q: can I use it for...

A: yes. It's MIT-licensed, I don't care.

11.
Q: but my issue isn't listed here!

A: that's not a question. But you can just open an issue, and I'll likely add
it here, if it makes sense.

<!-- Downloaded from https://yaml.org/spec/1.2.2/ by _tools/fetch-yaml-spec.sh -->

# YAML Ain't Markup Language (YAML™) version 1.2 {#yaml-aint-markup-language-yaml-version-12}

## Revision 1.2.2 (2021-10-01) {#revision-122-2021-10-01 .subtitle}

Copyright presently by YAML Language Development Team^[1](#fn:team){.footnote rel="footnote"}^\
Copyright 2001-2009 by Oren Ben-Kiki, Clark Evans, Ingy döt Net

This document may be freely copied, provided it is not modified.

**Status of this Document**

This is the **YAML specification v1.2.2**. It defines the **YAML 1.2 data language**. There are no normative changes
from the **YAML specification v1.2**. The primary objectives of this revision are to correct errors and add clarity.

This revision also strives to make the YAML language development process more open, more transparent and easier for
people to contribute to. The input format is now Markdown instead of DocBook, and the images are made from plain text
LaTeX files rather than proprietary drawing software. All the source content for the specification is publicly
hosted^[2](#fn:spec-repo){.footnote rel="footnote"}^.

The previous YAML specification^[3](#fn:1-2-spec){.footnote rel="footnote"}^ was published 12 years ago. In that time
span, YAML's popularity has grown significantly. Efforts are ongoing to improve the language and grow it to meet the
needs and expectations of its users. While this revision of the specification makes no actual changes to YAML, it begins
a process by which the language intends to evolve and stay modern.

The YAML specification is often seen as overly complicated for something which appears to be so simple. Even though YAML
often is used for software configuration, it has always been and will continue to be a complete data serialization
language. Future YAML plans are focused on making the language and ecosystem more powerful and reliable while
simultaneously simplifying the development process for implementers.

While this revision of the specification is limiting itself to informational changes only, there is companion
documentation intended to guide YAML framework implementers and YAML language users. This documentation can continue to
evolve and expand continually between published revisions of this specification.

See:

- [YAML Resources Index](ext/resources)
- [YAML Vocabulary Glossary](ext/glossary)
- [YAML Specification Changes](ext/changes)
- [YAML Specification Errata](ext/errata)

**Abstract**

YAML™ (rhymes with "camel") is a human-friendly, cross language, Unicode based data serialization language designed
around the common native data types of dynamic programming languages. It is broadly useful for programming needs ranging
from configuration files to internet messaging to object persistence to data auditing and visualization. Together with
the Unicode standard for characters^[4](#fn:unicode){.footnote rel="footnote"}^, this specification provides all the
information necessary to understand YAML version 1.2 and to create programs that process YAML information.

**Contents**

- [YAML Ain't Markup Language (YAML™) version
  1.2](#yaml-aint-markup-language-yaml-version-12){#markdown-toc-yaml-aint-markup-language-yaml-version-12}
  - [Revision 1.2.2 (2021-10-01)](#revision-122-2021-10-01){#markdown-toc-revision-122-2021-10-01}
- [Chapter 1. Introduction to YAML](#chapter-1-introduction-to-yaml){#markdown-toc-chapter-1-introduction-to-yaml}
  - [1.1. Goals](#11-goals){#markdown-toc-11-goals}
  - [1.2. YAML History](#12-yaml-history){#markdown-toc-12-yaml-history}
  - [1.3. Terminology](#13-terminology){#markdown-toc-13-terminology}
- [Chapter 2. Language Overview](#chapter-2-language-overview){#markdown-toc-chapter-2-language-overview}
  - [2.1. Collections](#21-collections){#markdown-toc-21-collections}
  - [2.2. Structures](#22-structures){#markdown-toc-22-structures}
  - [2.3. Scalars](#23-scalars){#markdown-toc-23-scalars}
  - [2.4. Tags](#24-tags){#markdown-toc-24-tags}
  - [2.5. Full Length Example](#25-full-length-example){#markdown-toc-25-full-length-example}
- [Chapter 3. Processes and Models](#chapter-3-processes-and-models){#markdown-toc-chapter-3-processes-and-models}
  - [3.1. Processes](#31-processes){#markdown-toc-31-processes}
    - [3.1.1. Dump](#311-dump){#markdown-toc-311-dump}
    - [3.1.2. Load](#312-load){#markdown-toc-312-load}
  - [3.2. Information Models](#32-information-models){#markdown-toc-32-information-models}
    - [3.2.1. Representation Graph](#321-representation-graph){#markdown-toc-321-representation-graph}
      - [3.2.1.1. Nodes](#3211-nodes){#markdown-toc-3211-nodes}
      - [3.2.1.2. Tags](#3212-tags){#markdown-toc-3212-tags}
      - [3.2.1.3. Node Comparison](#3213-node-comparison){#markdown-toc-3213-node-comparison}
    - [3.2.2. Serialization Tree](#322-serialization-tree){#markdown-toc-322-serialization-tree}
      - [3.2.2.1. Mapping Key Order](#3221-mapping-key-order){#markdown-toc-3221-mapping-key-order}
      - [3.2.2.2. Anchors and Aliases](#3222-anchors-and-aliases){#markdown-toc-3222-anchors-and-aliases}
    - [3.2.3. Presentation Stream](#323-presentation-stream){#markdown-toc-323-presentation-stream}
      - [3.2.3.1. Node Styles](#3231-node-styles){#markdown-toc-3231-node-styles}
      - [3.2.3.2. Scalar Formats](#3232-scalar-formats){#markdown-toc-3232-scalar-formats}
      - [3.2.3.3. Comments](#3233-comments){#markdown-toc-3233-comments}
      - [3.2.3.4. Directives](#3234-directives){#markdown-toc-3234-directives}
  - [3.3. Loading Failure Points](#33-loading-failure-points){#markdown-toc-33-loading-failure-points}
    - [3.3.1. Well-Formed Streams and Identified
      Aliases](#331-well-formed-streams-and-identified-aliases){#markdown-toc-331-well-formed-streams-and-identified-ali
ases}
    - [3.3.2. Resolved Tags](#332-resolved-tags){#markdown-toc-332-resolved-tags}
    - [3.3.3. Recognized and Valid Tags](#333-recognized-and-valid-tags){#markdown-toc-333-recognized-and-valid-tags}
    - [3.3.4. Available Tags](#334-available-tags){#markdown-toc-334-available-tags}
- [Chapter 4. Syntax Conventions](#chapter-4-syntax-conventions){#markdown-toc-chapter-4-syntax-conventions}
  - [4.1. Production Syntax](#41-production-syntax){#markdown-toc-41-production-syntax}
  - [4.2. Production Parameters](#42-production-parameters){#markdown-toc-42-production-parameters}
  - [4.3. Production Naming
    Conventions](#43-production-naming-conventions){#markdown-toc-43-production-naming-conventions}
- [Chapter 5. Character Productions](#chapter-5-character-productions){#markdown-toc-chapter-5-character-productions}
  - [5.1. Character Set](#51-character-set){#markdown-toc-51-character-set}
  - [5.2. Character Encodings](#52-character-encodings){#markdown-toc-52-character-encodings}
  - [5.3. Indicator Characters](#53-indicator-characters){#markdown-toc-53-indicator-characters}
  - [5.4. Line Break Characters](#54-line-break-characters){#markdown-toc-54-line-break-characters}
  - [5.5. White Space Characters](#55-white-space-characters){#markdown-toc-55-white-space-characters}
  - [5.6. Miscellaneous Characters](#56-miscellaneous-characters){#markdown-toc-56-miscellaneous-characters}
  - [5.7. Escaped Characters](#57-escaped-characters){#markdown-toc-57-escaped-characters}
- [Chapter 6. Structural Productions](#chapter-6-structural-productions){#markdown-toc-chapter-6-structural-productions}
  - [6.1. Indentation Spaces](#61-indentation-spaces){#markdown-toc-61-indentation-spaces}
  - [6.2. Separation Spaces](#62-separation-spaces){#markdown-toc-62-separation-spaces}
  - [6.3. Line Prefixes](#63-line-prefixes){#markdown-toc-63-line-prefixes}
  - [6.4. Empty Lines](#64-empty-lines){#markdown-toc-64-empty-lines}
  - [6.5. Line Folding](#65-line-folding){#markdown-toc-65-line-folding}
  - [6.6. Comments](#66-comments){#markdown-toc-66-comments}
  - [6.7. Separation Lines](#67-separation-lines){#markdown-toc-67-separation-lines}
  - [6.8. Directives](#68-directives){#markdown-toc-68-directives}
    - [6.8.1. "`YAML`{.language-plaintext .highlighter-rouge}"
      Directives](#681-yaml-directives){#markdown-toc-681-yaml-directives}
    - [6.8.2. "`TAG`{.language-plaintext .highlighter-rouge}"
      Directives](#682-tag-directives){#markdown-toc-682-tag-directives}
      - [6.8.2.1. Tag Handles](#6821-tag-handles){#markdown-toc-6821-tag-handles}
      - [6.8.2.2. Tag Prefixes](#6822-tag-prefixes){#markdown-toc-6822-tag-prefixes}
  - [6.9. Node Properties](#69-node-properties){#markdown-toc-69-node-properties}
    - [6.9.1. Node Tags](#691-node-tags){#markdown-toc-691-node-tags}
    - [6.9.2. Node Anchors](#692-node-anchors){#markdown-toc-692-node-anchors}
- [Chapter 7. Flow Style Productions](#chapter-7-flow-style-productions){#markdown-toc-chapter-7-flow-style-productions}
  - [7.1. Alias Nodes](#71-alias-nodes){#markdown-toc-71-alias-nodes}
  - [7.2. Empty Nodes](#72-empty-nodes){#markdown-toc-72-empty-nodes}
  - [7.3. Flow Scalar Styles](#73-flow-scalar-styles){#markdown-toc-73-flow-scalar-styles}
    - [7.3.1. Double-Quoted Style](#731-double-quoted-style){#markdown-toc-731-double-quoted-style}
    - [7.3.2. Single-Quoted Style](#732-single-quoted-style){#markdown-toc-732-single-quoted-style}
    - [7.3.3. Plain Style](#733-plain-style){#markdown-toc-733-plain-style}
  - [7.4. Flow Collection Styles](#74-flow-collection-styles){#markdown-toc-74-flow-collection-styles}
    - [7.4.1. Flow Sequences](#741-flow-sequences){#markdown-toc-741-flow-sequences}
    - [7.4.2. Flow Mappings](#742-flow-mappings){#markdown-toc-742-flow-mappings}
  - [7.5. Flow Nodes](#75-flow-nodes){#markdown-toc-75-flow-nodes}
- [Chapter 8. Block Style
  Productions](#chapter-8-block-style-productions){#markdown-toc-chapter-8-block-style-productions}
  - [8.1. Block Scalar Styles](#81-block-scalar-styles){#markdown-toc-81-block-scalar-styles}
    - [8.1.1. Block Scalar Headers](#811-block-scalar-headers){#markdown-toc-811-block-scalar-headers}
      - [8.1.1.1. Block Indentation
        Indicator](#8111-block-indentation-indicator){#markdown-toc-8111-block-indentation-indicator}
      - [8.1.1.2. Block Chomping Indicator](#8112-block-chomping-indicator){#markdown-toc-8112-block-chomping-indicator}
    - [8.1.2. Literal Style](#812-literal-style){#markdown-toc-812-literal-style}
    - [8.1.3. Folded Style](#813-folded-style){#markdown-toc-813-folded-style}
  - [8.2. Block Collection Styles](#82-block-collection-styles){#markdown-toc-82-block-collection-styles}
    - [8.2.1. Block Sequences](#821-block-sequences){#markdown-toc-821-block-sequences}
    - [8.2.2. Block Mappings](#822-block-mappings){#markdown-toc-822-block-mappings}
    - [8.2.3. Block Nodes](#823-block-nodes){#markdown-toc-823-block-nodes}
- [Chapter 9. Document Stream
  Productions](#chapter-9-document-stream-productions){#markdown-toc-chapter-9-document-stream-productions}
  - [9.1. Documents](#91-documents){#markdown-toc-91-documents}
    - [9.1.1. Document Prefix](#911-document-prefix){#markdown-toc-911-document-prefix}
    - [9.1.2. Document Markers](#912-document-markers){#markdown-toc-912-document-markers}
    - [9.1.3. Bare Documents](#913-bare-documents){#markdown-toc-913-bare-documents}
    - [9.1.4. Explicit Documents](#914-explicit-documents){#markdown-toc-914-explicit-documents}
    - [9.1.5. Directives Documents](#915-directives-documents){#markdown-toc-915-directives-documents}
  - [9.2. Streams](#92-streams){#markdown-toc-92-streams}
- [Chapter 10. Recommended Schemas](#chapter-10-recommended-schemas){#markdown-toc-chapter-10-recommended-schemas}
  - [10.1. Failsafe Schema](#101-failsafe-schema){#markdown-toc-101-failsafe-schema}
    - [10.1.1. Tags](#1011-tags){#markdown-toc-1011-tags}
      - [10.1.1.1. Generic Mapping](#10111-generic-mapping){#markdown-toc-10111-generic-mapping}
      - [10.1.1.2. Generic Sequence](#10112-generic-sequence){#markdown-toc-10112-generic-sequence}
      - [10.1.1.3. Generic String](#10113-generic-string){#markdown-toc-10113-generic-string}
    - [10.1.2. Tag Resolution](#1012-tag-resolution){#markdown-toc-1012-tag-resolution}
  - [10.2. JSON Schema](#102-json-schema){#markdown-toc-102-json-schema}
    - [10.2.1. Tags](#1021-tags){#markdown-toc-1021-tags}
      - [10.2.1.1. Null](#10211-null){#markdown-toc-10211-null}
      - [10.2.1.2. Boolean](#10212-boolean){#markdown-toc-10212-boolean}
      - [10.2.1.3. Integer](#10213-integer){#markdown-toc-10213-integer}
      - [10.2.1.4. Floating Point](#10214-floating-point){#markdown-toc-10214-floating-point}
    - [10.2.2. Tag Resolution](#1022-tag-resolution){#markdown-toc-1022-tag-resolution}
  - [10.3. Core Schema](#103-core-schema){#markdown-toc-103-core-schema}
    - [10.3.1. Tags](#1031-tags){#markdown-toc-1031-tags}
    - [10.3.2. Tag Resolution](#1032-tag-resolution){#markdown-toc-1032-tag-resolution}
  - [10.4. Other Schemas](#104-other-schemas){#markdown-toc-104-other-schemas}
- [Reference Links](#reference-links){#markdown-toc-reference-links}


# Chapter 1. Introduction to YAML {#chapter-1-introduction-to-yaml}

YAML (a recursive acronym for "YAML Ain't Markup Language") is a data serialization language designed to be
human-friendly and work well with modern programming languages for common everyday tasks. This specification is both an
introduction to the YAML language and the concepts supporting it. It is also a complete specification of the information
needed to develop [applications](#processes-and-models) for processing YAML.

Open, interoperable and readily understandable tools have advanced computing immensely. YAML was designed from the start
to be useful and friendly to people working with data. It uses Unicode [printable](#character-set) characters,
[some](#indicator-characters) of which provide structural information and the rest containing the data itself. YAML
achieves a unique cleanness by minimizing the amount of structural characters and allowing the data to show itself in a
natural and meaningful way. For example, [indentation](#indentation-spaces) may be used for structure,
[colons](#flow-mappings) separate [key/value pairs](#mapping) and [dashes](#block-sequences) are used to create
"bulleted" [lists](#sequence).

There are many kinds of [data structures](#dump), but they can all be adequately [represented](#representation-graph)
with three basic primitives: [mappings](#mapping) (hashes/dictionaries), [sequences](#sequence) (arrays/lists) and
[scalars](#scalars) (strings/numbers). YAML leverages these primitives and adds a simple typing system and
[aliasing](#anchors-and-aliases) mechanism to form a complete language for
[serializing](#serializing-the-representation-graph) any [native data structure](#representing-native-data-structures).
While most programming languages can use YAML for data serialization, YAML excels in working with those languages that
are fundamentally built around the three basic primitives. These include common dynamic languages such as JavaScript,
Perl, PHP, Python and Ruby.

There are hundreds of different languages for programming, but only a handful of languages for storing and transferring
data. Even though its potential is virtually boundless, YAML was specifically created to work well for common use cases
such as: configuration files, log files, interprocess messaging, cross-language data sharing, object persistence and
debugging of complex data structures. When data is easy to view and understand, programming becomes a simpler task.


## 1.1. Goals {#11-goals}

The design goals for YAML are, in decreasing priority:

1.  YAML should be easily readable by humans.
2.  YAML data should be portable between programming languages.
3.  YAML should match the [native data structures](#representing-native-data-structures) of dynamic languages.
4.  YAML should have a consistent model to support generic tools.
5.  YAML should support one-pass processing.
6.  YAML should be expressive and extensible.
7.  YAML should be easy to implement and use.


## 1.2. YAML History {#12-yaml-history}

The YAML 1.0 specification was published in early 2004 by by Clark Evans, Oren Ben-Kiki, and Ingy döt Net after 3 years
of collaborative design work through the yaml-core mailing list^[5](#fn:yaml-core){.footnote rel="footnote"}^. The
project was initially rooted in Clark and Oren's work on the SML-DEV^[6](#fn:sml-dev){.footnote rel="footnote"}^ mailing
list (for simplifying XML) and Ingy's plain text serialization module^[7](#fn:denter){.footnote rel="footnote"}^ for
Perl. The language took a lot of inspiration from many other technologies and formats that preceded it.

The first YAML framework was written in Perl in 2001 and Ruby was the first language to ship a YAML framework as part of
its core language distribution in 2003.

The YAML 1.1^[8](#fn:1-1-spec){.footnote rel="footnote"}^ specification was published in 2005. Around this time, the
developers became aware of JSON^[9](#fn:json){.footnote rel="footnote"}^. By sheer coincidence, JSON was almost a
complete subset of YAML (both syntactically and semantically).

In 2006, Kyrylo Simonov produced PyYAML^[10](#fn:pyyaml){.footnote rel="footnote"}^ and
LibYAML^[11](#fn:libyaml){.footnote rel="footnote"}^. A lot of the YAML frameworks in various programming languages are
built over LibYAML and many others have looked to PyYAML as a solid reference for their implementations.

The YAML 1.2^[3](#fn:1-2-spec){.footnote rel="footnote"}^ specification was published in 2009. Its primary focus was
making YAML a strict superset of JSON. It also removed many of the problematic implicit typing recommendations.

Since the release of the 1.2 specification, YAML adoption has continued to grow, and many large-scale projects use it as
their primary interface language. In 2020, the new [YAML language design team](ext/team) began meeting regularly to
discuss improvements to the YAML language and specification; to better meet the needs and expectations of its users and
use cases.

This YAML 1.2.2 specification, published in October 2021, is the first step in YAML's rejuvenated development journey.
YAML is now more popular than it has ever been, but there is a long list of things that need to be addressed for it to
reach its full potential. The YAML design team is focused on making YAML as good as possible.


## 1.3. Terminology {#13-terminology}

The key words "MUST", "MUST NOT", "REQUIRED", "SHALL", "SHALL NOT", "SHOULD", "SHOULD NOT", "RECOMMENDED", "MAY", and
"OPTIONAL" in this document are to be interpreted as described in RFC 2119^[12](#fn:rfc-2119){.footnote
rel="footnote"}^.

The rest of this document is arranged as follows. Chapter [2](#language-overview) provides a short preview of the main
YAML features. Chapter [3](#processes-and-models) describes the YAML information model and the processes for converting
from and to this model and the YAML text format. The bulk of the document, chapters [4](#syntax-conventions),
[5](#character-productions), [6](#structural-productions), [7](#flow-style-productions), [8](#block-style-productions)
and [9](#document-stream-productions), formally define this text format. Finally, chapter [10](#recommended-schemas)
recommends basic YAML schemas.


# Chapter 2. Language Overview {#chapter-2-language-overview}

This section provides a quick glimpse into the expressive power of YAML. It is not expected that the first-time reader
grok all of the examples. Rather, these selections are used as motivation for the remainder of the specification.


## 2.1. Collections {#21-collections}

YAML's [block collections](#block-collection-styles) use [indentation](#indentation-spaces) for scope and begin each
entry on its own line. [Block sequences](#block-sequences) indicate each entry with a dash and space
("`- `{.language-plaintext .highlighter-rouge}"). [Mappings](#mapping) use a colon and space ("`: `{.language-plaintext
.highlighter-rouge}") to mark each [key/value pair](#mapping). [Comments](#comments) begin with an octothorpe (also
called a "hash", "sharp", "pound" or "number sign" - "`#`{.language-plaintext .highlighter-rouge}").

**Example 2.1 Sequence of Scalars (ball players)**

``` example
- Mark McGwire
- Sammy Sosa
- Ken Griffey
```

**Example 2.2 Mapping Scalars to Scalars (player statistics)**

``` example
hr:  65    # Home runs
avg: 0.278 # Batting average
rbi: 147   # Runs Batted In
```

**Example 2.3 Mapping Scalars to Sequences (ball clubs in each league)**

``` example
american:
- Boston Red Sox
- Detroit Tigers
- New York Yankees
national:
- New York Mets
- Chicago Cubs
- Atlanta Braves
```

**Example 2.4 Sequence of Mappings (players' statistics)**

``` example
-
  name: Mark McGwire
  hr:   65
  avg:  0.278
-
  name: Sammy Sosa
  hr:   63
  avg:  0.288
```

YAML also has [flow styles](#flow-style-productions), using explicit [indicators](#indicator-characters) rather than
[indentation](#indentation-spaces) to denote scope. The [flow sequence](#flow-sequences) is written as a
[comma](#flow-collection-styles) separated list within [square](#flow-sequences) [brackets](#flow-sequences). In a
similar manner, the [flow mapping](#flow-mappings) uses [curly](#flow-mappings) [braces](#flow-mappings).

**Example 2.5 Sequence of Sequences**

``` example
- [name        , hr, avg  ]
- [Mark McGwire, 65, 0.278]
- [Sammy Sosa  , 63, 0.288]
```

**Example 2.6 Mapping of Mappings**

``` example
Mark McGwire: {hr: 65, avg: 0.278}
Sammy Sosa: {
    hr: 63,
    avg: 0.288,
 }
```


## 2.2. Structures {#22-structures}

YAML uses three dashes ("`---`{.language-plaintext .highlighter-rouge}") to separate [directives](#directives) from
[document](#documents) [content](#nodes). This also serves to signal the start of a document if no
[directives](#directives) are present. Three dots ( "`...`{.language-plaintext .highlighter-rouge}") indicate the end of
a document without starting a new one, for use in communication channels.

**Example 2.7 Two Documents in a Stream (each with a leading comment)**

``` example
# Ranking of 1998 home runs
---
- Mark McGwire
- Sammy Sosa
- Ken Griffey

# Team ranking
---
- Chicago Cubs
- St Louis Cardinals
```

**Example 2.8 Play by Play Feed from a Game**

``` example
---
time: 20:03:20
player: Sammy Sosa
action: strike (miss)
...
---
time: 20:03:47
player: Sammy Sosa
action: grand slam
...
```

Repeated [nodes](#nodes) (objects) are first [identified](#anchors-and-aliases) by an [anchor](#anchors-and-aliases)
(marked with the ampersand - "`&`{.language-plaintext .highlighter-rouge}") and are then [aliased](#anchors-and-aliases)
(referenced with an asterisk - "`*`{.language-plaintext .highlighter-rouge}") thereafter.

**Example 2.9 Single Document with Two Comments**

``` example
---
hr: # 1998 hr ranking
- Mark McGwire
- Sammy Sosa
# 1998 rbi ranking
rbi:
- Sammy Sosa
- Ken Griffey
```

**Example 2.10 Node for "`Sammy Sosa`{.language-plaintext .highlighter-rouge}" appears twice in this document**

``` example
---
hr:
- Mark McGwire
# Following node labeled SS
- &SS Sammy Sosa
rbi:
- *SS # Subsequent occurrence
- Ken Griffey
```

A question mark and space ("`? `{.language-plaintext .highlighter-rouge}") indicate a complex [mapping](#mapping)
[key](#nodes). Within a [block collection](#block-collection-styles), [key/value pairs](#mapping) can start immediately
following the [dash](#block-sequences), [colon](#flow-mappings) or [question mark](#flow-mappings).

**Example 2.11 Mapping between Sequences**

``` example
? - Detroit Tigers
  - Chicago cubs
: - 2001-07-23

? [ New York Yankees,
    Atlanta Braves ]
: [ 2001-07-02, 2001-08-12,
    2001-08-14 ]
```

**Example 2.12 Compact Nested Mapping**

``` example
---
# Products purchased
- item    : Super Hoop
  quantity: 1
- item    : Basketball
  quantity: 4
- item    : Big Shoes
  quantity: 1
```


## 2.3. Scalars {#23-scalars}

[Scalar content](#scalar) can be written in [block](#scalars) notation, using a [literal style](#literal-style)
(indicated by "`|`{.language-plaintext .highlighter-rouge}") where all [line breaks](#line-break-characters) are
significant. Alternatively, they can be written with the [folded style](#folded-style) (denoted by
"`>`{.language-plaintext .highlighter-rouge}") where each [line break](#line-break-characters) is
[folded](#line-folding) to a [space](#white-space-characters) unless it ends an [empty](#empty-lines) or a
[more-indented](#example-more-indented-lines) line.

**Example 2.13 In literals, newlines are preserved**

``` example
# ASCII Art
--- |
  \//||\/||
  // ||  ||__
```

**Example 2.14 In the folded scalars, newlines become spaces**

``` example
--- >
  Mark McGwire's
  year was crippled
  by a knee injury.
```

**Example 2.15 Folded newlines are preserved for "more indented" and blank lines**

``` example
--- >
 Sammy Sosa completed another
 fine season with great stats.

   63 Home Runs
   0.288 Batting Average

 What a year!
```

**Example 2.16 Indentation determines scope**

``` example
name: Mark McGwire
accomplishment: >
  Mark set a major league
  home run record in 1998.
stats: |
  65 Home Runs
  0.278 Batting Average
```

YAML's [flow scalars](#flow-scalar-styles) include the [plain style](#plain-style) (most examples thus far) and two
quoted styles. The [double-quoted style](#double-quoted-style) provides [escape sequences](#escaped-characters). The
[single-quoted style](#single-quoted-style) is useful when [escaping](#escaped-characters) is not needed. All [flow
scalars](#flow-scalar-styles) can span multiple lines; [line breaks](#line-break-characters) are always
[folded](#line-folding).

**Example 2.17 Quoted Scalars**

``` example
unicode: "Sosa did fine.\u263A"
control: "\b1998\t1999\t2000\n"
hex esc: "\x0d\x0a is \r\n"

single: '"Howdy!" he cried.'
quoted: ' # Not a ''comment''.'
tie-fighter: '|\-*-/|'
```

**Example 2.18 Multi-line Flow Scalars**

``` example
plain:
  This unquoted scalar
  spans many lines.

quoted: "So does this
  quoted scalar.\n"
```


## 2.4. Tags {#24-tags}

In YAML, [untagged nodes](#resolved-tags) are given a type depending on the [application](#processes-and-models). The
examples in this specification generally use the `seq`{.language-plaintext .highlighter-rouge},
`map`{.language-plaintext .highlighter-rouge} and `str`{.language-plaintext .highlighter-rouge} types from the [fail
safe schema](#failsafe-schema). A few examples also use the `int`{.language-plaintext .highlighter-rouge},
`float`{.language-plaintext .highlighter-rouge} and `null`{.language-plaintext .highlighter-rouge} types from the [JSON
schema](#json-schema).

**Example 2.19 Integers**

``` example
canonical: 12345
decimal: +12345
octal: 0o14
hexadecimal: 0xC
```

**Example 2.20 Floating Point**

``` example
canonical: 1.23015e+3
exponential: 12.3015e+02
fixed: 1230.15
negative infinity: -.inf
not a number: .nan
```

**Example 2.21 Miscellaneous**

``` example
null:
booleans: [ true, false ]
string: '012345'
```

**Example 2.22 Timestamps**

``` example
canonical: 2001-12-15T02:59:43.1Z
iso8601: 2001-12-14t21:59:43.10-05:00
spaced: 2001-12-14 21:59:43.10 -5
date: 2002-12-14
```

Explicit typing is denoted with a [tag](#tags) using the exclamation point ("`!`{.language-plaintext
.highlighter-rouge}") symbol. [Global tags](#tags) are URIs and may be specified in a [tag shorthand](#tag-shorthands)
notation using a [handle](#tag-handles). [Application](#processes-and-models)-specific [local tags](#tags) may also be
used.

**Example 2.23 Various Explicit Tags**

``` example
---
not-date: !!str 2002-04-28

picture: !!binary |
 R0lGODlhDAAMAIQAAP//9/X
 17unp5WZmZgAAAOfn515eXv
 Pz7Y6OjuDg4J+fn5OTk6enp
 56enmleECcgggoBADs=

application specific tag: !something |
 The semantics of the tag
 above may be different for
 different documents.
```

**Example 2.24 Global Tags**

``` example
%TAG ! tag:clarkevans.com,2002:
--- !shape
  # Use the ! handle for presenting
  # tag:clarkevans.com,2002:circle
- !circle
  center: &ORIGIN {x: 73, y: 129}
  radius: 7
- !line
  start: *ORIGIN
  finish: { x: 89, y: 102 }
- !label
  start: *ORIGIN
  color: 0xFFEEBB
  text: Pretty vector drawing.
```

**Example 2.25 Unordered Sets**

``` example
# Sets are represented as a
# Mapping where each key is
# associated with a null value
--- !!set
? Mark McGwire
? Sammy Sosa
? Ken Griffey
```

**Example 2.26 Ordered Mappings**

``` example
# Ordered maps are represented as
# A sequence of mappings, with
# each mapping having one key
--- !!omap
- Mark McGwire: 65
- Sammy Sosa: 63
- Ken Griffey: 58
```


## 2.5. Full Length Example {#25-full-length-example}

Below are two full-length examples of YAML. The first is a sample invoice; the second is a sample log file.

**Example 2.27 Invoice**

``` example
--- !<tag:clarkevans.com,2002:invoice>
invoice: 34843
date   : 2001-01-23
bill-to: &id001
  given  : Chris
  family : Dumars
  address:
    lines: |
      458 Walkman Dr.
      Suite #292
    city    : Royal Oak
    state   : MI
    postal  : 48046
ship-to: *id001
product:
- sku         : BL394D
  quantity    : 4
  description : Basketball
  price       : 450.00
- sku         : BL4438H
  quantity    : 1
  description : Super Hoop
  price       : 2392.00
tax  : 251.42
total: 4443.52
comments:
  Late afternoon is best.
  Backup contact is Nancy
  Billsmer @ 338-4338.
```

**Example 2.28 Log File**

``` example
---
Time: 2001-11-23 15:01:42 -5
User: ed
Warning:
  This is an error message
  for the log file
---
Time: 2001-11-23 15:02:31 -5
User: ed
Warning:
  A slightly different error
  message.
---
Date: 2001-11-23 15:03:17 -5
User: ed
Fatal:
  Unknown variable "bar"
Stack:
- file: TopClass.py
  line: 23
  code: |
    x = MoreObject("345\n")
- file: MoreClass.py
  line: 58
  code: |-
    foo = bar
```


# Chapter 3. Processes and Models {#chapter-3-processes-and-models}

YAML is both a text format and a method for [presenting](#presenting-the-serialization-tree) any [native data
structure](#representing-native-data-structures) in this format. Therefore, this specification defines two concepts: a
class of data objects called YAML [representations](#representation-graph) and a syntax for
[presenting](#presenting-the-serialization-tree) YAML [representations](#representation-graph) as a series of
characters, called a YAML [stream](#streams).

A YAML *processor* is a tool for converting information between these complementary views. It is assumed that a YAML
processor does its work on behalf of another module, called an *application*. This chapter describes the information
structures a YAML processor must provide to or obtain from the application.

YAML information is used in two ways: for machine processing and for human consumption. The challenge of reconciling
these two perspectives is best done in three distinct translation stages: [representation](#representation-graph),
[serialization](#serialization-tree) and [presentation](#presentation-stream). [Representation](#representation-graph)
addresses how YAML views [native data structures](#representing-native-data-structures) to achieve portability between
programming environments. [Serialization](#serialization-tree) concerns itself with turning a YAML
[representation](#representation-graph) into a serial form, that is, a form with sequential access constraints.
[Presentation](#presentation-stream) deals with the formatting of a YAML [serialization](#serialization-tree) as a
series of characters in a human-friendly manner.


## 3.1. Processes {#31-processes}

Translating between [native data structures](#representing-native-data-structures) and a character [stream](#streams) is
done in several logically distinct stages, each with a well defined input and output data model, as shown in the
following diagram:

**Figure 3.1. Processing Overview**

![Processing Overview](/spec/1.2.2/img/overview2.svg)

A YAML processor need not expose the [serialization](#serialization-tree) or [representation](#representation-graph)
stages. It may translate directly between [native data structures](#representing-native-data-structures) and a character
[stream](#streams) ([dump](#dump) and [load](#load) in the diagram above). However, such a direct translation should
take place so that the [native data structures](#representing-native-data-structures) are
[constructed](#constructing-native-data-structures) only from information available in the
[representation](#representation-graph). In particular, [mapping key order](#mapping), [comments](#comments) and [tag
handles](#tag-handles) should not be referenced during [construction](#constructing-native-data-structures).


### 3.1.1. Dump {#311-dump}

*Dumping* native data structures to a character [stream](#streams) is done using the following three stages:


Representing Native Data Structures

:   YAML *represents* any *native data structure* using three [node kinds](#nodes): [sequence](#sequence) - an ordered
    series of entries; [mapping](#mapping) - an unordered association of [unique](#node-comparison) [keys](#nodes) to
    [values](#nodes); and [scalar](#scalar) - any datum with opaque structure presentable as a series of Unicode
    characters.

:   Combined, these primitives generate directed graph structures. These primitives were chosen because they are both
    powerful and familiar: the [sequence](#sequence) corresponds to a Perl array and a Python list, the
    [mapping](#mapping) corresponds to a Perl hash table and a Python dictionary. The [scalar](#scalar) represents
    strings, integers, dates and other atomic data types.

:   Each YAML [node](#nodes) requires, in addition to its [kind](#nodes) and [content](#nodes), a [tag](#tags)
    specifying its data type. Type specifiers are either [global](#tags) URIs or are [local](#tags) in scope to a single
    [application](#processes-and-models). For example, an integer is represented in YAML with a [scalar](#scalar) plus
    the [global tag](#tags) "`tag:yaml.org,2002:int`{.language-plaintext .highlighter-rouge}". Similarly, an invoice
    object, particular to a given organization, could be represented as a [mapping](#mapping) together with the [local
    tag](#tags) "`!invoice`{.language-plaintext .highlighter-rouge}". This simple model can represent any data structure
    independent of programming language.


Serializing the Representation Graph

:   For sequential access mediums, such as an event callback API, a YAML [representation](#representation-graph) must be
    *serialized* to an ordered tree. Since in a YAML [representation](#representation-graph), [mapping keys](#nodes) are
    unordered and [nodes](#nodes) may be referenced more than once (have more than one incoming "arrow"), the
    serialization process is required to impose an [ordering](#mapping-key-order) on the [mapping keys](#nodes) and to
    replace the second and subsequent references to a given [node](#nodes) with place holders called
    [aliases](#anchors-and-aliases). YAML does not specify how these *serialization details* are chosen. It is up to the
    YAML [processor](#processes-and-models) to come up with human-friendly [key order](#mapping-key-order) and
    [anchor](#anchors-and-aliases) names, possibly with the help of the [application](#processes-and-models). The result
    of this process, a YAML [serialization tree](#serialization-tree), can then be traversed to produce a series of
    event calls for one-pass processing of YAML data.


Presenting the Serialization Tree

:   The final output process is *presenting* the YAML [serializations](#serialization-tree) as a character
    [stream](#streams) in a human-friendly manner. To maximize human readability, YAML offers a rich set of stylistic
    options which go far beyond the minimal functional needs of simple data storage. Therefore the YAML
    [processor](#processes-and-models) is required to introduce various *presentation details* when creating the
    [stream](#streams), such as the choice of [node styles](#node-styles), how to [format scalar
    content](#scalar-formats), the amount of [indentation](#indentation-spaces), which [tag handles](#tag-handles) to
    use, the [node tags](#node-tags) to leave [unspecified](#resolved-tags), the set of [directives](#directives) to
    provide and possibly even what [comments](#comments) to add. While some of this can be done with the help of the
    [application](#processes-and-models), in general this process should be guided by the preferences of the user.


### 3.1.2. Load {#312-load}

*Loading* [native data structures](#representing-native-data-structures) from a character [stream](#streams) is done
using the following three stages:


Parsing the Presentation Stream

:   *Parsing* is the inverse process of [presentation](#presentation-stream), it takes a [stream](#streams) of
    characters and produces a [serialization tree](#serialization-tree). Parsing discards all the
    [details](#presenting-the-serialization-tree) introduced in the [presentation](#presentation-stream) process,
    reporting only the [serialization tree](#serialization-tree). Parsing can fail due to
    [ill-formed](#well-formed-streams-and-identified-aliases) input.


Composing the Representation Graph

:   *Composing* takes a [serialization tree](#serialization-tree) and produces a [representation
    graph](#representation-graph). Composing discards all the [details](#presenting-the-serialization-tree) introduced
    in the [serialization](#serialization-tree) process, producing only the [representation
    graph](#representation-graph). Composing can fail due to any of several reasons, detailed
    [below](#loading-failure-points).


Constructing Native Data Structures

:   The final input process is *constructing* [native data structures](#representing-native-data-structures) from the
    YAML [representation](#representation-graph). Construction must be based only on the information available in the
    [representation](#representation-graph) and not on additional [serialization](#serialization-tree) or [presentation
    details](#presenting-the-serialization-tree) such as [comments](#comments), [directives](#directives), [mapping key
    order](#mapping), [node styles](#node-styles), [scalar content format](#scalar-formats),
    [indentation](#indentation-spaces) levels etc. Construction can fail due to the [unavailability](#available-tags) of
    the required [native data types](#representing-native-data-structures).


## 3.2. Information Models {#32-information-models}

This section specifies the formal details of the results of the above processes. To maximize data portability between
programming languages and implementations, users of YAML should be mindful of the distinction between
[serialization](#serialization-tree) or [presentation](#presentation-stream) properties and those which are part of the
YAML [representation](#representation-graph). Thus, while imposing a [order](#mapping-key-order) on [mapping
keys](#nodes) is necessary for flattening YAML [representations](#representation-graph) to a sequential access medium,
this [serialization detail](#serializing-the-representation-graph) must not be used to convey
[application](#processes-and-models) level information. In a similar manner, while [indentation](#indentation-spaces)
technique and a choice of a [node style](#node-styles) are needed for the human readability, these [presentation
details](#presenting-the-serialization-tree) are neither part of the YAML [serialization](#serialization-tree) nor the
YAML [representation](#representation-graph). By carefully separating properties needed for
[serialization](#serialization-tree) and [presentation](#presentation-stream), YAML
[representations](#representation-graph) of [application](#processes-and-models) information will be consistent and
portable between various programming environments.

The following diagram summarizes the three *information models*. Full arrows denote composition, hollow arrows denote
inheritance, "`1`{.language-plaintext .highlighter-rouge}" and "`*`{.language-plaintext .highlighter-rouge}" denote
"one" and "many" relationships. A single "`+`{.language-plaintext .highlighter-rouge}" denotes
[serialization](#serialization-tree) details, a double "`++`{.language-plaintext .highlighter-rouge}" denotes
[presentation](#presentation-stream) details.

**Figure 3.2. Information Models**

![Information Models](/spec/1.2.2/img/model2.svg)


### 3.2.1. Representation Graph {#321-representation-graph}

YAML's *representation* of [native data structure](#representing-native-data-structures) is a rooted, connected,
directed graph of [tagged](#tags) [nodes](#nodes). By "directed graph" we mean a set of [nodes](#nodes) and directed
edges ("arrows"), where each edge connects one [node](#nodes) to another (see a formal directed graph
definition^[13](#fn:digraph){.footnote rel="footnote"}^). All the [nodes](#nodes) must be reachable from the *root node*
via such edges. Note that the YAML graph may include cycles and a [node](#nodes) may have more than one incoming edge.

[Nodes](#nodes) that are defined in terms of other [nodes](#nodes) are [collections](#collections); [nodes](#nodes) that
are independent of any other [nodes](#nodes) are [scalars](#scalars). YAML supports two [kinds](#nodes) of [collection
nodes](#mapping): [sequences](#sequence) and [mappings](#mapping). [Mapping nodes](#mapping) are somewhat tricky because
their [keys](#nodes) are unordered and must be [unique](#node-comparison).

**Figure 3.3. Representation Model**

![Representation Model](/spec/1.2.2/img/represent2.svg)


#### 3.2.1.1. Nodes {#3211-nodes}

A YAML *node* [represents](#representation-graph) a single [native data
structure](#representing-native-data-structures). Such nodes have *content* of one of three *kinds*: scalar, sequence or
mapping. In addition, each node has a [tag](#tags) which serves to restrict the set of possible values the content can
have.


Scalar

:   The content of a *scalar* node is an opaque datum that can be [presented](#presenting-the-serialization-tree) as a
    series of zero or more Unicode characters.


Sequence

:   The content of a *sequence* node is an ordered series of zero or more nodes. In particular, a sequence may contain
    the same node more than once. It could even contain itself.


Mapping

:   The content of a *mapping* node is an unordered set of *key/value* node *pairs*, with the restriction that each of
    the keys is [unique](#node-comparison). YAML places no further restrictions on the nodes. In particular, keys may be
    arbitrary nodes, the same node may be used as the value of several key/value pairs and a mapping could even contain
    itself as a key or a value.


#### 3.2.1.2. Tags {#3212-tags}

YAML [represents](#representation-graph) type information of [native data
structures](#representing-native-data-structures) with a simple identifier, called a *tag*. *Global tags* are URIs and
hence globally unique across all [applications](#processes-and-models). The "`tag:`{.language-plaintext
.highlighter-rouge}" URI scheme^[14](#fn:tag-uri){.footnote rel="footnote"}^ is recommended for all global YAML tags. In
contrast, *local tags* are specific to a single [application](#processes-and-models). Local tags start with
"`!`{.language-plaintext .highlighter-rouge}", are not URIs and are not expected to be globally unique. YAML provides a
"`TAG`{.language-plaintext .highlighter-rouge}" directive to make tag notation less verbose; it also offers easy
migration from local to global tags. To ensure this, local tags are restricted to the URI character set and use URI
character [escaping](#escaped-characters).

YAML does not mandate any special relationship between different tags that begin with the same substring. Tags ending
with URI fragments (containing "`#`{.language-plaintext .highlighter-rouge}") are no exception; tags that share the same
base URI but differ in their fragment part are considered to be different, independent tags. By convention, fragments
are used to identify different "variants" of a tag, while "`/`{.language-plaintext .highlighter-rouge}" is used to
define nested tag "namespace" hierarchies. However, this is merely a convention and each tag may employ its own rules.
For example, Perl tags may use "`::`{.language-plaintext .highlighter-rouge}" to express namespace hierarchies, Java
tags may use "`.`{.language-plaintext .highlighter-rouge}", etc.

YAML tags are used to associate meta information with each [node](#nodes). In particular, each tag must specify the
expected [node kind](#nodes) ([scalar](#scalar), [sequence](#sequence) or [mapping](#mapping)). [Scalar](#scalar) tags
must also provide a mechanism for converting [formatted content](#scalar-formats) to a [canonical form](#canonical-form)
for supporting [equality](#equality) testing. Furthermore, a tag may provide additional information such as the set of
allowed [content](#nodes) values for validation, a mechanism for [tag resolution](#tag-resolution) or any other data
that is applicable to all of the tag's [nodes](#nodes).


#### 3.2.1.3. Node Comparison {#3213-node-comparison}

Since YAML [mappings](#mapping) require [key](#nodes) uniqueness, [representations](#representation-graph) must include
a mechanism for testing the equality of [nodes](#nodes). This is non-trivial since YAML allows various ways to [format
scalar content](#scalar-formats). For example, the integer eleven can be written as "`0o13`{.language-plaintext
.highlighter-rouge}" (octal) or "`0xB`{.language-plaintext .highlighter-rouge}" (hexadecimal). If both notations are
used as [keys](#nodes) in the same [mapping](#mapping), only a YAML [processor](#processes-and-models) which recognizes
integer [formats](#scalar-formats) would correctly flag the duplicate [key](#nodes) as an error.


Canonical Form

:   YAML supports the need for [scalar](#scalar) equality by requiring that every [scalar](#scalar) [tag](#tags) must
    specify a mechanism for producing the *canonical form* of any [formatted content](#scalar-formats). This form is a
    Unicode character string which also [presents](#presenting-the-serialization-tree) the same [content](#nodes) and
    can be used for equality testing.


Equality

:   Two [nodes](#nodes) must have the same [tag](#tags) and [content](#nodes) to be *equal*. Since each [tag](#tags)
    applies to exactly one [kind](#nodes), this implies that the two [nodes](#nodes) must have the same [kind](#nodes)
    to be equal.

:   Two [scalars](#scalars) are equal only when their [tags](#tags) and canonical forms are equal
    character-by-character. Equality of [collections](#collections) is defined recursively.

:   Two [sequences](#sequence) are equal only when they have the same [tag](#tags) and length and each [node](#nodes) in
    one [sequence](#sequence) is equal to the corresponding [node](#nodes) in the other [sequence](#sequence).

:   Two [mappings](#mapping) are equal only when they have the same [tag](#tags) and an equal set of [keys](#nodes) and
    each [key](#nodes) in this set is associated with equal [values](#nodes) in both [mappings](#mapping).

:   Different URI schemes may define different rules for testing the equality of URIs. Since a YAML
    [processor](#processes-and-models) cannot be reasonably expected to be aware of them all, it must resort to a simple
    character-by-character comparison of [tags](#tags) to ensure consistency. This also happens to be the comparison
    method defined by the "`tag:`{.language-plaintext .highlighter-rouge}" URI scheme. [Tags](#tags) in a YAML stream
    must therefore be [presented](#presenting-the-serialization-tree) in a canonical way so that such comparison would
    yield the correct results.

:   If a node has itself as a descendant (via an alias), then determining the equality of that node is
    implementation-defined.

:   A YAML [processor](#processes-and-models) may treat equal [scalars](#scalars) as if they were identical.


Uniqueness

:   A [mapping's](#mapping) [keys](#nodes) are *unique* if no two keys are equal to each other. Obviously, identical
    nodes are always considered equal.


### 3.2.2. Serialization Tree {#322-serialization-tree}

To express a YAML [representation](#representation-graph) using a serial API, it is necessary to impose an
[order](#mapping-key-order) on [mapping keys](#nodes) and employ [alias nodes](#alias-nodes) to indicate a subsequent
occurrence of a previously encountered [node](#nodes). The result of this process is a *serialization tree*, where each
[node](#nodes) has an ordered set of children. This tree can be traversed for a serial event-based API.
[Construction](#constructing-native-data-structures) of [native data structures](#representing-native-data-structures)
from the serial interface should not use [key order](#mapping-key-order) or [anchor names](#anchors-and-aliases) for the
preservation of [application](#processes-and-models) data.

**Figure 3.4. Serialization Model**

![Serialization Model](/spec/1.2.2/img/serialize2.svg)


#### 3.2.2.1. Mapping Key Order {#3221-mapping-key-order}

In the [representation](#representation-graph) model, [mapping keys](#nodes) do not have an order. To
[serialize](#serializing-the-representation-graph) a [mapping](#mapping), it is necessary to impose an *ordering* on its
[keys](#nodes). This order is a [serialization detail](#serializing-the-representation-graph) and should not be used
when [composing](#composing-the-representation-graph) the [representation graph](#representation-graph) (and hence for
the preservation of [application](#processes-and-models) data). In every case where [node](#nodes) order is significant,
a [sequence](#sequence) must be used. For example, an ordered [mapping](#mapping) can be
[represented](#representation-graph) as a [sequence](#sequence) of [mappings](#mapping), where each [mapping](#mapping)
is a single [key/value pair](#mapping). YAML provides convenient [compact
notation](#example-flow-mapping-adjacent-values) for this case.


#### 3.2.2.2. Anchors and Aliases {#3222-anchors-and-aliases}

In the [representation graph](#representation-graph), a [node](#nodes) may appear in more than one
[collection](#collections). When [serializing](#serializing-the-representation-graph) such data, the first occurrence of
the [node](#nodes) is *identified* by an *anchor*. Each subsequent occurrence is
[serialized](#serializing-the-representation-graph) as an [alias node](#alias-nodes) which refers back to this anchor.
Otherwise, anchor names are a [serialization detail](#serializing-the-representation-graph) and are discarded once
[composing](#composing-the-representation-graph) is completed. When [composing](#composing-the-representation-graph) a
[representation graph](#representation-graph) from [serialized](#serializing-the-representation-graph) events, an alias
event refers to the most recent event in the [serialization](#serialization-tree) having the specified anchor.
Therefore, anchors need not be unique within a [serialization](#serialization-tree). In addition, an anchor need not
have an alias node referring to it.


### 3.2.3. Presentation Stream {#323-presentation-stream}

A YAML *presentation* is a [stream](#streams) of Unicode characters making use of [styles](#node-styles), [scalar
content formats](#scalar-formats), [comments](#comments), [directives](#directives) and other [presentation
details](#presenting-the-serialization-tree) to [present](#presenting-the-serialization-tree) a YAML
[serialization](#serialization-tree) in a human readable way. YAML allows several [serialization
trees](#serialization-tree) to be contained in the same YAML presentation stream, as a series of [documents](#documents)
separated by [markers](#document-markers).

**Figure 3.5. Presentation Model**

![Presentation Model](/spec/1.2.2/img/present2.svg)


#### 3.2.3.1. Node Styles {#3231-node-styles}

Each [node](#nodes) is presented in some *style*, depending on its [kind](#nodes). The node style is a [presentation
detail](#presenting-the-serialization-tree) and is not reflected in the [serialization tree](#serialization-tree) or
[representation graph](#representation-graph). There are two groups of styles. [Block styles](#block-style-productions)
use [indentation](#indentation-spaces) to denote structure. In contrast, [flow styles](#flow-style-productions) rely on
explicit [indicators](#indicator-characters).

YAML provides a rich set of *scalar styles*. [Block scalar](#block-scalar-styles) styles include the [literal
style](#literal-style) and the [folded style](#folded-style). [Flow scalar](#flow-scalar-styles) styles include the
[plain style](#plain-style) and two quoted styles, the [single-quoted style](#single-quoted-style) and the
[double-quoted style](#double-quoted-style). These styles offer a range of trade-offs between expressive power and
readability.

Normally, [block sequences](#block-sequences) and [mappings](#mapping) begin on the next line. In some cases, YAML also
allows nested [block](#scalars) [collections](#collections) to start in-line for a more [compact
notation](#example-flow-mapping-adjacent-values). In addition, YAML provides a [compact
notation](#example-flow-mapping-adjacent-values) for [flow mappings](#flow-mappings) with a single [key/value
pair](#mapping), nested inside a [flow sequence](#flow-sequences). These allow for a natural "ordered mapping" notation.

**Figure 3.6. Kind/Style Combinations**

![Kind/Style Combinations](/spec/1.2.2/img/styles2.svg)


#### 3.2.3.2. Scalar Formats {#3232-scalar-formats}

YAML allows [scalars](#scalars) to be [presented](#presenting-the-serialization-tree) in several *formats*. For example,
the integer "`11`{.language-plaintext .highlighter-rouge}" might also be written as "`0xB`{.language-plaintext
.highlighter-rouge}". [Tags](#tags) must specify a mechanism for converting the formatted content to a [canonical
form](#canonical-form) for use in [equality](#equality) testing. Like [node style](#node-styles), the format is a
[presentation detail](#presenting-the-serialization-tree) and is not reflected in the [serialization
tree](#serialization-tree) and [representation graph](#representation-graph).


#### 3.2.3.3. Comments {#3233-comments}

[Comments](#comments) are a [presentation detail](#presenting-the-serialization-tree) and must not have any effect on
the [serialization tree](#serialization-tree) or [representation graph](#representation-graph). In particular, comments
are not associated with a particular [node](#nodes). The usual purpose of a comment is to communicate between the human
maintainers of a file. A typical example is comments in a configuration file. Comments must not appear inside
[scalars](#scalars), but may be interleaved with such [scalars](#scalars) inside [collections](#collections).


#### 3.2.3.4. Directives {#3234-directives}

Each [document](#documents) may be associated with a set of [directives](#directives). A directive has a name and an
optional sequence of parameters. Directives are instructions to the YAML [processor](#processes-and-models) and like all
other [presentation details](#presenting-the-serialization-tree) are not reflected in the YAML [serialization
tree](#serialization-tree) or [representation graph](#representation-graph). This version of YAML defines two
directives, "`YAML`{.language-plaintext .highlighter-rouge}" and "`TAG`{.language-plaintext .highlighter-rouge}". All
other directives are [reserved](#directives) for future versions of YAML.


## 3.3. Loading Failure Points {#33-loading-failure-points}

The process of [loading](#load) [native data structures](#representing-native-data-structures) from a YAML
[stream](#streams) has several potential *failure points*. The character [stream](#streams) may be
[ill-formed](#well-formed-streams-and-identified-aliases), [aliases](#anchors-and-aliases) may be
[unidentified](#well-formed-streams-and-identified-aliases), [unspecified tags](#resolved-tags) may be
[unresolvable](#resolved-tags), [tags](#tags) may be [unrecognized](#recognized-and-valid-tags), the [content](#nodes)
may be [invalid](#recognized-and-valid-tags), [mapping](#mapping) [keys](#nodes) may not be [unique](#node-comparison)
and a native type may be [unavailable](#available-tags). Each of these failures results with an incomplete loading.

A *partial representation* need not [resolve](#resolved-tags) the [tag](#tags) of each [node](#nodes) and the [canonical
form](#canonical-form) of [formatted scalar content](#scalar-formats) need not be available. This weaker representation
is useful for cases of incomplete knowledge of the types used in the [document](#documents).

In contrast, a *complete representation* specifies the [tag](#tags) of each [node](#nodes) and provides the [canonical
form](#canonical-form) of [formatted scalar content](#scalar-formats), allowing for [equality](#equality) testing. A
complete representation is required in order to [construct](#constructing-native-data-structures) [native data
structures](#representing-native-data-structures).

**Figure 3.7. Loading Failure Points**

![Loading Failure Points](/spec/1.2.2/img/validity2.svg)


### 3.3.1. Well-Formed Streams and Identified Aliases {#331-well-formed-streams-and-identified-aliases}

A [well-formed](#example-stream) character [stream](#streams) must match the BNF productions specified in the following
chapters. Successful loading also requires that each [alias](#anchors-and-aliases) shall refer to a previous
[node](#nodes) [identified](#anchors-and-aliases) by the [anchor](#anchors-and-aliases). A YAML
[processor](#processes-and-models) should reject *ill-formed streams* and *unidentified aliases*. A YAML
[processor](#processes-and-models) may recover from syntax errors, possibly by ignoring certain parts of the input, but
it must provide a mechanism for reporting such errors.


### 3.3.2. Resolved Tags {#332-resolved-tags}

Typically, most [tags](#tags) are not explicitly specified in the character [stream](#streams). During
[parsing](#parsing-the-presentation-stream), [nodes](#nodes) lacking an explicit [tag](#tags) are given a *non-specific
tag*: "`!`{.language-plaintext .highlighter-rouge}" for non-[plain scalars](#plain-style) and "`?`{.language-plaintext
.highlighter-rouge}" for all other [nodes](#nodes). [Composing](#composing-the-representation-graph) a [complete
representation](#loading-failure-points) requires each such non-specific tag to be *resolved* to a *specific tag*, be it
a [global tag](#tags) or a [local tag](#tags).

Resolving the [tag](#tags) of a [node](#nodes) must only depend on the following three parameters: (1) the non-specific
tag of the [node](#nodes), (2) the path leading from the [root](#representation-graph) to the [node](#nodes) and (3) the
[content](#nodes) (and hence the [kind](#nodes)) of the [node](#nodes). When a [node](#nodes) has more than one
occurrence (using [aliases](#anchors-and-aliases)), tag resolution must depend only on the path to the first
([anchored](#anchors-and-aliases)) occurrence of the [node](#nodes).

Note that resolution must not consider [presentation details](#presenting-the-serialization-tree) such as
[comments](#comments), [indentation](#indentation-spaces) and [node style](#node-styles). Also, resolution must not
consider the [content](#nodes) of any other [node](#nodes), except for the [content](#nodes) of the [key
nodes](#mapping) directly along the path leading from the [root](#representation-graph) to the resolved [node](#nodes).
Finally, resolution must not consider the [content](#nodes) of a sibling [node](#nodes) in a [collection](#collections)
or the [content](#nodes) of the [value node](#nodes) associated with a [key node](#mapping) being resolved.

These rules ensure that tag resolution can be performed as soon as a [node](#nodes) is first encountered in the
[stream](#streams), typically before its [content](#nodes) is [parsed](#parsing-the-presentation-stream). Also, tag
resolution only requires referring to a relatively small number of previously parsed [nodes](#nodes). Thus, in most
cases, tag resolution in one-pass [processors](#processes-and-models) is both possible and practical.

YAML [processors](#processes-and-models) should resolve [nodes](#nodes) having the "`!`{.language-plaintext
.highlighter-rouge}" non-specific tag as "`tag:yaml.org,2002:seq`{.language-plaintext .highlighter-rouge}",
"`tag:yaml.org,2002:map`{.language-plaintext .highlighter-rouge}" or "`tag:yaml.org,2002:str`{.language-plaintext
.highlighter-rouge}" depending on their [kind](#nodes). This *tag resolution convention* allows the author of a YAML
character [stream](#streams) to effectively "disable" the tag resolution process. By explicitly specifying a
"`!`{.language-plaintext .highlighter-rouge}" non-specific [tag property](#node-tags), the [node](#nodes) would then be
resolved to a "vanilla" [sequence](#sequence), [mapping](#mapping) or string, according to its [kind](#nodes).

[Application](#processes-and-models) specific tag resolution rules should be restricted to resolving the
"`?`{.language-plaintext .highlighter-rouge}" non-specific tag, most commonly to resolving [plain
scalars](#plain-style). These may be matched against a set of regular expressions to provide automatic resolution of
integers, floats, timestamps and similar types. An [application](#processes-and-models) may also match the
[content](#nodes) of [mapping nodes](#mapping) against sets of expected [keys](#nodes) to automatically resolve points,
complex numbers and similar types. Resolved [sequence node](#sequence) types such as the "ordered mapping" are also
possible.

That said, tag resolution is specific to the [application](#processes-and-models). YAML
[processors](#processes-and-models) should therefore provide a mechanism allowing the
[application](#processes-and-models) to override and expand these default tag resolution rules.

If a [document](#documents) contains *unresolved tags*, the YAML [processor](#processes-and-models) is unable to
[compose](#composing-the-representation-graph) a [complete representation](#loading-failure-points) graph. In such a
case, the YAML [processor](#processes-and-models) may [compose](#composing-the-representation-graph) a [partial
representation](#loading-failure-points), based on each [node's kind](#nodes) and allowing for non-specific tags.


### 3.3.3. Recognized and Valid Tags {#333-recognized-and-valid-tags}

To be *valid*, a [node](#nodes) must have a [tag](#tags) which is *recognized* by the YAML
[processor](#processes-and-models) and its [content](#nodes) must satisfy the constraints imposed by this [tag](#tags).
If a [document](#documents) contains a [scalar node](#nodes) with an *unrecognized tag* or *invalid content*, only a
[partial representation](#loading-failure-points) may be [composed](#composing-the-representation-graph). In contrast, a
YAML [processor](#processes-and-models) can always [compose](#composing-the-representation-graph) a [complete
representation](#loading-failure-points) for an unrecognized or an invalid [collection](#collections), since
[collection](#collections) [equality](#equality) does not depend upon knowledge of the [collection's](#mapping) data
type. However, such a [complete representation](#loading-failure-points) cannot be used to
[construct](#constructing-native-data-structures) a [native data structure](#representing-native-data-structures).


### 3.3.4. Available Tags {#334-available-tags}

In a given processing environment, there need not be an *available* native type corresponding to a given [tag](#tags).
If a [node's tag](#tags) is *unavailable*, a YAML [processor](#processes-and-models) will not be able to
[construct](#constructing-native-data-structures) a [native data structure](#representing-native-data-structures) for
it. In this case, a [complete representation](#loading-failure-points) may still be
[composed](#composing-the-representation-graph) and an [application](#processes-and-models) may wish to use this
[representation](#representation-graph) directly.


# Chapter 4. Syntax Conventions {#chapter-4-syntax-conventions}

The following chapters formally define the syntax of YAML character [streams](#streams), using parameterized BNF
productions. Each BNF production is both named and numbered for easy reference. Whenever possible, basic structures are
specified before the more complex structures using them in a "bottom up" fashion.

The productions are accompanied by examples which are presented in a two-pane side-by-side format. The left-hand side is
the YAML example and the right-hand side is an alternate YAML view of the example. The right-hand view uses JSON when
possible. Otherwise it uses a YAML form that is as close to JSON as possible.


## 4.1. Production Syntax {#41-production-syntax}

Productions are defined using the syntax `production-name ::= term`{.language-plaintext .highlighter-rouge}, where a
term is either:


An atomic term

:   - A quoted string (`"abc"`{.language-plaintext .highlighter-rouge}), which matches that concatenation of characters.
      A single character is usually written with single quotes (`'a'`{.language-plaintext .highlighter-rouge}).
    - A hexadecimal number (`x0A`{.language-plaintext .highlighter-rouge}), which matches the character at that Unicode
      code point.
    - A range of hexadecimal numbers (`[x20-x7E]`{.language-plaintext .highlighter-rouge}), which matches any character
      whose Unicode code point is within that range.
    - The name of a production (`c-printable`{.language-plaintext .highlighter-rouge}), which matches that production.


A lookaround

:   - `[ lookahead = term ]`{.language-plaintext .highlighter-rouge}, which matches the empty string if
      `term`{.language-plaintext .highlighter-rouge} would match.
    - `[ lookahead ≠ term ]`{.language-plaintext .highlighter-rouge}, which matches the empty string if
      `term`{.language-plaintext .highlighter-rouge} would not match.
    - `[ lookbehind = term ]`{.language-plaintext .highlighter-rouge}, which matches the empty string if
      `term`{.language-plaintext .highlighter-rouge} would match beginning at any prior point on the line and ending at
      the current position.


A special production

:   - `<start-of-line>`{.language-plaintext .highlighter-rouge}, which matches the empty string at the beginning of a
      line.
    - `<end-of-input>`{.language-plaintext .highlighter-rouge}, matches the empty string at the end of the input.
    - `<empty>`{.language-plaintext .highlighter-rouge}, which (always) matches the empty string.


A parenthesized term

:   Matches its contents.


A concatenation

:   Is `term-one term-two`{.language-plaintext .highlighter-rouge}, which matches `term-one`{.language-plaintext
    .highlighter-rouge} followed by `term-two`{.language-plaintext .highlighter-rouge}.


A alternation

:   Is `term-one | term-two`{.language-plaintext .highlighter-rouge}, which matches the `term-one`{.language-plaintext
    .highlighter-rouge} if possible, or `term-two`{.language-plaintext .highlighter-rouge} otherwise.


A quantified term:

:   - `term?`{.language-plaintext .highlighter-rouge}, which matches `(term | <empty>)`{.language-plaintext
      .highlighter-rouge}.
    - `term*`{.language-plaintext .highlighter-rouge}, which matches `(term term* | <empty>)`{.language-plaintext
      .highlighter-rouge}.
    - `term+`{.language-plaintext .highlighter-rouge}, which matches `(term term*)`{.language-plaintext
      .highlighter-rouge}.

> Note: Quantified terms are always greedy.

The order of precedence is parenthesization, then quantification, then concatenation, then alternation.

Some lines in a production definition might have a comment like:

    production-a ::=
      production-b      # clarifying comment

These comments are meant to be informative only. For instance a comment that says
`# not followed by non-ws char`{.language-plaintext .highlighter-rouge} just means that you should be aware that actual
production rules will behave as described even though it might not be obvious from the content of that particular
production alone.


## 4.2. Production Parameters {#42-production-parameters}

Some productions have parameters in parentheses after the name, such as [`s-line-prefix(n,c)`{.language-plaintext
.highlighter-rouge}](#rule-s-line-prefix). A parameterized production is shorthand for a (infinite) series of
productions, each with a fixed value for each parameter.

For instance, this production:

    production-a(n) ::= production-b(n)

Is shorthand for:

    production-a(0) ::= production-b(0)
    production-a(1) ::= production-b(1)
    …

And this production:

    production-a(n) ::=
      ( production-b(n+m) production-c(n+m) )+

Is shorthand for:

    production-a(0) ::=
        ( production-b(0) production-c(0) )+
      | ( production-b(1) production-c(1) )+
      | …
    production-a(1) ::=
        ( production-b(1) production-c(1) )+
      | ( production-b(2) production-c(2) )+
      | …
    …

The parameters are as follows:


Indentation: `n`{.language-plaintext .highlighter-rouge} or `m`{.language-plaintext .highlighter-rouge}

:   May be any natural number, including zero. `n`{.language-plaintext .highlighter-rouge} may also be -1.


Context: `c`{.language-plaintext .highlighter-rouge}

:   This parameter allows productions to tweak their behavior according to their surrounding. YAML supports two groups
    of *contexts*, distinguishing between [block styles](#block-style-productions) and [flow
    styles](#flow-style-productions).

:   May be any of the following values:

:   - `BLOCK-IN`{.language-plaintext .highlighter-rouge} -- inside block context
    - `BLOCK-OUT`{.language-plaintext .highlighter-rouge} -- outside block context
    - `BLOCK-KEY`{.language-plaintext .highlighter-rouge} -- inside block key context
    - `FLOW-IN`{.language-plaintext .highlighter-rouge} -- inside flow context
    - `FLOW-OUT`{.language-plaintext .highlighter-rouge} -- outside flow context
    - `FLOW-KEY`{.language-plaintext .highlighter-rouge} -- inside flow key context

(Block) Chomping: `t`{.language-plaintext .highlighter-rouge}

:   The [line break](#line-break-characters) chomping behavior for flow scalars. May be any of the following values:

- `STRIP`{.language-plaintext .highlighter-rouge} -- remove all trailing newlines
- `CLIP`{.language-plaintext .highlighter-rouge} -- remove all trailing newlines except the first
- `KEEP`{.language-plaintext .highlighter-rouge} -- retain all trailing newlines


## 4.3. Production Naming Conventions {#43-production-naming-conventions}

To make it easier to follow production combinations, production names use a prefix-style naming convention. Each
production is given a prefix based on the type of characters it begins and ends with.

`e-`{.language-plaintext .highlighter-rouge}

:   A production matching no characters.

`c-`{.language-plaintext .highlighter-rouge}

:   A production starting and ending with a special character.

`b-`{.language-plaintext .highlighter-rouge}

:   A production matching a single [line break](#line-break-characters).

`nb-`{.language-plaintext .highlighter-rouge}

:   A production starting and ending with a non-[break](#line-break-characters) character.

`s-`{.language-plaintext .highlighter-rouge}

:   A production starting and ending with a [white space](#white-space-characters) character.

`ns-`{.language-plaintext .highlighter-rouge}

:   A production starting and ending with a non-[space](#white-space-characters) character.

`l-`{.language-plaintext .highlighter-rouge}

:   A production matching complete line(s).

`X-Y-`{.language-plaintext .highlighter-rouge}

:   A production starting with an `X-`{.language-plaintext .highlighter-rouge} character and ending with a
    `Y-`{.language-plaintext .highlighter-rouge} character, where `X-`{.language-plaintext .highlighter-rouge} and
    `Y-`{.language-plaintext .highlighter-rouge} are any of the above prefixes.

`X+`{.language-plaintext .highlighter-rouge}, `X-Y+`{.language-plaintext .highlighter-rouge}

:   A production as above, with the additional property that the matched content [indentation](#indentation-spaces)
    level is greater than the specified `n`{.language-plaintext .highlighter-rouge} parameter.


# Chapter 5. Character Productions {#chapter-5-character-productions}


## 5.1. Character Set {#51-character-set}

To ensure readability, YAML [streams](#streams) use only the *printable* subset of the Unicode character set. The
allowed character range explicitly excludes the C0 control block^[15](#fn:c0-block){.footnote rel="footnote"}^
`x00-x1F`{.language-plaintext .highlighter-rouge} (except for TAB `x09`{.language-plaintext .highlighter-rouge}, LF
`x0A`{.language-plaintext .highlighter-rouge} and CR `x0D`{.language-plaintext .highlighter-rouge} which are allowed),
DEL `x7F`{.language-plaintext .highlighter-rouge}, the C1 control block `x80-x9F`{.language-plaintext
.highlighter-rouge} (except for NEL `x85`{.language-plaintext .highlighter-rouge} which is allowed), the surrogate
block^[16](#fn:surrogates){.footnote rel="footnote"}^ `xD800-xDFFF`{.language-plaintext .highlighter-rouge},
`xFFFE`{.language-plaintext .highlighter-rouge} and `xFFFF`{.language-plaintext .highlighter-rouge}.

On input, a YAML [processor](#processes-and-models) must accept all characters in this printable subset.

On output, a YAML [processor](#processes-and-models) must only produce only characters in this printable subset.
Characters outside this set must be [presented](#presenting-the-serialization-tree) using [escape](#escaped-characters)
sequences. In addition, any allowed characters known to be non-printable should also be [escaped](#escaped-characters).

> Note: This isn't mandatory since a full implementation would require extensive character property tables.


``` rule
[1] c-printable ::=
                         # 8 bit
    x09                  # Tab (\t)
  | x0A                  # Line feed (LF \n)
  | x0D                  # Carriage Return (CR \r)
  | [x20-x7E]            # Printable ASCII
                         # 16 bit
  | x85                  # Next Line (NEL)
  | [xA0-xD7FF]          # Basic Multilingual Plane (BMP)
  | [xE000-xFFFD]        # Additional Unicode Areas
  | [x010000-x10FFFF]    # 32 bit
```

To ensure [JSON compatibility](#yaml-directives), YAML [processors](#processes-and-models) must allow all non-C0
characters inside [quoted scalars](#double-quoted-style). To ensure readability, non-printable characters should be
[escaped](#escaped-characters) on output, even inside such [scalars](#scalars).

> Note: JSON [quoted scalars](#double-quoted-style) cannot span multiple lines or contain
> [tabs](#white-space-characters), but YAML [quoted scalars](#double-quoted-style) can.


``` rule
[2] nb-json ::=
    x09              # Tab character
  | [x20-x10FFFF]    # Non-C0-control characters
```

> Note: The production name `nb-json`{.language-plaintext .highlighter-rouge} means "non-break JSON compatible" here.


## 5.2. Character Encodings {#52-character-encodings}

All characters mentioned in this specification are Unicode code points. Each such code point is written as one or more
bytes depending on the *character encoding* used. Note that in UTF-16, characters above `xFFFF`{.language-plaintext
.highlighter-rouge} are written as four bytes, using a surrogate pair.

The character encoding is a [presentation detail](#presenting-the-serialization-tree) and must not be used to convey
[content](#nodes) information.

On input, a YAML [processor](#processes-and-models) must support the UTF-8 and UTF-16 character encodings. For [JSON
compatibility](#yaml-directives), the UTF-32 encodings must also be supported.

If a character [stream](#streams) begins with a *byte order mark*, the character encoding will be taken to be as
indicated by the byte order mark. Otherwise, the [stream](#streams) must begin with an ASCII character. This allows the
encoding to be deduced by the pattern of null (`x00`{.language-plaintext .highlighter-rouge}) characters.

Byte order marks may appear at the start of any [document](#documents), however all [documents](#documents) in the same
[stream](#streams) must use the same character encoding.

To allow for [JSON compatibility](#yaml-directives), byte order marks are also allowed inside [quoted
scalars](#double-quoted-style). For readability, such [content](#nodes) byte order marks should be
[escaped](#escaped-characters) on output.

The encoding can therefore be deduced by matching the first few bytes of the [stream](#streams) with the following table
rows (in order):

                          Byte0   Byte1   Byte2   Byte3   Encoding
  ----------------------- ------- ------- ------- ------- ----------
  Explicit BOM            x00     x00     xFE     xFF     UTF-32BE
  ASCII first character   x00     x00     x00     any     UTF-32BE
  Explicit BOM            xFF     xFE     x00     x00     UTF-32LE
  ASCII first character   any     x00     x00     x00     UTF-32LE
  Explicit BOM            xFE     xFF                     UTF-16BE
  ASCII first character   x00     any                     UTF-16BE
  Explicit BOM            xFF     xFE                     UTF-16LE
  ASCII first character   any     x00                     UTF-16LE
  Explicit BOM            xEF     xBB     xBF             UTF-8
  Default                                                 UTF-8

The recommended output encoding is UTF-8. If another encoding is used, it is recommended that an explicit byte order
mark be used, even if the first [stream](#streams) character is ASCII.

For more information about the byte order mark and the Unicode character encoding schemes see the Unicode
FAQ^[17](#fn:uni-faq){.footnote rel="footnote"}^.


``` rule
[3] c-byte-order-mark ::= xFEFF
```

In the examples, byte order mark characters are displayed as "`⇔`{.language-plaintext .highlighter-rouge}".

**Example 5.1 Byte Order Mark**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     ⇔# Comment only.                                      |     # This stream contains no
|
|                                                           |     # documents, only comments.
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-byte-order-mark`{.legend-1}](#rule-c-byte-order-mark)

**Example 5.2 Invalid Byte Order Mark**

+-----------------------------------------------------------+-----------------------------------------------------------
+
| ``` error                                                 | ``` error
|
| - Invalid use of BOM                                      | ERROR:
|
| ⇔                                                         |  A BOM must not appear
|
| - Inside a document.                                      |  inside a document.
|
| ```                                                       | ```
|
+-----------------------------------------------------------+-----------------------------------------------------------
+


## 5.3. Indicator Characters {#53-indicator-characters}

*Indicators* are characters that have special semantics.

"`-`{.language-plaintext .highlighter-rouge}" (`x2D`{.language-plaintext .highlighter-rouge}, hyphen) denotes a [block
sequence](#block-sequences) entry.


``` rule
[4] c-sequence-entry ::= '-'
```

"`?`{.language-plaintext .highlighter-rouge}" (`x3F`{.language-plaintext .highlighter-rouge}, question mark) denotes a
[mapping key](#nodes).


``` rule
[5] c-mapping-key ::= '?'
```

"`:`{.language-plaintext .highlighter-rouge}" (`x3A`{.language-plaintext .highlighter-rouge}, colon) denotes a [mapping
value](#mapping).


``` rule
[6] c-mapping-value ::= ':'
```

**Example 5.3 Block Structure Indicators**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     sequence:                                             | :::: {.language-json .highlighter-rouge}
|
|     - one                                                 | ::: highlight
|
|     - two                                                 | ``` highlight
|
|     mapping:                                              | { "sequence": [
|
|       ? sky                                               |     "one",
|
|       : blue                                              |     "two" ],
|
|       sea : green                                         |   "mapping": {
|
|                                                           |     "sky": "blue",
|
|                                                           |     "sea": "green" } }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-sequence-entry`{.legend-1}](#rule-c-sequence-entry)
- [`c-mapping-key`{.legend-2}](#rule-c-mapping-key)
- [`c-mapping-value`{.legend-3}](#rule-c-mapping-value)

"`,`{.language-plaintext .highlighter-rouge}" (`x2C`{.language-plaintext .highlighter-rouge}, comma) ends a [flow
collection](#flow-collection-styles) entry.


``` rule
[7] c-collect-entry ::= ','
```

"`[`{.language-plaintext .highlighter-rouge}" (`x5B`{.language-plaintext .highlighter-rouge}, left bracket) starts a
[flow sequence](#flow-sequences).


``` rule
[8] c-sequence-start ::= '['
```

"`]`{.language-plaintext .highlighter-rouge}" (`x5D`{.language-plaintext .highlighter-rouge}, right bracket) ends a
[flow sequence](#flow-sequences).


``` rule
[9] c-sequence-end ::= ']'
```

"`{`{.language-plaintext .highlighter-rouge}" (`x7B`{.language-plaintext .highlighter-rouge}, left brace) starts a [flow
mapping](#flow-mappings).


``` rule
[10] c-mapping-start ::= '{'
```

"`}`{.language-plaintext .highlighter-rouge}" (`x7D`{.language-plaintext .highlighter-rouge}, right brace) ends a [flow
mapping](#flow-mappings).


``` rule
[11] c-mapping-end ::= '}'
```

**Example 5.4 Flow Collection Indicators**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     sequence: [ one, two, ]                               | :::: {.language-json .highlighter-rouge}
|
|     mapping: { sky: blue, sea: green }                    | ::: highlight
|
|                                                           | ``` highlight
|
|                                                           | { "sequence": [ "one", "two" ],
|
|                                                           |   "mapping":
|
|                                                           |     { "sky": "blue", "sea": "green" } }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-sequence-start`{.legend-1}](#rule-c-sequence-start)` `{.legend-1}[`c-sequence-end`{.legend-1}](#rule-c-sequence-en
d)
- [`c-mapping-start`{.legend-2}](#rule-c-mapping-start)` `{.legend-2}[`c-mapping-end`{.legend-2}](#rule-c-mapping-end)
- [`c-collect-entry`{.legend-3}](#rule-c-collect-entry)

"`#`{.language-plaintext .highlighter-rouge}" (`x23`{.language-plaintext .highlighter-rouge}, octothorpe, hash, sharp,
pound, number sign) denotes a [comment](#comments).


``` rule
[12] c-comment ::= '#'
```

**Example 5.5 Comment Indicator**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     # Comment only.                                       |     # This stream contains no
|
|                                                           |     # documents, only comments.
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-comment`{.legend-1}](#rule-c-comment)

"`&`{.language-plaintext .highlighter-rouge}" (`x26`{.language-plaintext .highlighter-rouge}, ampersand) denotes a
[node's anchor property](#anchors-and-aliases).


``` rule
[13] c-anchor ::= '&'
```

"`*`{.language-plaintext .highlighter-rouge}" (`x2A`{.language-plaintext .highlighter-rouge}, asterisk) denotes an
[alias node](#alias-nodes).


``` rule
[14] c-alias ::= '*'
```

The "`!`{.language-plaintext .highlighter-rouge}" (`x21`{.language-plaintext .highlighter-rouge}, exclamation) is used
for specifying [node tags](#node-tags). It is used to denote [tag handles](#tag-handles) used in [tag
directives](#tag-directives) and [tag properties](#node-tags); to denote [local tags](#tags); and as the [non-specific
tag](#resolved-tags) for non-[plain scalars](#plain-style).


``` rule
[15] c-tag ::= '!'
```

**Example 5.6 Node Property Indicators**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     anchored: !local &anchor value                        | :::: {.language-json .highlighter-rouge}
|
|     alias: *anchor                                        | ::: highlight
|
|                                                           | ``` highlight
|
|                                                           | { "anchored": !local &A1 "value",
|
|                                                           |   "alias": *A1 }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-tag`{.legend-1}](#rule-c-tag)
- [`c-anchor`{.legend-2}](#rule-c-anchor)
- [`c-alias`{.legend-3}](#rule-c-alias)

"`|`{.language-plaintext .highlighter-rouge}" (`7C`{.language-plaintext .highlighter-rouge}, vertical bar) denotes a
[literal block scalar](#literal-style).


``` rule
[16] c-literal ::= '|'
```

"`>`{.language-plaintext .highlighter-rouge}" (`x3E`{.language-plaintext .highlighter-rouge}, greater than) denotes a
[folded block scalar](#block-folding).


``` rule
[17] c-folded ::= '>'
```

**Example 5.7 Block Scalar Indicators**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     literal: |                                            | :::: {.language-json .highlighter-rouge}
|
|       some                                                | ::: highlight
|
|       text                                                | ``` highlight
|
|     folded: >                                             | { "literal": "some\ntext\n",
|
|       some                                                |   "folded": "some text\n" }
|
|       text                                                | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-literal`{.legend-1}](#rule-c-literal)
- [`c-folded`{.legend-2}](#rule-c-folded)

"`'`{.language-plaintext .highlighter-rouge}" (`x27`{.language-plaintext .highlighter-rouge}, apostrophe, single quote)
surrounds a [single-quoted flow scalar](#single-quoted-style).


``` rule
[18] c-single-quote ::= "'"
```

"`"`{.language-plaintext .highlighter-rouge}" (`x22`{.language-plaintext .highlighter-rouge}, double quote) surrounds a
[double-quoted flow scalar](#double-quoted-style).


``` rule
[19] c-double-quote ::= '"'
```

**Example 5.8 Quoted Scalar Indicators**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     single: 'text'                                        | :::: {.language-json .highlighter-rouge}
|
|     double: "text"                                        | ::: highlight
|
|                                                           | ``` highlight
|
|                                                           | { "single": "text",
|
|                                                           |   "double": "text" }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-single-quote`{.legend-1}](#rule-c-single-quote)
- [`c-double-quote`{.legend-2}](#rule-c-double-quote)

"`%`{.language-plaintext .highlighter-rouge}" (`x25`{.language-plaintext .highlighter-rouge}, percent) denotes a
[directive](#directives) line.


``` rule
[20] c-directive ::= '%'
```

**Example 5.9 Directive Indicator**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     %YAML 1.2                                             | :::: {.language-json .highlighter-rouge}
|
|     --- text                                              | ::: highlight
|
|                                                           | ``` highlight
|
|                                                           | "text"
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-directive`{.legend-1}](#rule-c-directive)

The "`@`{.language-plaintext .highlighter-rouge}" (`x40`{.language-plaintext .highlighter-rouge}, at) and "`` ` ``"
(`x60`{.language-plaintext .highlighter-rouge}, grave accent) are *reserved* for future use.


``` rule
[21] c-reserved ::=
    '@' | '`'
```

**Example 5.10 Invalid use of Reserved Indicators**

+-----------------------------------------------------------+-----------------------------------------------------------
+
| ``` error                                                 | ``` error
|
| commercial-at: @text                                      | ERROR:
|
| grave-accent: `text                                       |  Reserved indicators can't
|
| ```                                                       |  start a plain scalar.
|
|                                                           | ```
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

Any indicator character:


``` rule
[22] c-indicator ::=
    c-sequence-entry    # '-'
  | c-mapping-key       # '?'
  | c-mapping-value     # ':'
  | c-collect-entry     # ','
  | c-sequence-start    # '['
  | c-sequence-end      # ']'
  | c-mapping-start     # '{'
  | c-mapping-end       # '}'
  | c-comment           # '#'
  | c-anchor            # '&'
  | c-alias             # '*'
  | c-tag               # '!'
  | c-literal           # '|'
  | c-folded            # '>'
  | c-single-quote      # "'"
  | c-double-quote      # '"'
  | c-directive         # '%'
  | c-reserved          # '@' '`'
```

The "`[`{.language-plaintext .highlighter-rouge}", "`]`{.language-plaintext .highlighter-rouge}",
"`{`{.language-plaintext .highlighter-rouge}", "`}`{.language-plaintext .highlighter-rouge}" and
"`,`{.language-plaintext .highlighter-rouge}" indicators denote structure in [flow
collections](#flow-collection-styles). They are therefore forbidden in some cases, to avoid ambiguity in several
constructs. This is handled on a case-by-case basis by the relevant productions.


``` rule
[23] c-flow-indicator ::=
    c-collect-entry     # ','
  | c-sequence-start    # '['
  | c-sequence-end      # ']'
  | c-mapping-start     # '{'
  | c-mapping-end       # '}'
```


## 5.4. Line Break Characters {#54-line-break-characters}

YAML recognizes the following ASCII *line break* characters.


``` rule
[24] b-line-feed ::= x0A
```


``` rule
[25] b-carriage-return ::= x0D
```


``` rule
[26] b-char ::=
    b-line-feed          # x0A
  | b-carriage-return    # X0D
```

All other characters, including the form feed (`x0C`{.language-plaintext .highlighter-rouge}), are considered to be
non-break characters. Note that these include the *non-ASCII line breaks*: next line (`x85`{.language-plaintext
.highlighter-rouge}), line separator (`x2028`{.language-plaintext .highlighter-rouge}) and paragraph separator
(`x2029`{.language-plaintext .highlighter-rouge}).

[YAML version 1.1](#yaml-directives) did support the above non-ASCII line break characters; however, JSON does not.
Hence, to ensure [JSON compatibility](#yaml-directives), YAML treats them as non-break characters as of version 1.2.
YAML 1.2 [processors](#processes-and-models) [parsing](#parsing-the-presentation-stream) a [version
1.1](#yaml-directives) [document](#documents) should therefore treat these line breaks as non-break characters, with an
appropriate warning.


``` rule
[27] nb-char ::=
  c-printable - b-char - c-byte-order-mark
```

Line breaks are interpreted differently by different systems and have multiple widely used formats.


``` rule
[28] b-break ::=
    (
      b-carriage-return  # x0A
      b-line-feed
    )                    # x0D
  | b-carriage-return
  | b-line-feed
```

Line breaks inside [scalar content](#scalar) must be *normalized* by the YAML [processor](#processes-and-models). Each
such line break must be [parsed](#parsing-the-presentation-stream) into a single line feed character. The original line
break format is a [presentation detail](#presenting-the-serialization-tree) and must not be used to convey
[content](#nodes) information.


``` rule
[29] b-as-line-feed ::=
  b-break
```

Outside [scalar content](#scalar), YAML allows any line break to be used to terminate lines.


``` rule
[30] b-non-content ::=
  b-break
```

On output, a YAML [processor](#processes-and-models) is free to emit line breaks using whatever convention is most
appropriate.

In the examples, line breaks are sometimes displayed using the "`↓`{.language-plaintext .highlighter-rouge}" glyph for
clarity.

**Example 5.11 Line Break Characters**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     |                                                     | :::: {.language-json .highlighter-rouge}
|
|       Line break (no glyph)                               | ::: highlight
|
|       Line break (glyphed)↓                               | ``` highlight
|
|                                                           | "Line break (no glyph)\nLine break (glyphed)\n"
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`b-break`{.legend-1}](#rule-b-break)


## 5.5. White Space Characters {#55-white-space-characters}

YAML recognizes two *white space* characters: *space* and *tab*.


``` rule
[31] s-space ::= x20
```


``` rule
[32] s-tab ::= x09
```


``` rule
[33] s-white ::=
  s-space | s-tab
```

The rest of the ([printable](#character-set)) non-[break](#line-break-characters) characters are considered to be
non-space characters.


``` rule
[34] ns-char ::=
  nb-char - s-white
```

In the examples, tab characters are displayed as the glyph "`→`{.language-plaintext .highlighter-rouge}". Space
characters are sometimes displayed as the glyph "`·`{.language-plaintext .highlighter-rouge}" for clarity.

**Example 5.12 Tabs and Spaces**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     # Tabs and spaces                                     | :::: {.language-json .highlighter-rouge}
|
|     quoted:·"Quoted →"                                    | ::: highlight
|
|     block:→|                                              | ``` highlight
|
|     ··void main() {                                       | { "quoted": "Quoted \t",
|
|     ··→printf("Hello, world!\n");                         |   "block": "void main()
|
|     ··}                                                   |     {\n\tprintf(\"Hello, world!\\n\");\n}\n" }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`s-space`{.legend-1}](#rule-s-space)
- [`s-tab`{.legend-2}](#rule-s-tab)


## 5.6. Miscellaneous Characters {#56-miscellaneous-characters}

The YAML syntax productions make use of the following additional character classes:

A decimal digit for numbers:


``` rule
[35] ns-dec-digit ::=
  [x30-x39]             # 0-9
```

A hexadecimal digit for [escape sequences](#escaped-characters):


``` rule
[36] ns-hex-digit ::=
    ns-dec-digit        # 0-9
  | [x41-x46]           # A-F
  | [x61-x66]           # a-f
```

ASCII letter (alphabetic) characters:


``` rule
[37] ns-ascii-letter ::=
    [x41-x5A]           # A-Z
  | [x61-x7A]           # a-z
```

Word (alphanumeric) characters for identifiers:


``` rule
[38] ns-word-char ::=
    ns-dec-digit        # 0-9
  | ns-ascii-letter     # A-Z a-z
  | '-'                 # '-'
```

URI characters for [tags](#tags), as defined in the URI specification^[18](#fn:uri){.footnote rel="footnote"}^.

By convention, any URI characters other than the allowed printable ASCII characters are first *encoded* in UTF-8 and
then each byte is *escaped* using the "`%`{.language-plaintext .highlighter-rouge}" character. The YAML
[processor](#processes-and-models) must not expand such escaped characters. [Tag](#tags) characters must be preserved
and compared exactly as [presented](#presenting-the-serialization-tree) in the YAML [stream](#streams), without any
processing.


``` rule
[39] ns-uri-char ::=
    (
      '%'
      ns-hex-digit{2}
    )
  | ns-word-char
  | '#'
  | ';'
  | '/'
  | '?'
  | ':'
  | '@'
  | '&'
  | '='
  | '+'
  | '$'
  | ','
  | '_'
  | '.'
  | '!'
  | '~'
  | '*'
  | "'"
  | '('
  | ')'
  | '['
  | ']'
```

The "`!`{.language-plaintext .highlighter-rouge}" character is used to indicate the end of a [named tag
handle](#tag-handles); hence its use in [tag shorthands](#tag-shorthands) is restricted. In addition, such
[shorthands](#tag-shorthands) must not contain the "`[`{.language-plaintext .highlighter-rouge}",
"`]`{.language-plaintext .highlighter-rouge}", "`{`{.language-plaintext .highlighter-rouge}", "`}`{.language-plaintext
.highlighter-rouge}" and "`,`{.language-plaintext .highlighter-rouge}" characters. These characters would cause
ambiguity with [flow collection](#flow-collection-styles) structures.


``` rule
[40] ns-tag-char ::=
    ns-uri-char
  - c-tag               # '!'
  - c-flow-indicator
```


## 5.7. Escaped Characters {#57-escaped-characters}

All non-[printable](#character-set) characters must be *escaped*. YAML escape sequences use the "`\`{.language-plaintext
.highlighter-rouge}" notation common to most modern computer languages. Each escape sequence must be
[parsed](#parsing-the-presentation-stream) into the appropriate Unicode character. The original escape sequence is a
[presentation detail](#presenting-the-serialization-tree) and must not be used to convey [content](#nodes) information.

Note that escape sequences are only interpreted in [double-quoted scalars](#double-quoted-style). In all other [scalar
styles](#node-styles), the "`\`{.language-plaintext .highlighter-rouge}" character has no special meaning and
non-[printable](#character-set) characters are not available.


``` rule
[41] c-escape ::= '\'
```

YAML escape sequences are a superset of C's escape sequences:

Escaped ASCII null (`x00`{.language-plaintext .highlighter-rouge}) character.


``` rule
[42] ns-esc-null ::= '0'
```

Escaped ASCII bell (`x07`{.language-plaintext .highlighter-rouge}) character.


``` rule
[43] ns-esc-bell ::= 'a'
```

Escaped ASCII backspace (`x08`{.language-plaintext .highlighter-rouge}) character.


``` rule
[44] ns-esc-backspace ::= 'b'
```

Escaped ASCII horizontal tab (`x09`{.language-plaintext .highlighter-rouge}) character. This is useful at the start or
the end of a line to force a leading or trailing tab to become part of the [content](#nodes).


``` rule
[45] ns-esc-horizontal-tab ::=
  't' | x09
```

Escaped ASCII line feed (`x0A`{.language-plaintext .highlighter-rouge}) character.


``` rule
[46] ns-esc-line-feed ::= 'n'
```

Escaped ASCII vertical tab (`x0B`{.language-plaintext .highlighter-rouge}) character.


``` rule
[47] ns-esc-vertical-tab ::= 'v'
```

Escaped ASCII form feed (`x0C`{.language-plaintext .highlighter-rouge}) character.


``` rule
[48] ns-esc-form-feed ::= 'f'
```

Escaped ASCII carriage return (`x0D`{.language-plaintext .highlighter-rouge}) character.


``` rule
[49] ns-esc-carriage-return ::= 'r'
```

Escaped ASCII escape (`x1B`{.language-plaintext .highlighter-rouge}) character.


``` rule
[50] ns-esc-escape ::= 'e'
```

Escaped ASCII space (`x20`{.language-plaintext .highlighter-rouge}) character. This is useful at the start or the end of
a line to force a leading or trailing space to become part of the [content](#nodes).


``` rule
[51] ns-esc-space ::= x20
```

Escaped ASCII double quote (`x22`{.language-plaintext .highlighter-rouge}).


``` rule
[52] ns-esc-double-quote ::= '"'
```

Escaped ASCII slash (`x2F`{.language-plaintext .highlighter-rouge}), for [JSON compatibility](#yaml-directives).


``` rule
[53] ns-esc-slash ::= '/'
```

Escaped ASCII back slash (`x5C`{.language-plaintext .highlighter-rouge}).


``` rule
[54] ns-esc-backslash ::= '\'
```

Escaped Unicode next line (`x85`{.language-plaintext .highlighter-rouge}) character.


``` rule
[55] ns-esc-next-line ::= 'N'
```

Escaped Unicode non-breaking space (`xA0`{.language-plaintext .highlighter-rouge}) character.


``` rule
[56] ns-esc-non-breaking-space ::= '_'
```

Escaped Unicode line separator (`x2028`{.language-plaintext .highlighter-rouge}) character.


``` rule
[57] ns-esc-line-separator ::= 'L'
```

Escaped Unicode paragraph separator (`x2029`{.language-plaintext .highlighter-rouge}) character.


``` rule
[58] ns-esc-paragraph-separator ::= 'P'
```

Escaped 8-bit Unicode character.


``` rule
[59] ns-esc-8-bit ::=
  'x'
  ns-hex-digit{2}
```

Escaped 16-bit Unicode character.


``` rule
[60] ns-esc-16-bit ::=
  'u'
  ns-hex-digit{4}
```

Escaped 32-bit Unicode character.


``` rule
[61] ns-esc-32-bit ::=
  'U'
  ns-hex-digit{8}
```

Any escaped character:


``` rule
[62] c-ns-esc-char ::=
  c-escape         # '\'
  (
      ns-esc-null
    | ns-esc-bell
    | ns-esc-backspace
    | ns-esc-horizontal-tab
    | ns-esc-line-feed
    | ns-esc-vertical-tab
    | ns-esc-form-feed
    | ns-esc-carriage-return
    | ns-esc-escape
    | ns-esc-space
    | ns-esc-double-quote
    | ns-esc-slash
    | ns-esc-backslash
    | ns-esc-next-line
    | ns-esc-non-breaking-space
    | ns-esc-line-separator
    | ns-esc-paragraph-separator
    | ns-esc-8-bit
    | ns-esc-16-bit
    | ns-esc-32-bit
  )
```

**Example 5.13 Escaped Characters**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     - "Fun with \\"                                       | :::: {.language-json .highlighter-rouge}
|
|     - "\" \a \b \e \f"                                    | ::: highlight
|
|     - "\n \r \t \v \0"                                    | ``` highlight
|
|     - "\  \_ \N \L \P \                                   | [ "Fun with \\",
|
|       \x41 \u0041 \U00000041"                             |   "\" \u0007 \b \u001b \f",
|
|                                                           |   "\n \r \t \u000b \u0000",
|
|                                                           |   "\u0020 \u00a0 \u0085 \u2028 \u2029 A A A" ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-ns-esc-char`{.legend-1}](#rule-c-ns-esc-char)

**Example 5.14 Invalid Escaped Characters**

+-----------------------------------------------------------+-----------------------------------------------------------
+
| ``` error                                                 | ``` error
|
| Bad escapes:                                              | ERROR:
|
|   "\c                                                     | - c is an invalid escaped character.
|
|   \xq-"                                                   | - q and - are invalid hex digits.
|
| ```                                                       | ```
|
+-----------------------------------------------------------+-----------------------------------------------------------
+


# Chapter 6. Structural Productions {#chapter-6-structural-productions}


## 6.1. Indentation Spaces {#61-indentation-spaces}

In YAML [block styles](#block-style-productions), structure is determined by *indentation*. In general, indentation is
defined as a zero or more [space](#white-space-characters) characters at the start of a line.

To maintain portability, [tab](#white-space-characters) characters must not be used in indentation, since different
systems treat [tabs](#white-space-characters) differently. Note that most modern editors may be configured so that
pressing the [tab](#white-space-characters) key results in the insertion of an appropriate number of
[spaces](#white-space-characters).

The amount of indentation is a [presentation detail](#presenting-the-serialization-tree) and must not be used to convey
[content](#nodes) information.


``` rule
[63]
s-indent(0) ::=
  <empty>

# When n≥0
s-indent(n+1) ::=
  s-space s-indent(n)
```

A [block style](#block-style-productions) construct is terminated when encountering a line which is less indented than
the construct. The productions use the notation "`s-indent-less-than(n)`{.language-plaintext .highlighter-rouge}" and
"`s-indent-less-or-equal(n)`{.language-plaintext .highlighter-rouge}" to express this.


``` rule
[64]
s-indent-less-than(1) ::=
  <empty>

# When n≥1
s-indent-less-than(n+1) ::=
  s-space s-indent-less-than(n)
  | <empty>
```


``` rule
[65]
s-indent-less-or-equal(0) ::=
  <empty>

# When n≥0
s-indent-less-or-equal(n+1) ::=
  s-space s-indent-less-or-equal(n)
  | <empty>
```

Each [node](#nodes) must be indented further than its parent [node](#nodes). All sibling [nodes](#nodes) must use the
exact same indentation level. However the [content](#nodes) of each sibling [node](#nodes) may be further indented
independently.

**Example 6.1 Indentation Spaces**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     ··# Leading comment line spaces are                   | :::: {.language-json .highlighter-rouge}
|
|     ···# neither content nor indentation.                 | ::: highlight
|
|     ····                                                  | ``` highlight
|
|     Not indented:                                         | { "Not indented": {
|
|     ·By one space: |                                      |     "By one space": "By four\n  spaces\n",
|
|     ····By four                                           |     "Flow style": [
|
|     ······spaces                                          |       "By two",
|
|     ·Flow style: [    # Leading spaces                    |       "Also by two",
|
|     ···By two,        # in flow style                     |       "Still by two" ] } }
|
|     ··Also by two,    # are neither                       | ```
|
|     ··→Still by two   # content nor                       | :::
|
|     ····]             # indentation.                      | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`s-indent(n)`{.legend-1}](#rule-s-indent)
- `Content`{.legend-2}
- `Neither content nor indentation`{.legend-3}

The "`-`{.language-plaintext .highlighter-rouge}", "`?`{.language-plaintext .highlighter-rouge}" and
"`:`{.language-plaintext .highlighter-rouge}" characters used to denote [block collection](#block-collection-styles)
entries are perceived by people to be part of the indentation. This is handled on a case-by-case basis by the relevant
productions.

**Example 6.2 Indentation Indicators**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     ?·a                                                   | :::: {.language-json .highlighter-rouge}
|
|     :·-→b                                                 | ::: highlight
|
|     ··-··-→c                                              | ``` highlight
|
|     ·····-·d                                              | { "a":
|
|                                                           |   [ "b",
|
|                                                           |     [ "c",
|
|                                                           |       "d" ] ] }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- `Total Indentation`{.legend-1}
- [`s-indent(n)`{.legend-2}](#rule-s-indent)
- `Indicator as indentation`{.legend-3}


## 6.2. Separation Spaces {#62-separation-spaces}

Outside [indentation](#indentation-spaces) and [scalar content](#scalar), YAML uses [white
space](#white-space-characters) characters for *separation* between tokens within a line. Note that such [white
space](#white-space-characters) may safely include [tab](#white-space-characters) characters.

Separation spaces are a [presentation detail](#presenting-the-serialization-tree) and must not be used to convey
[content](#nodes) information.


``` rule
[66] s-separate-in-line ::=
    s-white+
  | <start-of-line>
```

**Example 6.3 Separation Spaces**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     -·foo:→·bar                                           | :::: {.language-json .highlighter-rouge}
|
|     - -·baz                                               | ::: highlight
|
|       -→baz                                               | ``` highlight
|
|                                                           | [ { "foo": "bar" },
|
|                                                           |   [ "baz",
|
|                                                           |     "baz" ] ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`s-separate-in-line`{.legend-1}](#rule-s-separate-in-line)


## 6.3. Line Prefixes {#63-line-prefixes}

Inside [scalar content](#scalar), each line begins with a non-[content](#nodes) *line prefix*. This prefix always
includes the [indentation](#indentation-spaces). For [flow scalar styles](#flow-scalar-styles) it additionally includes
all leading [white space](#white-space-characters), which may contain [tab](#white-space-characters) characters.

Line prefixes are a [presentation detail](#presenting-the-serialization-tree) and must not be used to convey
[content](#nodes) information.


``` rule
[67]
s-line-prefix(n,BLOCK-OUT) ::= s-block-line-prefix(n)
s-line-prefix(n,BLOCK-IN)  ::= s-block-line-prefix(n)
s-line-prefix(n,FLOW-OUT)  ::= s-flow-line-prefix(n)
s-line-prefix(n,FLOW-IN)   ::= s-flow-line-prefix(n)
```


``` rule
[68] s-block-line-prefix(n) ::=
  s-indent(n)
```


``` rule
[69] s-flow-line-prefix(n) ::=
  s-indent(n)
  s-separate-in-line?
```

**Example 6.4 Line Prefixes**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     plain: text                                           | :::: {.language-json .highlighter-rouge}
|
|     ··lines                                               | ::: highlight
|
|     quoted: "text                                         | ``` highlight
|
|     ··→lines"                                             | { "plain": "text lines",
|
|     block: |                                              |   "quoted": "text lines",
|
|     ··text                                                |   "block": "text\n \tlines\n" }
|
|     ···→lines                                             | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`s-flow-line-prefix(n)`{.legend-1}](#rule-s-flow-line-prefix)
- [`s-block-line-prefix(n)`{.legend-2}](#rule-s-block-line-prefix)
- [`s-indent(n)`{.legend-3}](#rule-s-indent)


## 6.4. Empty Lines {#64-empty-lines}

An *empty line* line consists of the non-[content](#nodes) [prefix](#tag-prefixes) followed by a [line
break](#line-break-characters).


``` rule
[70] l-empty(n,c) ::=
  (
      s-line-prefix(n,c)
    | s-indent-less-than(n)
  )
  b-as-line-feed
```

The semantics of empty lines depend on the [scalar style](#node-styles) they appear in. This is handled on a
case-by-case basis by the relevant productions.

**Example 6.5 Empty Lines**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     Folding:                                              | :::: {.language-json .highlighter-rouge}
|
|       "Empty line                                         | ::: highlight
|
|     ···→                                                  | ``` highlight
|
|       as a line feed"                                     | { "Folding": "Empty line\nas a line feed",
|
|     Chomping: |                                           |   "Chomping": "Clipped empty lines\n" }
|
|       Clipped empty lines                                 | ```
|
|     ·                                                     | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`l-empty(n,c)`{.legend-1}](#rule-l-empty)


## 6.5. Line Folding {#65-line-folding}

*Line folding* allows long lines to be broken for readability, while retaining the semantics of the original long line.
If a [line break](#line-break-characters) is followed by an [empty line](#empty-lines), it is *trimmed*; the first [line
break](#line-break-characters) is discarded and the rest are retained as [content](#nodes).


``` rule
[71] b-l-trimmed(n,c) ::=
  b-non-content
  l-empty(n,c)+
```

Otherwise (the following line is not [empty](#empty-lines)), the [line break](#line-break-characters) is converted to a
single [space](#white-space-characters) (`x20`{.language-plaintext .highlighter-rouge}).


``` rule
[72] b-as-space ::=
  b-break
```

A folded non-[empty line](#empty-lines) may end with either of the above [line breaks](#line-break-characters).


``` rule
[73] b-l-folded(n,c) ::=
  b-l-trimmed(n,c) | b-as-space
```

**Example 6.6 Line Folding**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     >-                                                    | :::: {.language-json .highlighter-rouge}
|
|       trimmed↓                                            | ::: highlight
|
|     ··↓                                                   | ``` highlight
|
|     ·↓                                                    | "trimmed\n\n\nas space"
|
|     ↓                                                     | ```
|
|       as↓                                                 | :::
|
|       space                                               | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`b-l-trimmed(n,c)`{.legend-1}](#rule-b-l-trimmed)
- [`b-as-space`{.legend-2}](#rule-b-as-space)

The above rules are common to both the [folded block style](#block-folding) and the [scalar flow
styles](#flow-scalar-styles). Folding does distinguish between these cases in the following way:


Block Folding

:   In the [folded block style](#block-folding), the final [line break](#line-break-characters) and trailing [empty
    lines](#empty-lines) are subject to [chomping](#block-chomping-indicator) and are never folded. In addition, folding
    does not apply to [line breaks](#line-break-characters) surrounding text lines that contain leading [white
    space](#white-space-characters). Note that such a [more-indented](#example-more-indented-lines) line may consist
    only of such leading [white space](#white-space-characters).

:   The combined effect of the *block line folding* rules is that each "paragraph" is interpreted as a line, [empty
    lines](#empty-lines) are interpreted as a line feed and the formatting of
    [more-indented](#example-more-indented-lines) lines is preserved.

**Example 6.7 Block Folding**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     >                                                     | :::: {.language-json .highlighter-rouge}
|
|     ··foo·↓                                               | ::: highlight
|
|     ·↓                                                    | ``` highlight
|
|     ··→·bar↓                                              | "foo \n\n\t bar\n\nbaz\n"
|
|     ↓                                                     | ```
|
|     ··baz↓                                                | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`b-l-folded(n,c)`{.legend-1}](#rule-b-l-folded)
- `Non-content spaces`{.legend-2}
- `Content spaces`{.legend-3}


Flow Folding

:   Folding in [flow styles](#flow-style-productions) provides more relaxed semantics. [Flow
    styles](#flow-style-productions) typically depend on explicit [indicators](#indicator-characters) rather than
    [indentation](#indentation-spaces) to convey structure. Hence spaces preceding or following the text in a line are a
    [presentation detail](#presenting-the-serialization-tree) and must not be used to convey [content](#nodes)
    information. Once all such spaces have been discarded, all [line breaks](#line-break-characters) are folded without
    exception.

:   The combined effect of the *flow line folding* rules is that each "paragraph" is interpreted as a line, [empty
    lines](#empty-lines) are interpreted as line feeds and text can be freely
    [more-indented](#example-more-indented-lines) without affecting the [content](#nodes) information.


``` rule
[74] s-flow-folded(n) ::=
  s-separate-in-line?
  b-l-folded(n,FLOW-IN)
  s-flow-line-prefix(n)
```

**Example 6.8 Flow Folding**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     "↓                                                    | :::: {.language-json .highlighter-rouge}
|
|     ··foo·↓                                               | ::: highlight
|
|     ·↓                                                    | ``` highlight
|
|     ··→·bar↓                                              | " foo\nbar\nbaz "
|
|     ↓                                                     | ```
|
|     ··baz↓ "                                              | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`s-flow-folded(n)`{.legend-1}](#rule-s-flow-folded)
- `Non-content spaces`{.legend-2}


## 6.6. Comments {#66-comments}

An explicit *comment* is marked by a "`#`{.language-plaintext .highlighter-rouge}" indicator. Comments are a
[presentation detail](#presenting-the-serialization-tree) and must not be used to convey [content](#nodes) information.

Comments must be [separated](#separation-spaces) from other tokens by [white space](#white-space-characters) characters.

> Note: To ensure [JSON compatibility](#yaml-directives), YAML [processors](#processes-and-models) must allow for the
> omission of the final comment [line break](#line-break-characters) of the input [stream](#streams). However, as this
> confuses many tools, YAML [processors](#processes-and-models) should terminate the [stream](#streams) with an explicit
> [line break](#line-break-characters) on output.


``` rule
[75] c-nb-comment-text ::=
  c-comment    # '#'
  nb-char*
```


``` rule
[76] b-comment ::=
    b-non-content
  | <end-of-input>
```


``` rule
[77] s-b-comment ::=
  (
    s-separate-in-line
    c-nb-comment-text?
  )?
  b-comment
```

**Example 6.9 Separated Comment**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     key:····# Comment↓                                    | :::: {.language-json .highlighter-rouge}
|
|       valueeof                                            | ::: highlight
|
|                                                           | ``` highlight
|
|                                                           | { "key": "value" }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-nb-comment-text`{.legend-1}](#rule-c-nb-comment-text)
- [`b-comment`{.legend-2}](#rule-b-comment)
- [`s-b-comment`{.legend-3}](#rule-s-b-comment)

Outside [scalar content](#scalar), comments may appear on a line of their own, independent of the
[indentation](#indentation-spaces) level. Note that outside [scalar content](#scalar), a line containing only [white
space](#white-space-characters) characters is taken to be a comment line.


``` rule
[78] l-comment ::=
  s-separate-in-line
  c-nb-comment-text?
  b-comment
```

**Example 6.10 Comment Lines**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     ··# Comment↓                                          |     # This stream contains no
|
|     ···↓                                                  |     # documents, only comments.
|
|     ↓                                                     |
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`s-b-comment`{.legend-1}](#rule-s-b-comment)
- [`l-comment`{.legend-2}](#rule-l-comment)

In most cases, when a line may end with a comment, YAML allows it to be followed by additional comment lines. The only
exception is a comment ending a [block scalar header](#block-scalar-headers).


``` rule
[79] s-l-comments ::=
  (
      s-b-comment
    | <start-of-line>
  )
  l-comment*
```

**Example 6.11 Multi-Line Comments**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     key:····# Comment↓                                    | :::: {.language-json .highlighter-rouge}
|
|     ········# lines↓                                      | ::: highlight
|
|       value↓                                              | ``` highlight
|
|     ↓                                                     | { "key": "value" }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`s-b-comment`{.legend-1}](#rule-s-b-comment)
- [`l-comment`{.legend-2}](#rule-l-comment)
- [`s-l-comments`{.legend-3}](#rule-s-l-comments)


## 6.7. Separation Lines {#67-separation-lines}

[Implicit keys](#example-single-pair-explicit-entry) are restricted to a single line. In all other cases, YAML allows
tokens to be separated by multi-line (possibly empty) [comments](#comments).

Note that structures following multi-line comment separation must be properly [indented](#indentation-spaces), even
though there is no such restriction on the separation [comment](#comments) lines themselves.


``` rule
[80]
s-separate(n,BLOCK-OUT) ::= s-separate-lines(n)
s-separate(n,BLOCK-IN)  ::= s-separate-lines(n)
s-separate(n,FLOW-OUT)  ::= s-separate-lines(n)
s-separate(n,FLOW-IN)   ::= s-separate-lines(n)
s-separate(n,BLOCK-KEY) ::= s-separate-in-line
s-separate(n,FLOW-KEY)  ::= s-separate-in-line
```


``` rule
[81] s-separate-lines(n) ::=
    (
      s-l-comments
      s-flow-line-prefix(n)
    )
  | s-separate-in-line
```

**Example 6.12 Separation Spaces**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     {·first:·Sammy,·last:·Sosa·}:↓                        | :::: {.language-json .highlighter-rouge}
|
|     # Statistics:                                         | ::: highlight
|
|     ··hr:··# Home runs                                    | ``` highlight
|
|     ·····65                                               | { { "first": "Sammy",
|
|     ··avg:·# Average                                      |     "last": "Sosa" }: {
|
|     ···0.278                                              |     "hr": 65,
|
|                                                           |     "avg": 0.278 } }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`s-separate-in-line`{.legend-1}](#rule-s-separate-in-line)
- [`s-separate-lines(n)`{.legend-2}](#rule-s-separate-lines)
- [`s-indent(n)`{.legend-3}](#rule-s-indent)


## 6.8. Directives {#68-directives}

*Directives* are instructions to the YAML [processor](#processes-and-models). This specification defines two directives,
"`YAML`{.language-plaintext .highlighter-rouge}" and "`TAG`{.language-plaintext .highlighter-rouge}", and *reserves* all
other directives for future use. There is no way to define private directives. This is intentional.

Directives are a [presentation detail](#presenting-the-serialization-tree) and must not be used to convey
[content](#nodes) information.


``` rule
[82] l-directive ::=
  c-directive            # '%'
  (
      ns-yaml-directive
    | ns-tag-directive
    | ns-reserved-directive
  )
  s-l-comments
```

Each directive is specified on a separate non-[indented](#indentation-spaces) line starting with the
"`%`{.language-plaintext .highlighter-rouge}" indicator, followed by the directive name and a list of parameters. The
semantics of these parameters depends on the specific directive. A YAML [processor](#processes-and-models) should ignore
unknown directives with an appropriate warning.


``` rule
[83] ns-reserved-directive ::=
  ns-directive-name
  (
    s-separate-in-line
    ns-directive-parameter
  )*
```


``` rule
[84] ns-directive-name ::=
  ns-char+
```


``` rule
[85] ns-directive-parameter ::=
  ns-char+
```

**Example 6.13 Reserved Directives**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     %FOO  bar baz # Should be ignored                     | :::: {.language-json .highlighter-rouge}
|
|                    # with a warning.                      | ::: highlight
|
|     --- "foo"                                             | ``` highlight
|
|                                                           | "foo"
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-reserved-directive`{.legend-1}](#rule-ns-reserved-directive)
- [`ns-directive-name`{.legend-2}](#rule-ns-directive-name)
- [`ns-directive-parameter`{.legend-3}](#rule-ns-directive-parameter)


### 6.8.1. "`YAML`{.language-plaintext .highlighter-rouge}" Directives {#681-yaml-directives}

The "`YAML`{.language-plaintext .highlighter-rouge}" directive specifies the version of YAML the [document](#documents)
conforms to. This specification defines version "`1.2`{.language-plaintext .highlighter-rouge}", including
recommendations for *YAML 1.1 processing*.

A version 1.2 YAML [processor](#processes-and-models) must accept [documents](#documents) with an explicit
"`%YAML 1.2`{.language-plaintext .highlighter-rouge}" directive, as well as [documents](#documents) lacking a
"`YAML`{.language-plaintext .highlighter-rouge}" directive. Such [documents](#documents) are assumed to conform to the
1.2 version specification. [Documents](#documents) with a "`YAML`{.language-plaintext .highlighter-rouge}" directive
specifying a higher minor version (e.g. "`%YAML 1.3`{.language-plaintext .highlighter-rouge}") should be processed with
an appropriate warning. [Documents](#documents) with a "`YAML`{.language-plaintext .highlighter-rouge}" directive
specifying a higher major version (e.g. "`%YAML 2.0`{.language-plaintext .highlighter-rouge}") should be rejected with
an appropriate error message.

A version 1.2 YAML [processor](#processes-and-models) must also accept [documents](#documents) with an explicit
"`%YAML 1.1`{.language-plaintext .highlighter-rouge}" directive. Note that version 1.2 is mostly a superset of version
1.1, defined for the purpose of ensuring *JSON compatibility*. Hence a version 1.2 [processor](#processes-and-models)
should process version 1.1 [documents](#documents) as if they were version 1.2, giving a warning on points of
incompatibility (handling of [non-ASCII line breaks](#line-break-characters), as described
[above](#line-break-characters)).


``` rule
[86] ns-yaml-directive ::=
  "YAML"
  s-separate-in-line
  ns-yaml-version
```


``` rule
[87] ns-yaml-version ::=
  ns-dec-digit+
  '.'
  ns-dec-digit+
```

**Example 6.14 "`YAML`{.language-plaintext .highlighter-rouge}" directive**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     %YAML 1.3 # Attempt parsing                           | :::: {.language-json .highlighter-rouge}
|
|                # with a warning                           | ::: highlight
|
|     ---                                                   | ``` highlight
|
|     "foo"                                                 | "foo"
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-yaml-directive`{.legend-1}](#rule-ns-yaml-directive)
- [`ns-yaml-version`{.legend-2}](#rule-ns-yaml-version)

It is an error to specify more than one "`YAML`{.language-plaintext .highlighter-rouge}" directive for the same
document, even if both occurrences give the same version number.

**Example 6.15 Invalid Repeated YAML directive**

+-----------------------------------------------------------+-----------------------------------------------------------
+
| ``` error                                                 | ``` error
|
| %YAML 1.2                                                 | ERROR:
|
| %YAML 1.1                                                 | The YAML directive must only be
|
| foo                                                       | given at most once per document.
|
| ```                                                       | ```
|
+-----------------------------------------------------------+-----------------------------------------------------------
+


### 6.8.2. "`TAG`{.language-plaintext .highlighter-rouge}" Directives {#682-tag-directives}

The "`TAG`{.language-plaintext .highlighter-rouge}" directive establishes a [tag shorthand](#tag-shorthands) notation
for specifying [node tags](#node-tags). Each "`TAG`{.language-plaintext .highlighter-rouge}" directive associates a
[handle](#tag-handles) with a [prefix](#tag-prefixes). This allows for compact and readable [tag](#tags) notation.


``` rule
[88] ns-tag-directive ::=
  "TAG"
  s-separate-in-line
  c-tag-handle
  s-separate-in-line
  ns-tag-prefix
```

**Example 6.16 "`TAG`{.language-plaintext .highlighter-rouge}" directive**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     %TAG !yaml! tag:yaml.org,2002:                        | :::: {.language-json .highlighter-rouge}
|
|     ---                                                   | ::: highlight
|
|     !yaml!str "foo"                                       | ``` highlight
|
|                                                           | "foo"
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-tag-directive`{.legend-1}](#rule-ns-tag-directive)
- [`c-tag-handle`{.legend-2}](#rule-c-tag-handle)
- [`ns-tag-prefix`{.legend-3}](#rule-ns-tag-prefix)

It is an error to specify more than one "`TAG`{.language-plaintext .highlighter-rouge}" directive for the same
[handle](#tag-handles) in the same document, even if both occurrences give the same [prefix](#tag-prefixes).

**Example 6.17 Invalid Repeated TAG directive**

+-----------------------------------------------------------+-----------------------------------------------------------
+
| ``` error                                                 | ``` error
|
| %TAG ! !foo                                               | ERROR:
|
| %TAG ! !foo                                               | The TAG directive must only
|
| bar                                                       | be given at most once per
|
| ```                                                       | handle in the same document.
|
|                                                           | ```
|
+-----------------------------------------------------------+-----------------------------------------------------------
+


#### 6.8.2.1. Tag Handles {#6821-tag-handles}

The *tag handle* exactly matches the prefix of the affected [tag shorthand](#tag-shorthands). There are three tag handle
variants:


``` rule
[89] c-tag-handle ::=
    c-named-tag-handle
  | c-secondary-tag-handle
  | c-primary-tag-handle
```


Primary Handle

:   The *primary tag handle* is a single "`!`{.language-plaintext .highlighter-rouge}" character. This allows using the
    most compact possible notation for a single "primary" name space. By default, the prefix associated with this handle
    is "`!`{.language-plaintext .highlighter-rouge}". Thus, by default, [shorthands](#tag-shorthands) using this handle
    are interpreted as [local tags](#tags).

:   It is possible to override the default behavior by providing an explicit "`TAG`{.language-plaintext
    .highlighter-rouge}" directive, associating a different prefix for this handle. This provides smooth migration from
    using [local tags](#tags) to using [global tags](#tags) by the simple addition of a single
    "`TAG`{.language-plaintext .highlighter-rouge}" directive.


``` rule
[90] c-primary-tag-handle ::= '!'
```

**Example 6.18 Primary Tag Handle**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     # Private                                             | :::: {.language-json .highlighter-rouge}
|
|     !foo "bar"                                            | ::: highlight
|
|     ...                                                   | ``` highlight
|
|     # Global                                              | !<!foo> "bar"
|
|     %TAG ! tag:example.com,2000:app/                      | ---
|
|     ---                                                   | !<tag:example.com,2000:app/foo> "bar"
|
|     !foo "bar"                                            | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-primary-tag-handle`{.legend-1}](#rule-c-primary-tag-handle)


Secondary Handle

:   The *secondary tag handle* is written as "`!!`{.language-plaintext .highlighter-rouge}". This allows using a compact
    notation for a single "secondary" name space. By default, the prefix associated with this handle is
    "`tag:yaml.org,2002:`{.language-plaintext .highlighter-rouge}".

:   It is possible to override this default behavior by providing an explicit "`TAG`{.language-plaintext
    .highlighter-rouge}" directive associating a different prefix for this handle.


``` rule
[91] c-secondary-tag-handle ::= "!!"
```

**Example 6.19 Secondary Tag Handle**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     %TAG !! tag:example.com,2000:app/                     | :::: {.language-json .highlighter-rouge}
|
|     ---                                                   | ::: highlight
|
|     !!int 1 - 3 # Interval, not integer                   | ``` highlight
|
|                                                           | !<tag:example.com,2000:app/int> "1 - 3"
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-secondary-tag-handle`{.legend-1}](#rule-c-secondary-tag-handle)


Named Handles

:   A *named tag handle* surrounds a non-empty name with "`!`{.language-plaintext .highlighter-rouge}" characters. A
    handle name must not be used in a [tag shorthand](#tag-shorthands) unless an explicit "`TAG`{.language-plaintext
    .highlighter-rouge}" directive has associated some prefix with it.

:   The name of the handle is a [presentation detail](#presenting-the-serialization-tree) and must not be used to convey
    [content](#nodes) information. In particular, the YAML [processor](#processes-and-models) need not preserve the
    handle name once [parsing](#parsing-the-presentation-stream) is completed.


``` rule
[92] c-named-tag-handle ::=
  c-tag            # '!'
  ns-word-char+
  c-tag            # '!'
```

**Example 6.20 Tag Handles**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     %TAG !e! tag:example.com,2000:app/                    | :::: {.language-json .highlighter-rouge}
|
|     ---                                                   | ::: highlight
|
|     !e!foo "bar"                                          | ``` highlight
|
|                                                           | !<tag:example.com,2000:app/foo> "bar"
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-named-tag-handle`{.legend-1}](#rule-c-named-tag-handle)


#### 6.8.2.2. Tag Prefixes {#6822-tag-prefixes}

There are two *tag prefix* variants:


``` rule
[93] ns-tag-prefix ::=
  c-ns-local-tag-prefix | ns-global-tag-prefix
```


Local Tag Prefix

:   If the prefix begins with a "`!`{.language-plaintext .highlighter-rouge}" character, [shorthands](#tag-shorthands)
    using the [handle](#tag-handles) are expanded to a [local tag](#tags). Note that such a [tag](#tags) is
    intentionally not a valid URI and its semantics are specific to the [application](#processes-and-models). In
    particular, two [documents](#documents) in the same [stream](#streams) may assign different semantics to the same
    [local tag](#tags).


``` rule
[94] c-ns-local-tag-prefix ::=
  c-tag           # '!'
  ns-uri-char*
```

**Example 6.21 Local Tag Prefix**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     %TAG !m! !my-                                         | :::: {.language-json .highlighter-rouge}
|
|     --- # Bulb here                                       | ::: highlight
|
|     !m!light fluorescent                                  | ``` highlight
|
|     ...                                                   | !<!my-light> "fluorescent"
|
|     %TAG !m! !my-                                         | ---
|
|     --- # Color here                                      | !<!my-light> "green"
|
|     !m!light green                                        | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-ns-local-tag-prefix`{.legend-1}](#rule-c-ns-local-tag-prefix)


Global Tag Prefix

:   If the prefix begins with a character other than "`!`{.language-plaintext .highlighter-rouge}", it must be a valid
    URI prefix, and should contain at least the scheme. [Shorthands](#tag-shorthands) using the associated
    [handle](#tag-handles) are expanded to globally unique URI tags and their semantics is consistent across
    [applications](#processes-and-models). In particular, every [document](#documents) in every [stream](#streams) must
    assign the same semantics to the same [global tag](#tags).


``` rule
[95] ns-global-tag-prefix ::=
  ns-tag-char
  ns-uri-char*
```

**Example 6.22 Global Tag Prefix**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     %TAG !e! tag:example.com,2000:app/                    |     - !<tag:example.com,2000:app/foo> "bar"
|
|     ---                                                   |
|
|     - !e!foo "bar"                                        |
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-global-tag-prefix`{.legend-1}](#rule-ns-global-tag-prefix)


## 6.9. Node Properties {#69-node-properties}

Each [node](#nodes) may have two optional *properties*, [anchor](#anchors-and-aliases) and [tag](#tags), in addition to
its [content](#nodes). Node properties may be specified in any order before the [node's content](#nodes). Either or both
may be omitted.


``` rule
[96] c-ns-properties(n,c) ::=
    (
      c-ns-tag-property
      (
        s-separate(n,c)
        c-ns-anchor-property
      )?
    )
  | (
      c-ns-anchor-property
      (
        s-separate(n,c)
        c-ns-tag-property
      )?
    )
```

**Example 6.23 Node Properties**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     !!str &a1 "foo":                                      | :::: {.language-json .highlighter-rouge}
|
|       !!str bar                                           | ::: highlight
|
|     &a2 baz : *a1                                         | ``` highlight
|
|                                                           | { &B1 "foo": "bar",
|
|                                                           |   "baz": *B1 }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-ns-properties(n,c)`{.legend-1}](#rule-c-ns-properties)
- [`c-ns-anchor-property`{.legend-2}](#rule-c-ns-anchor-property)
- [`c-ns-tag-property`{.legend-3}](#rule-c-ns-tag-property)


### 6.9.1. Node Tags {#691-node-tags}

The *tag property* identifies the type of the [native data structure](#representing-native-data-structures)
[presented](#presenting-the-serialization-tree) by the [node](#nodes). A tag is denoted by the "`!`{.language-plaintext
.highlighter-rouge}" indicator.


``` rule
[97] c-ns-tag-property ::=
    c-verbatim-tag
  | c-ns-shorthand-tag
  | c-non-specific-tag
```


Verbatim Tags

:   A tag may be written *verbatim* by surrounding it with the "`<`{.language-plaintext .highlighter-rouge}" and
    "`>`{.language-plaintext .highlighter-rouge}" characters. In this case, the YAML [processor](#processes-and-models)
    must deliver the verbatim tag as-is to the [application](#processes-and-models). In particular, verbatim tags are
    not subject to [tag resolution](#tag-resolution). A verbatim tag must either begin with a "`!`{.language-plaintext
    .highlighter-rouge}" (a [local tag](#tags)) or be a valid URI (a [global tag](#tags)).


``` rule
[98] c-verbatim-tag ::=
  "!<"
  ns-uri-char+
  '>'
```

**Example 6.24 Verbatim Tags**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     !<tag:yaml.org,2002:str> foo :                        | :::: {.language-json .highlighter-rouge}
|
|       !<!bar> baz                                         | ::: highlight
|
|                                                           | ``` highlight
|
|                                                           | { "foo": !<!bar> "baz" }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-verbatim-tag`{.legend-1}](#rule-c-verbatim-tag)

**Example 6.25 Invalid Verbatim Tags**

+-----------------------------------------------------------+-----------------------------------------------------------
+
| ``` error                                                 | ``` error
|
| - !<!> foo                                                | ERROR:
|
| - !<$:?> bar                                              | - Verbatim tags aren't resolved,
|
| ```                                                       |   so ! is invalid.
|
|                                                           | - The $:? tag is neither a global
|
|                                                           |   URI tag nor a local tag starting
|
|                                                           |   with '!'.
|
|                                                           | ```
|
+-----------------------------------------------------------+-----------------------------------------------------------
+


Tag Shorthands

:   A *tag shorthand* consists of a valid [tag handle](#tag-handles) followed by a non-empty suffix. The [tag
    handle](#tag-handles) must be associated with a [prefix](#tag-prefixes), either by default or by using a
    "`TAG`{.language-plaintext .highlighter-rouge}" directive. The resulting [parsed](#parsing-the-presentation-stream)
    [tag](#tags) is the concatenation of the [prefix](#tag-prefixes) and the suffix and must either begin with
    "`!`{.language-plaintext .highlighter-rouge}" (a [local tag](#tags)) or be a valid URI (a [global tag](#tags)).

:   The choice of [tag handle](#tag-handles) is a [presentation detail](#presenting-the-serialization-tree) and must not
    be used to convey [content](#nodes) information. In particular, the [tag handle](#tag-handles) may be discarded once
    [parsing](#parsing-the-presentation-stream) is completed.

:   The suffix must not contain any "`!`{.language-plaintext .highlighter-rouge}" character. This would cause the tag
    shorthand to be interpreted as having a [named tag handle](#tag-handles). In addition, the suffix must not contain
    the "`[`{.language-plaintext .highlighter-rouge}", "`]`{.language-plaintext .highlighter-rouge}",
    "`{`{.language-plaintext .highlighter-rouge}", "`}`{.language-plaintext .highlighter-rouge}" and
    "`,`{.language-plaintext .highlighter-rouge}" characters. These characters would cause ambiguity with [flow
    collection](#flow-collection-styles) structures. If the suffix needs to specify any of the above restricted
    characters, they must be [escaped](#escaped-characters) using the "`%`{.language-plaintext .highlighter-rouge}"
    character. This behavior is consistent with the URI character escaping rules (specifically, section 2.3 of URI RFC).


``` rule
[99] c-ns-shorthand-tag ::=
  c-tag-handle
  ns-tag-char+
```

**Example 6.26 Tag Shorthands**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     %TAG !e! tag:example.com,2000:app/                    | :::: {.language-json .highlighter-rouge}
|
|     ---                                                   | ::: highlight
|
|     - !local foo                                          | ``` highlight
|
|     - !!str bar                                           | [ !<!local> "foo",
|
|     - !e!tag%21 baz                                       |   !<tag:yaml.org,2002:str> "bar",
|
|                                                           |   !<tag:example.com,2000:app/tag!> "baz" ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-ns-shorthand-tag`{.legend-1}](#rule-c-ns-shorthand-tag)

**Example 6.27 Invalid Tag Shorthands**

+-----------------------------------------------------------+-----------------------------------------------------------
+
| ``` error                                                 | ``` error
|
| %TAG !e! tag:example,2000:app/                            | ERROR:
|
| ---                                                       | - The !e! handle has no suffix.
|
| - !e! foo                                                 | - The !h! handle wasn't declared.
|
| - !h!bar baz                                              | ```
|
| ```                                                       |
|
+-----------------------------------------------------------+-----------------------------------------------------------
+


Non-Specific Tags

:   If a [node](#nodes) has no tag property, it is assigned a [non-specific tag](#resolved-tags) that needs to be
    [resolved](#resolved-tags) to a [specific](#resolved-tags) one. This [non-specific tag](#resolved-tags) is
    "`!`{.language-plaintext .highlighter-rouge}" for non-[plain scalars](#plain-style) and "`?`{.language-plaintext
    .highlighter-rouge}" for all other [nodes](#nodes). This is the only case where the [node style](#node-styles) has
    any effect on the [content](#nodes) information.

:   It is possible for the tag property to be explicitly set to the "`!`{.language-plaintext .highlighter-rouge}"
    non-specific tag. By [convention](#resolved-tags), this "disables" [tag resolution](#tag-resolution), forcing the
    [node](#nodes) to be interpreted as "`tag:yaml.org,2002:seq`{.language-plaintext .highlighter-rouge}",
    "`tag:yaml.org,2002:map`{.language-plaintext .highlighter-rouge}" or "`tag:yaml.org,2002:str`{.language-plaintext
    .highlighter-rouge}", according to its [kind](#nodes).

:   There is no way to explicitly specify the "`?`{.language-plaintext .highlighter-rouge}" non-specific tag. This is
    intentional.


``` rule
[100] c-non-specific-tag ::= '!'
```

**Example 6.28 Non-Specific Tags**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     # Assuming conventional resolution:                   | :::: {.language-json .highlighter-rouge}
|
|     - "12"                                                | ::: highlight
|
|     - 12                                                  | ``` highlight
|
|     - ! 12                                                | [ "12",
|
|                                                           |   12,
|
|                                                           |   "12" ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-non-specific-tag`{.legend-1}](#rule-c-non-specific-tag)


### 6.9.2. Node Anchors {#692-node-anchors}

An anchor is denoted by the "`&`{.language-plaintext .highlighter-rouge}" indicator. It marks a [node](#nodes) for
future reference. An [alias node](#alias-nodes) can then be used to indicate additional inclusions of the anchored
[node](#nodes). An anchored [node](#nodes) need not be referenced by any [alias nodes](#alias-nodes); in particular, it
is valid for all [nodes](#nodes) to be anchored.


``` rule
[101] c-ns-anchor-property ::=
  c-anchor          # '&'
  ns-anchor-name
```

Note that as a [serialization detail](#serializing-the-representation-graph), the anchor name is preserved in the
[serialization tree](#serialization-tree). However, it is not reflected in the [representation](#representation-graph)
graph and must not be used to convey [content](#nodes) information. In particular, the YAML
[processor](#processes-and-models) need not preserve the anchor name once the [representation](#representation-graph) is
[composed](#composing-the-representation-graph).

Anchor names must not contain the "`[`{.language-plaintext .highlighter-rouge}", "`]`{.language-plaintext
.highlighter-rouge}", "`{`{.language-plaintext .highlighter-rouge}", "`}`{.language-plaintext .highlighter-rouge}" and
"`,`{.language-plaintext .highlighter-rouge}" characters. These characters would cause ambiguity with [flow
collection](#flow-collection-styles) structures.


``` rule
[102] ns-anchor-char ::=
    ns-char - c-flow-indicator
```


``` rule
[103] ns-anchor-name ::=
  ns-anchor-char+
```

**Example 6.29 Node Anchors**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     First occurrence: &anchor Value                       | :::: {.language-json .highlighter-rouge}
|
|     Second occurrence: *anchor                            | ::: highlight
|
|                                                           | ``` highlight
|
|                                                           | { "First occurrence": &A "Value",
|
|                                                           |   "Second occurrence": *A }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-ns-anchor-property`{.legend-1}](#rule-c-ns-anchor-property)
- [`ns-anchor-name`{.legend-2}](#rule-ns-anchor-name)


# Chapter 7. Flow Style Productions {#chapter-7-flow-style-productions}

YAML's *flow styles* can be thought of as the natural extension of JSON to cover [folding](#line-folding) long content
lines for readability, [tagging](#tags) nodes to control [construction](#constructing-native-data-structures) of [native
data structures](#representing-native-data-structures) and using [anchors](#anchors-and-aliases) and
[aliases](#anchors-and-aliases) to reuse [constructed](#constructing-native-data-structures) object instances.


## 7.1. Alias Nodes {#71-alias-nodes}

Subsequent occurrences of a previously [serialized](#serializing-the-representation-graph) node are
[presented](#presenting-the-serialization-tree) as *alias nodes*. The first occurrence of the [node](#nodes) must be
marked by an [anchor](#anchors-and-aliases) to allow subsequent occurrences to be
[presented](#presenting-the-serialization-tree) as alias nodes.

An alias node is denoted by the "`*`{.language-plaintext .highlighter-rouge}" indicator. The alias refers to the most
recent preceding [node](#nodes) having the same [anchor](#anchors-and-aliases). It is an error for an alias node to use
an [anchor](#anchors-and-aliases) that does not previously occur in the [document](#documents). It is not an error to
specify an [anchor](#anchors-and-aliases) that is not used by any alias node.

Note that an alias node must not specify any [properties](#node-properties) or [content](#nodes), as these were already
specified at the first occurrence of the [node](#nodes).


``` rule
[104] c-ns-alias-node ::=
  c-alias           # '*'
  ns-anchor-name
```

**Example 7.1 Alias Nodes**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     First occurrence: &anchor Foo                         | :::: {.language-json .highlighter-rouge}
|
|     Second occurrence: *anchor                            | ::: highlight
|
|     Override anchor: &anchor Bar                          | ``` highlight
|
|     Reuse anchor: *anchor                                 | { "First occurrence": &A "Foo",
|
|                                                           |   "Override anchor": &B "Bar",
|
|                                                           |   "Second occurrence": *A,
|
|                                                           |   "Reuse anchor": *B }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-ns-alias-node`{.legend-1}](#rule-c-ns-alias-node)
- [`ns-anchor-name`{.legend-2}](#rule-ns-anchor-name)


## 7.2. Empty Nodes {#72-empty-nodes}

YAML allows the [node content](#nodes) to be omitted in many cases. [Nodes](#nodes) with empty [content](#nodes) are
interpreted as if they were [plain scalars](#plain-style) with an empty value. Such [nodes](#nodes) are commonly
resolved to a "`null`{.language-plaintext .highlighter-rouge}" value.


``` rule
[105] e-scalar ::= ""
```

In the examples, empty [scalars](#scalars) are sometimes displayed as the glyph "`°`{.language-plaintext
.highlighter-rouge}" for clarity. Note that this glyph corresponds to a position in the characters [stream](#streams)
rather than to an actual character.

**Example 7.2 Empty Content**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     {                                                     | :::: {.language-json .highlighter-rouge}
|
|       foo : !!str°,                                       | ::: highlight
|
|       !!str° : bar,                                       | ``` highlight
|
|     }                                                     | { "foo": "",
|
|                                                           |   "": "bar" }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`e-scalar`{.legend-1}](#rule-e-scalar)

Both the [node's properties](#node-properties) and [node content](#nodes) are optional. This allows for a *completely
empty node*. Completely empty nodes are only valid when following some explicit indication for their existence.


``` rule
[106] e-node ::=
  e-scalar    # ""
```

**Example 7.3 Completely Empty Flow Nodes**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     {                                                     | :::: {.language-json .highlighter-rouge}
|
|       ? foo :°,                                           | ::: highlight
|
|       °: bar,                                             | ``` highlight
|
|     }                                                     | { "foo": null,
|
|                                                           |   null : "bar" }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`e-node`{.legend-1}](#rule-e-node)


## 7.3. Flow Scalar Styles {#73-flow-scalar-styles}

YAML provides three *flow scalar styles*: [double-quoted](#double-quoted-style), [single-quoted](#single-quoted-style)
and [plain](#plain-style) (unquoted). Each provides a different trade-off between readability and expressive power.

The [scalar style](#node-styles) is a [presentation detail](#presenting-the-serialization-tree) and must not be used to
convey [content](#nodes) information, with the exception that [plain scalars](#plain-style) are distinguished for the
purpose of [tag resolution](#tag-resolution).


### 7.3.1. Double-Quoted Style {#731-double-quoted-style}

The *double-quoted style* is specified by surrounding "`"`{.language-plaintext .highlighter-rouge}" indicators. This is
the only [style](#node-styles) capable of expressing arbitrary strings, by using "`\`{.language-plaintext
.highlighter-rouge}" [escape sequences](#escaped-characters). This comes at the cost of having to escape the
"`\`{.language-plaintext .highlighter-rouge}" and "`"`{.language-plaintext .highlighter-rouge}" characters.


``` rule
[107] nb-double-char ::=
    c-ns-esc-char
  | (
        nb-json
      - c-escape          # '\'
      - c-double-quote    # '"'
    )
```


``` rule
[108] ns-double-char ::=
  nb-double-char - s-white
```

Double-quoted scalars are restricted to a single line when contained inside an [implicit
key](#example-single-pair-explicit-entry).


``` rule
[109] c-double-quoted(n,c) ::=
  c-double-quote         # '"'
  nb-double-text(n,c)
  c-double-quote         # '"'
```


``` rule
[110]
nb-double-text(n,FLOW-OUT)  ::= nb-double-multi-line(n)
nb-double-text(n,FLOW-IN)   ::= nb-double-multi-line(n)
nb-double-text(n,BLOCK-KEY) ::= nb-double-one-line
nb-double-text(n,FLOW-KEY)  ::= nb-double-one-line
```


``` rule
[111] nb-double-one-line ::=
  nb-double-char*
```

**Example 7.4 Double Quoted Implicit Keys**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     "implicit block key" : [                              | :::: {.language-json .highlighter-rouge}
|
|       "implicit flow key" : value,                        | ::: highlight
|
|      ]                                                    | ``` highlight
|
|                                                           | { "implicit block key":
|
|                                                           |   [ { "implicit flow key": "value" } ] }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`nb-double-one-line`{.legend-1}](#rule-nb-double-one-line)
- [`c-double-quoted(n,c)`{.legend-2}](#rule-c-double-quoted)

In a multi-line double-quoted scalar, [line breaks](#line-break-characters) are subject to [flow line
folding](#flow-folding), which discards any trailing [white space](#white-space-characters) characters. It is also
possible to *escape* the [line break](#line-break-characters) character. In this case, the escaped [line
break](#line-break-characters) is excluded from the [content](#nodes) and any trailing [white
space](#white-space-characters) characters that precede the escaped line break are preserved. Combined with the ability
to [escape](#escaped-characters) [white space](#white-space-characters) characters, this allows double-quoted lines to
be broken at arbitrary positions.


``` rule
[112] s-double-escaped(n) ::=
  s-white*
  c-escape         # '\'
  b-non-content
  l-empty(n,FLOW-IN)*
  s-flow-line-prefix(n)
```


``` rule
[113] s-double-break(n) ::=
    s-double-escaped(n)
  | s-flow-folded(n)
```

**Example 7.5 Double Quoted Line Breaks**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     "folded·↓                                             | :::: {.language-json .highlighter-rouge}
|
|     to a space,→↓                                         | ::: highlight
|
|     ·↓                                                    | ``` highlight
|
|     to a line feed, or·→\↓                                | "folded to a space,\nto a line feed, or \t \tnon-content"
|
|     ·\·→non-content"                                      | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`s-flow-folded(n)`{.legend-1}](#rule-s-flow-folded)
- [`s-double-escaped(n)`{.legend-2}](#rule-s-double-escaped)

All leading and trailing [white space](#white-space-characters) characters on each line are excluded from the
[content](#nodes). Each continuation line must therefore contain at least one non-[space](#white-space-characters)
character. Empty lines, if any, are consumed as part of the [line folding](#line-folding).


``` rule
[114] nb-ns-double-in-line ::=
  (
    s-white*
    ns-double-char
  )*
```


``` rule
[115] s-double-next-line(n) ::=
  s-double-break(n)
  (
    ns-double-char nb-ns-double-in-line
    (
        s-double-next-line(n)
      | s-white*
    )
  )?
```


``` rule
[116] nb-double-multi-line(n) ::=
  nb-ns-double-in-line
  (
      s-double-next-line(n)
    | s-white*
  )
```

**Example 7.6 Double Quoted Lines**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     "·1st non-empty↓                                      | :::: {.language-json .highlighter-rouge}
|
|     ↓                                                     | ::: highlight
|
|     ·2nd non-empty·                                       | ``` highlight
|
|     →3rd non-empty·"                                      | " 1st non-empty\n2nd non-empty 3rd non-empty "
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`nb-ns-double-in-line`{.legend-1}](#rule-nb-ns-double-in-line)
- [`s-double-next-line(n)`{.legend-2}](#rule-s-double-next-line)


### 7.3.2. Single-Quoted Style {#732-single-quoted-style}

The *single-quoted style* is specified by surrounding "`'`{.language-plaintext .highlighter-rouge}" indicators.
Therefore, within a single-quoted scalar, such characters need to be repeated. This is the only form of *escaping*
performed in single-quoted scalars. In particular, the "`\`{.language-plaintext .highlighter-rouge}" and
"`"`{.language-plaintext .highlighter-rouge}" characters may be freely used. This restricts single-quoted scalars to
[printable](#character-set) characters. In addition, it is only possible to break a long single-quoted line where a
[space](#white-space-characters) character is surrounded by non-[spaces](#white-space-characters).


``` rule
[117] c-quoted-quote ::= "''"
```


``` rule
[118] nb-single-char ::=
    c-quoted-quote
  | (
        nb-json
      - c-single-quote    # "'"
    )
```


``` rule
[119] ns-single-char ::=
  nb-single-char - s-white
```

**Example 7.7 Single Quoted Characters**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     'here''s to "quotes"'                                 | :::: {.language-json .highlighter-rouge}
|
|                                                           | ::: highlight
|
|                                                           | ``` highlight
|
|                                                           | "here's to \"quotes\""
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-quoted-quote`{.legend-1}](#rule-c-quoted-quote)

Single-quoted scalars are restricted to a single line when contained inside a [implicit
key](#example-single-pair-explicit-entry).


``` rule
[120] c-single-quoted(n,c) ::=
  c-single-quote    # "'"
  nb-single-text(n,c)
  c-single-quote    # "'"
```


``` rule
[121]
nb-single-text(FLOW-OUT)  ::= nb-single-multi-line(n)
nb-single-text(FLOW-IN)   ::= nb-single-multi-line(n)
nb-single-text(BLOCK-KEY) ::= nb-single-one-line
nb-single-text(FLOW-KEY)  ::= nb-single-one-line
```


``` rule
[122] nb-single-one-line ::=
  nb-single-char*
```

**Example 7.8 Single Quoted Implicit Keys**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     'implicit block key' : [                              | :::: {.language-json .highlighter-rouge}
|
|       'implicit flow key' : value,                        | ::: highlight
|
|      ]                                                    | ``` highlight
|
|                                                           | { "implicit block key":
|
|                                                           |   [ { "implicit flow key": "value" } ] }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`nb-single-one-line`{.legend-1}](#rule-nb-single-one-line)
- [`c-single-quoted(n,c)`{.legend-2}](#rule-c-single-quoted)

All leading and trailing [white space](#white-space-characters) characters are excluded from the [content](#nodes). Each
continuation line must therefore contain at least one non-[space](#white-space-characters) character. Empty lines, if
any, are consumed as part of the [line folding](#line-folding).


``` rule
[123] nb-ns-single-in-line ::=
  (
    s-white*
    ns-single-char
  )*
```


``` rule
[124] s-single-next-line(n) ::=
  s-flow-folded(n)
  (
    ns-single-char
    nb-ns-single-in-line
    (
        s-single-next-line(n)
      | s-white*
    )
  )?
```


``` rule
[125] nb-single-multi-line(n) ::=
  nb-ns-single-in-line
  (
      s-single-next-line(n)
    | s-white*
  )
```

**Example 7.9 Single Quoted Lines**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     '·1st non-empty↓                                      | :::: {.language-json .highlighter-rouge}
|
|     ↓                                                     | ::: highlight
|
|     ·2nd non-empty·                                       | ``` highlight
|
|     →3rd non-empty·'                                      | " 1st non-empty\n2nd non-empty 3rd non-empty "
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`nb-ns-single-in-line(n)`{.legend-1}](#rule-nb-ns-single-in-line)
- [`s-single-next-line(n)`{.legend-2}](#rule-s-single-next-line)


### 7.3.3. Plain Style {#733-plain-style}

The *plain* (unquoted) style has no identifying [indicators](#indicator-characters) and provides no form of escaping. It
is therefore the most readable, most limited and most [context](#context-c) sensitive [style](#node-styles). In addition
to a restricted character set, a plain scalar must not be empty or contain leading or trailing [white
space](#white-space-characters) characters. It is only possible to break a long plain line where a
[space](#white-space-characters) character is surrounded by non-[spaces](#white-space-characters).

Plain scalars must not begin with most [indicators](#indicator-characters), as this would cause ambiguity with other
YAML constructs. However, the "`:`{.language-plaintext .highlighter-rouge}", "`?`{.language-plaintext
.highlighter-rouge}" and "`-`{.language-plaintext .highlighter-rouge}" [indicators](#indicator-characters) may be used
as the first character if followed by a non-[space](#white-space-characters) "safe" character, as this causes no
ambiguity.


``` rule
[126] ns-plain-first(c) ::=
    (
        ns-char
      - c-indicator
    )
  | (
      (
          c-mapping-key       # '?'
        | c-mapping-value     # ':'
        | c-sequence-entry    # '-'
      )
      [ lookahead = ns-plain-safe(c) ]
    )
```

Plain scalars must never contain the "`: `{.language-plaintext .highlighter-rouge}" and "` #`{.language-plaintext
.highlighter-rouge}" character combinations. Such combinations would cause ambiguity with [mapping](#mapping) [key/value
pairs](#mapping) and [comments](#comments). In addition, inside [flow collections](#flow-collection-styles), or when
used as [implicit keys](#example-single-pair-explicit-entry), plain scalars must not contain the
"`[`{.language-plaintext .highlighter-rouge}", "`]`{.language-plaintext .highlighter-rouge}", "`{`{.language-plaintext
.highlighter-rouge}", "`}`{.language-plaintext .highlighter-rouge}" and "`,`{.language-plaintext .highlighter-rouge}"
characters. These characters would cause ambiguity with [flow collection](#flow-collection-styles) structures.


``` rule
[127]
ns-plain-safe(FLOW-OUT)  ::= ns-plain-safe-out
ns-plain-safe(FLOW-IN)   ::= ns-plain-safe-in
ns-plain-safe(BLOCK-KEY) ::= ns-plain-safe-out
ns-plain-safe(FLOW-KEY)  ::= ns-plain-safe-in
```


``` rule
[128] ns-plain-safe-out ::=
  ns-char
```


``` rule
[129] ns-plain-safe-in ::=
  ns-char - c-flow-indicator
```


``` rule
[130] ns-plain-char(c) ::=
    (
        ns-plain-safe(c)
      - c-mapping-value    # ':'
      - c-comment          # '#'
    )
  | (
      [ lookbehind = ns-char ]
      c-comment          # '#'
    )
  | (
      c-mapping-value    # ':'
      [ lookahead = ns-plain-safe(c) ]
    )
```

**Example 7.10 Plain Characters**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     # Outside flow collection:                            | :::: {.language-json .highlighter-rouge}
|
|     - ::vector                                            | ::: highlight
|
|     - ": - ()"                                            | ``` highlight
|
|     - Up, up, and away!                                   | [ "::vector",
|
|     - -123                                                |   ": - ()",
|
|     - https://example.com/foo#bar                         |   "Up, up, and away!",
|
|     # Inside flow collection:                             |   -123,
|
|     - [ ::vector,                                         |   "http://example.com/foo#bar",
|
|       ": - ()",                                           |   [ "::vector",
|
|       "Up, up and away!",                                 |     ": - ()",
|
|       -123,                                               |     "Up, up, and away!",
|
|       https://example.com/foo#bar ]                       |     -123,
|
|                                                           |     "http://example.com/foo#bar" ] ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-plain-first(c)`{.legend-1}](#rule-ns-plain-first)
- [`ns-plain-char(c)`{.legend-2}](#rule-ns-plain-char)
- `Not ns-plain-first(c)`{.legend-3}
- `Not ns-plain-char(c)`{.legend-4}

Plain scalars are further restricted to a single line when contained inside an [implicit
key](#example-single-pair-explicit-entry).


``` rule
[131]
ns-plain(n,FLOW-OUT)  ::= ns-plain-multi-line(n,FLOW-OUT)
ns-plain(n,FLOW-IN)   ::= ns-plain-multi-line(n,FLOW-IN)
ns-plain(n,BLOCK-KEY) ::= ns-plain-one-line(BLOCK-KEY)
ns-plain(n,FLOW-KEY)  ::= ns-plain-one-line(FLOW-KEY)
```


``` rule
[132] nb-ns-plain-in-line(c) ::=
  (
    s-white*
    ns-plain-char(c)
  )*
```


``` rule
[133] ns-plain-one-line(c) ::=
  ns-plain-first(c)
  nb-ns-plain-in-line(c)
```

**Example 7.11 Plain Implicit Keys**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     implicit block key : [                                | :::: {.language-json .highlighter-rouge}
|
|       implicit flow key : value,                          | ::: highlight
|
|      ]                                                    | ``` highlight
|
|                                                           | { "implicit block key":
|
|                                                           |   [ { "implicit flow key": "value" } ] }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-plain-one-line(c)`{.legend-1}](#rule-ns-plain-one-line)

All leading and trailing [white space](#white-space-characters) characters are excluded from the [content](#nodes). Each
continuation line must therefore contain at least one non-[space](#white-space-characters) character. Empty lines, if
any, are consumed as part of the [line folding](#line-folding).


``` rule
[134] s-ns-plain-next-line(n,c) ::=
  s-flow-folded(n)
  ns-plain-char(c)
  nb-ns-plain-in-line(c)
```


``` rule
[135] ns-plain-multi-line(n,c) ::=
  ns-plain-one-line(c)
  s-ns-plain-next-line(n,c)*
```

**Example 7.12 Plain Lines**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     1st non-empty↓                                        | :::: {.language-json .highlighter-rouge}
|
|     ↓                                                     | ::: highlight
|
|     ·2nd non-empty·                                       | ``` highlight
|
|     →3rd non-empty                                        | "1st non-empty\n2nd non-empty 3rd non-empty"
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`nb-ns-plain-in-line(c)`{.legend-1}](#rule-nb-ns-plain-in-line)
- [`s-ns-plain-next-line(n,c)`{.legend-2}](#rule-s-ns-plain-next-line)


## 7.4. Flow Collection Styles {#74-flow-collection-styles}

A *flow collection* may be nested within a [block collection](#block-collection-styles)
(\[`FLOW-OUT`{.language-plaintext .highlighter-rouge} context\]), nested within another flow collection
(\[`FLOW-IN`{.language-plaintext .highlighter-rouge} context\]) or be a part of an [implicit
key](#example-single-pair-explicit-entry) (\[`FLOW-KEY`{.language-plaintext .highlighter-rouge} context\] or
\[`BLOCK-KEY`{.language-plaintext .highlighter-rouge} context\]). Flow collection entries are terminated by the
"`,`{.language-plaintext .highlighter-rouge}" indicator. The final "`,`{.language-plaintext .highlighter-rouge}" may be
omitted. This does not cause ambiguity because flow collection entries can never be [completely
empty](#example-empty-content).


``` rule
[136]
in-flow(n,FLOW-OUT)  ::= ns-s-flow-seq-entries(n,FLOW-IN)
in-flow(n,FLOW-IN)   ::= ns-s-flow-seq-entries(n,FLOW-IN)
in-flow(n,BLOCK-KEY) ::= ns-s-flow-seq-entries(n,FLOW-KEY)
in-flow(n,FLOW-KEY)  ::= ns-s-flow-seq-entries(n,FLOW-KEY)
```


### 7.4.1. Flow Sequences {#741-flow-sequences}

*Flow sequence content* is denoted by surrounding "`[`{.language-plaintext .highlighter-rouge}" and
"`]`{.language-plaintext .highlighter-rouge}" characters.


``` rule
[137] c-flow-sequence(n,c) ::=
  c-sequence-start    # '['
  s-separate(n,c)?
  in-flow(n,c)?
  c-sequence-end      # ']'
```

Sequence entries are separated by a "`,`{.language-plaintext .highlighter-rouge}" character.


``` rule
[138] ns-s-flow-seq-entries(n,c) ::=
  ns-flow-seq-entry(n,c)
  s-separate(n,c)?
  (
    c-collect-entry     # ','
    s-separate(n,c)?
    ns-s-flow-seq-entries(n,c)?
  )?
```

**Example 7.13 Flow Sequence**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     - [ one, two, ]                                       | :::: {.language-json .highlighter-rouge}
|
|     - [three ,four]                                       | ::: highlight
|
|                                                           | ``` highlight
|
|                                                           | [ [ "one",
|
|                                                           |     "two" ],
|
|                                                           |   [ "three",
|
|                                                           |     "four" ] ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-sequence-start`{.legend-1}](#rule-c-sequence-start)` `{.legend-1}[`c-sequence-end`{.legend-1}](#rule-c-sequence-en
d)
- [`ns-flow-seq-entry(n,c)`{.legend-2}](#rule-ns-flow-seq-entry)

Any [flow node](#flow-nodes) may be used as a flow sequence entry. In addition, YAML provides a [compact
notation](#example-flow-mapping-adjacent-values) for the case where a flow sequence entry is a [mapping](#mapping) with
a [single key/value pair](#mapping).


``` rule
[139] ns-flow-seq-entry(n,c) ::=
  ns-flow-pair(n,c) | ns-flow-node(n,c)
```

**Example 7.14 Flow Sequence Entries**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     [                                                     | :::: {.language-json .highlighter-rouge}
|
|     "double                                               | ::: highlight
|
|      quoted", 'single                                     | ``` highlight
|
|                quoted',                                   | [ "double quoted",
|
|     plain                                                 |   "single quoted",
|
|      text, [ nested ],                                    |   "plain text",
|
|     single: pair,                                         |   [ "nested" ],
|
|     ]                                                     |   { "single": "pair" } ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-flow-node(n,c)`{.legend-1}](#rule-ns-flow-node)
- [`ns-flow-pair(n,c)`{.legend-2}](#rule-ns-flow-pair)


### 7.4.2. Flow Mappings {#742-flow-mappings}

*Flow mappings* are denoted by surrounding "`{`{.language-plaintext .highlighter-rouge}" and "`}`{.language-plaintext
.highlighter-rouge}" characters.


``` rule
[140] c-flow-mapping(n,c) ::=
  c-mapping-start       # '{'
  s-separate(n,c)?
  ns-s-flow-map-entries(n,in-flow(c))?
  c-mapping-end         # '}'
```

Mapping entries are separated by a "`,`{.language-plaintext .highlighter-rouge}" character.


``` rule
[141] ns-s-flow-map-entries(n,c) ::=
  ns-flow-map-entry(n,c)
  s-separate(n,c)?
  (
    c-collect-entry     # ','
    s-separate(n,c)?
    ns-s-flow-map-entries(n,c)?
  )?
```

**Example 7.15 Flow Mappings**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     - { one : two , three: four , }                       | :::: {.language-json .highlighter-rouge}
|
|     - {five: six,seven : eight}                           | ::: highlight
|
|                                                           | ``` highlight
|
|                                                           | [ { "one": "two",
|
|                                                           |     "three": "four" },
|
|                                                           |   { "five": "six",
|
|                                                           |     "seven": "eight" } ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-mapping-start`{.legend-1}](#rule-c-mapping-start)` `{.legend-1}[`c-mapping-end`{.legend-1}](#rule-c-mapping-end)
- [`ns-flow-map-entry(n,c)`{.legend-2}](#rule-ns-flow-map-entry)

If the optional "`?`{.language-plaintext .highlighter-rouge}" mapping key indicator is specified, the rest of the entry
may be [completely empty](#example-empty-content).


``` rule
[142] ns-flow-map-entry(n,c) ::=
    (
      c-mapping-key    # '?' (not followed by non-ws char)
      s-separate(n,c)
      ns-flow-map-explicit-entry(n,c)
    )
  | ns-flow-map-implicit-entry(n,c)
```


``` rule
[143] ns-flow-map-explicit-entry(n,c) ::=
    ns-flow-map-implicit-entry(n,c)
  | (
      e-node    # ""
      e-node    # ""
    )
```

**Example 7.16 Flow Mapping Entries**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     {                                                     | :::: {.language-json .highlighter-rouge}
|
|     ? explicit: entry,                                    | ::: highlight
|
|     implicit: entry,                                      | ``` highlight
|
|     ?°°                                                   | { "explicit": "entry",
|
|     }                                                     |   "implicit": "entry",
|
|                                                           |   null: null }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-flow-map-explicit-entry(n,c)`{.legend-1}](#rule-ns-flow-map-explicit-entry)
- [`ns-flow-map-implicit-entry(n,c)`{.legend-2}](#rule-ns-flow-map-implicit-entry)
- [`e-node`{.legend-3}](#rule-e-node)

Normally, YAML insists the "`:`{.language-plaintext .highlighter-rouge}" mapping value indicator be
[separated](#separation-spaces) from the [value](#nodes) by [white space](#white-space-characters). A benefit of this
restriction is that the "`:`{.language-plaintext .highlighter-rouge}" character can be used inside [plain
scalars](#plain-style), as long as it is not followed by [white space](#white-space-characters). This allows for
unquoted URLs and timestamps. It is also a potential source for confusion as "`a:1`{.language-plaintext
.highlighter-rouge}" is a [plain scalar](#plain-style) and not a [key/value pair](#mapping).

Note that the [value](#nodes) may be [completely empty](#example-empty-content) since its existence is indicated by the
"`:`{.language-plaintext .highlighter-rouge}".


``` rule
[144] ns-flow-map-implicit-entry(n,c) ::=
    ns-flow-map-yaml-key-entry(n,c)
  | c-ns-flow-map-empty-key-entry(n,c)
  | c-ns-flow-map-json-key-entry(n,c)
```


``` rule
[145] ns-flow-map-yaml-key-entry(n,c) ::=
  ns-flow-yaml-node(n,c)
  (
      (
        s-separate(n,c)?
        c-ns-flow-map-separate-value(n,c)
      )
    | e-node    # ""
  )
```


``` rule
[146] c-ns-flow-map-empty-key-entry(n,c) ::=
  e-node    # ""
  c-ns-flow-map-separate-value(n,c)
```


``` rule
[147] c-ns-flow-map-separate-value(n,c) ::=
  c-mapping-value    # ':'
  [ lookahead ≠ ns-plain-safe(c) ]
  (
      (
        s-separate(n,c)
        ns-flow-node(n,c)
      )
    | e-node    # ""
  )
```

**Example 7.17 Flow Mapping Separate Values**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     {                                                     | :::: {.language-json .highlighter-rouge}
|
|     unquoted·:·"separate",                                | ::: highlight
|
|     https://foo.com,                                      | ``` highlight
|
|     omitted value:°,                                      | { "unquoted": "separate",
|
|     °:·omitted key,                                       |   "http://foo.com": null,
|
|     }                                                     |   "omitted value": null,
|
|                                                           |   null: "omitted key" }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-flow-yaml-node(n,c)`{.legend-1}](#rule-ns-flow-yaml-node)
- [`e-node`{.legend-2}](#rule-e-node)
- [`c-ns-flow-map-separate-value(n,c)`{.legend-3}](#rule-c-ns-flow-map-separate-value)

To ensure [JSON compatibility](#yaml-directives), if a [key](#nodes) inside a flow mapping is [JSON-like](#flow-nodes),
YAML allows the following [value](#nodes) to be specified adjacent to the "`:`{.language-plaintext .highlighter-rouge}".
This causes no ambiguity, as all [JSON-like](#flow-nodes) [keys](#nodes) are surrounded by
[indicators](#indicator-characters). However, as this greatly reduces readability, YAML
[processors](#processes-and-models) should [separate](#separation-spaces) the [value](#nodes) from the
"`:`{.language-plaintext .highlighter-rouge}" on output, even in this case.


``` rule
[148] c-ns-flow-map-json-key-entry(n,c) ::=
  c-flow-json-node(n,c)
  (
      (
        s-separate(n,c)?
        c-ns-flow-map-adjacent-value(n,c)
      )
    | e-node    # ""
  )
```


``` rule
[149] c-ns-flow-map-adjacent-value(n,c) ::=
  c-mapping-value          # ':'
  (
      (
        s-separate(n,c)?
        ns-flow-node(n,c)
      )
    | e-node    # ""
  )
```

**Example 7.18 Flow Mapping Adjacent Values**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     {                                                     | :::: {.language-json .highlighter-rouge}
|
|     "adjacent":value,                                     | ::: highlight
|
|     "readable":·value,                                    | ``` highlight
|
|     "empty":°                                             | { "adjacent": "value",
|
|     }                                                     |   "readable": "value",
|
|                                                           |   "empty": null }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-flow-json-node(n,c)`{.legend-1}](#rule-c-flow-json-node)
- [`e-node`{.legend-2}](#rule-e-node)
- [`c-ns-flow-map-adjacent-value(n,c)`{.legend-3}](#rule-c-ns-flow-map-adjacent-value)

A more compact notation is usable inside [flow sequences](#flow-sequences), if the [mapping](#mapping) contains a
*single key/value pair*. This notation does not require the surrounding "`{`{.language-plaintext .highlighter-rouge}"
and "`}`{.language-plaintext .highlighter-rouge}" characters. Note that it is not possible to specify any [node
properties](#node-properties) for the [mapping](#mapping) in this case.

**Example 7.19 Single Pair Flow Mappings**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     [                                                     | :::: {.language-json .highlighter-rouge}
|
|     foo: bar                                              | ::: highlight
|
|     ]                                                     | ``` highlight
|
|                                                           | [ { "foo": "bar" } ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-flow-pair(n,c)`{.legend-1}](#rule-ns-flow-pair)

If the "`?`{.language-plaintext .highlighter-rouge}" indicator is explicitly specified,
[parsing](#parsing-the-presentation-stream) is unambiguous and the syntax is identical to the general case.


``` rule
[150] ns-flow-pair(n,c) ::=
    (
      c-mapping-key     # '?' (not followed by non-ws char)
      s-separate(n,c)
      ns-flow-map-explicit-entry(n,c)
    )
  | ns-flow-pair-entry(n,c)
```

**Example 7.20 Single Pair Explicit Entry**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     [                                                     | :::: {.language-json .highlighter-rouge}
|
|     ? foo                                                 | ::: highlight
|
|      bar : baz                                            | ``` highlight
|
|     ]                                                     | [ { "foo bar": "baz" } ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-flow-map-explicit-entry(n,c)`{.legend-1}](#rule-ns-flow-map-explicit-entry)

If the "`?`{.language-plaintext .highlighter-rouge}" indicator is omitted, [parsing](#parsing-the-presentation-stream)
needs to see past the *implicit key* to recognize it as such. To limit the amount of lookahead required, the
"`:`{.language-plaintext .highlighter-rouge}" indicator must appear at most 1024 Unicode characters beyond the start of
the [key](#nodes). In addition, the [key](#nodes) is restricted to a single line.

Note that YAML allows arbitrary [nodes](#nodes) to be used as [keys](#nodes). In particular, a [key](#nodes) may be a
[sequence](#sequence) or a [mapping](#mapping). Thus, without the above restrictions, practical one-pass
[parsing](#parsing-the-presentation-stream) would have been impossible to implement.


``` rule
[151] ns-flow-pair-entry(n,c) ::=
    ns-flow-pair-yaml-key-entry(n,c)
  | c-ns-flow-map-empty-key-entry(n,c)
  | c-ns-flow-pair-json-key-entry(n,c)
```


``` rule
[152] ns-flow-pair-yaml-key-entry(n,c) ::=
  ns-s-implicit-yaml-key(FLOW-KEY)
  c-ns-flow-map-separate-value(n,c)
```


``` rule
[153] c-ns-flow-pair-json-key-entry(n,c) ::=
  c-s-implicit-json-key(FLOW-KEY)
  c-ns-flow-map-adjacent-value(n,c)
```


``` rule
[154] ns-s-implicit-yaml-key(c) ::=
  ns-flow-yaml-node(0,c)
  s-separate-in-line?
  /* At most 1024 characters altogether */
```


``` rule
[155] c-s-implicit-json-key(c) ::=
  c-flow-json-node(0,c)
  s-separate-in-line?
  /* At most 1024 characters altogether */
```

**Example 7.21 Single Pair Implicit Entries**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     - [ YAML·: separate ]                                 | :::: {.language-json .highlighter-rouge}
|
|     - [ °: empty key entry ]                              | ::: highlight
|
|     - [ {JSON: like}:adjacent ]                           | ``` highlight
|
|                                                           | [ [ { "YAML": "separate" } ],
|
|                                                           |   [ { null: "empty key entry" } ],
|
|                                                           |   [ { { "JSON": "like" }: "adjacent" } ] ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-s-implicit-yaml-key`{.legend-1}](#rule-ns-s-implicit-yaml-key)
- [`e-node`{.legend-2}](#rule-e-node)
- [`c-s-implicit-json-key`{.legend-3}](#rule-c-s-implicit-json-key)
- `Value`{.legend-4}

**Example 7.22 Invalid Implicit Keys**

+-----------------------------------------------------------+-----------------------------------------------------------
+
| ``` error                                                 | ``` error
|
| [ foo                                                     | ERROR:
|
|  bar: invalid,                                            | - The foo bar key spans multiple lines
|
|  "foo_...>1K characters..._bar": invalid ]                | - The foo...bar key is too long
|
| ```                                                       | ```
|
+-----------------------------------------------------------+-----------------------------------------------------------
+


## 7.5. Flow Nodes {#75-flow-nodes}

*JSON-like* [flow styles](#flow-style-productions) all have explicit start and end [indicators](#indicator-characters).
The only [flow style](#flow-style-productions) that does not have this property is the [plain scalar](#plain-style).
Note that none of the "JSON-like" styles is actually acceptable by JSON. Even the [double-quoted
style](#double-quoted-style) is a superset of the JSON string format.


``` rule
[156] ns-flow-yaml-content(n,c) ::=
  ns-plain(n,c)
```


``` rule
[157] c-flow-json-content(n,c) ::=
    c-flow-sequence(n,c)
  | c-flow-mapping(n,c)
  | c-single-quoted(n,c)
  | c-double-quoted(n,c)
```


``` rule
[158] ns-flow-content(n,c) ::=
    ns-flow-yaml-content(n,c)
  | c-flow-json-content(n,c)
```

**Example 7.23 Flow Content**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     - [ a, b ]                                            | :::: {.language-json .highlighter-rouge}
|
|     - { a: b }                                            | ::: highlight
|
|     - "a"                                                 | ``` highlight
|
|     - 'b'                                                 | [ [ "a", "b" ],
|
|     - c                                                   |   { "a": "b" },
|
|                                                           |   "a",
|
|                                                           |   "b",
|
|                                                           |   "c" ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-flow-json-content(n,c)`{.legend-1}](#rule-c-flow-json-content)
- [`ns-flow-yaml-content(n,c)`{.legend-2}](#rule-ns-flow-yaml-content)

A complete [flow](#flow-style-productions) [node](#nodes) also has optional [node properties](#node-properties), except
for [alias nodes](#alias-nodes) which refer to the [anchored](#anchors-and-aliases) [node properties](#node-properties).


``` rule
[159] ns-flow-yaml-node(n,c) ::=
    c-ns-alias-node
  | ns-flow-yaml-content(n,c)
  | (
      c-ns-properties(n,c)
      (
          (
            s-separate(n,c)
            ns-flow-yaml-content(n,c)
          )
        | e-scalar
      )
    )
```


``` rule
[160] c-flow-json-node(n,c) ::=
  (
    c-ns-properties(n,c)
    s-separate(n,c)
  )?
  c-flow-json-content(n,c)
```


``` rule
[161] ns-flow-node(n,c) ::=
    c-ns-alias-node
  | ns-flow-content(n,c)
  | (
      c-ns-properties(n,c)
      (
        (
          s-separate(n,c)
          ns-flow-content(n,c)
        )
        | e-scalar
      )
    )
```

**Example 7.24 Flow Nodes**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     - !!str "a"                                           | :::: {.language-json .highlighter-rouge}
|
|     - 'b'                                                 | ::: highlight
|
|     - &anchor "c"                                         | ``` highlight
|
|     - *anchor                                             | [ "a",
|
|     - !!str°                                              |   "b",
|
|                                                           |   "c",
|
|                                                           |   "c",
|
|                                                           |   "" ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-flow-json-node(n,c)`{.legend-1}](#rule-c-flow-json-node)
- [`ns-flow-yaml-node(n,c)`{.legend-2}](#rule-ns-flow-yaml-node)


# Chapter 8. Block Style Productions {#chapter-8-block-style-productions}

YAML's *block styles* employ [indentation](#indentation-spaces) rather than [indicators](#indicator-characters) to
denote structure. This results in a more human readable (though less compact) notation.


## 8.1. Block Scalar Styles {#81-block-scalar-styles}

YAML provides two *block scalar styles*, [literal](#literal-style) and [folded](#line-folding). Each provides a
different trade-off between readability and expressive power.


### 8.1.1. Block Scalar Headers {#811-block-scalar-headers}

[Block scalars](#block-scalar-styles) are controlled by a few [indicators](#indicator-characters) given in a *header*
preceding the [content](#nodes) itself. This header is followed by a non-content [line break](#line-break-characters)
with an optional [comment](#comments). This is the only case where a [comment](#comments) must not be followed by
additional [comment](#comments) lines.

> Note: See [Production Parameters](#production-parameters) for the definition of the `t`{.language-plaintext
> .highlighter-rouge} variable.


``` rule
[162] c-b-block-header(t) ::=
  (
      (
        c-indentation-indicator
        c-chomping-indicator(t)
      )
    | (
        c-chomping-indicator(t)
        c-indentation-indicator
      )
  )
  s-b-comment
```

**Example 8.1 Block Scalar Header**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     - | # Empty header↓                                   | :::: {.language-json .highlighter-rouge}
|
|      literal                                              | ::: highlight
|
|     - >1 # Indentation indicator↓                         | ``` highlight
|
|      ·folded                                              | [ "literal\n",
|
|     - |+ # Chomping indicator↓                            |   " folded\n",
|
|      keep                                                 |   "keep\n\n",
|
|                                                           |   " strip" ]
|
|     - >1- # Both indicators↓                              | ```
|
|      ·strip                                               | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-b-block-header(t)`{.legend-1}](#rule-c-b-block-header)


#### 8.1.1.1. Block Indentation Indicator {#8111-block-indentation-indicator}

Every block scalar has a *content indentation level*. The content of the block scalar excludes a number of leading
[spaces](#white-space-characters) on each line up to the content indentation level.

If a block scalar has an *indentation indicator*, then the content indentation level of the block scalar is equal to the
indentation level of the block scalar plus the integer value of the indentation indicator character.

If no indentation indicator is given, then the content indentation level is equal to the number of leading
[spaces](#white-space-characters) on the first non-[empty line](#empty-lines) of the contents. If there is no non-[empty
line](#empty-lines) then the content indentation level is equal to the number of spaces on the longest line.

It is an error if any non-[empty line](#empty-lines) does not begin with a number of spaces greater than or equal to the
content indentation level.

It is an error for any of the leading [empty lines](#empty-lines) to contain more [spaces](#white-space-characters) than
the first non-[empty line](#empty-lines).

A YAML [processor](#processes-and-models) should only emit an explicit indentation indicator for cases where detection
will fail.


``` rule
[163] c-indentation-indicator ::=
  [x31-x39]    # 1-9
```

**Example 8.2 Block Indentation Indicator**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     - |°                                                  | :::: {.language-json .highlighter-rouge}
|
|     ·detected                                             | ::: highlight
|
|     - >°                                                  | ``` highlight
|
|     ·                                                     | [ "detected\n",
|
|     ··                                                    |   "\n\n# detected\n",
|
|     ··# detected                                          |   " explicit\n",
|
|     - |1                                                  |   "\t\ndetected\n" ]
|
|     ··explicit                                            | ```
|
|     - >°                                                  | :::
|
|     ·→                                                    | ::::
|
|     ·detected                                             |
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-indentation-indicator`{.legend-1}](#rule-c-indentation-indicator)
- [`s-indent(n)`{.legend-2}](#rule-s-indent)

**Example 8.3 Invalid Block Scalar Indentation Indicators**

+-----------------------------------------------------------+-----------------------------------------------------------
+
| ``` error                                                 | ``` error
|
| - |                                                       | ERROR:
|
| ··                                                        | - A leading all-space line must
|
| ·text                                                     |   not have too many spaces.
|
| - >                                                       | - A following text line must
|
| ··text                                                    |   not be less indented.
|
| ·text                                                     | - The text is less indented
|
| - |2                                                      |   than the indicated level.
|
| ·text                                                     | ```
|
| ```                                                       |
|
+-----------------------------------------------------------+-----------------------------------------------------------
+


#### 8.1.1.2. Block Chomping Indicator {#8112-block-chomping-indicator}

*Chomping* controls how final [line breaks](#line-break-characters) and trailing [empty lines](#empty-lines) are
interpreted. YAML provides three chomping methods:


Strip

:   *Stripping* is specified by the "`-`{.language-plaintext .highlighter-rouge}" chomping indicator. In this case, the
    final [line break](#line-break-characters) and any trailing [empty lines](#empty-lines) are excluded from the
    [scalar's content](#scalar).


Clip

:   *Clipping* is the default behavior used if no explicit chomping indicator is specified. In this case, the final
    [line break](#line-break-characters) character is preserved in the [scalar's content](#scalar). However, any
    trailing [empty lines](#empty-lines) are excluded from the [scalar's content](#scalar).


Keep

:   *Keeping* is specified by the "`+`{.language-plaintext .highlighter-rouge}" chomping indicator. In this case, the
    final [line break](#line-break-characters) and any trailing [empty lines](#empty-lines) are considered to be part of
    the [scalar's content](#scalar). These additional lines are not subject to [folding](#line-folding).

The chomping method used is a [presentation detail](#presenting-the-serialization-tree) and must not be used to convey
[content](#nodes) information.


``` rule
[164]
c-chomping-indicator(STRIP) ::= '-'
c-chomping-indicator(KEEP)  ::= '+'
c-chomping-indicator(CLIP)  ::= ""
```

The interpretation of the final [line break](#line-break-characters) of a [block scalar](#block-scalar-styles) is
controlled by the chomping indicator specified in the [block scalar header](#block-scalar-headers).


``` rule
[165]
b-chomped-last(STRIP) ::= b-non-content  | <end-of-input>
b-chomped-last(CLIP)  ::= b-as-line-feed | <end-of-input>
b-chomped-last(KEEP)  ::= b-as-line-feed | <end-of-input>
```

**Example 8.4 Chomping Final Line Break**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     strip: |-                                             | :::: {.language-json .highlighter-rouge}
|
|       text↓                                               | ::: highlight
|
|     clip: |                                               | ``` highlight
|
|       text↓                                               | { "strip": "text",
|
|     keep: |+                                              |   "clip": "text\n",
|
|       text↓                                               |   "keep": "text\n" }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`b-non-content`{.legend-1}](#rule-b-non-content)
- [`b-as-line-feed`{.legend-2}](#rule-b-as-line-feed)

The interpretation of the trailing [empty lines](#empty-lines) following a [block scalar](#block-scalar-styles) is also
controlled by the chomping indicator specified in the [block scalar header](#block-scalar-headers).


``` rule
[166]
l-chomped-empty(n,STRIP) ::= l-strip-empty(n)
l-chomped-empty(n,CLIP)  ::= l-strip-empty(n)
l-chomped-empty(n,KEEP)  ::= l-keep-empty(n)
```


``` rule
[167] l-strip-empty(n) ::=
  (
    s-indent-less-or-equal(n)
    b-non-content
  )*
  l-trail-comments(n)?
```


``` rule
[168] l-keep-empty(n) ::=
  l-empty(n,BLOCK-IN)*
  l-trail-comments(n)?
```

Explicit [comment](#comments) lines may follow the trailing [empty lines](#empty-lines). To prevent ambiguity, the first
such [comment](#comments) line must be less [indented](#indentation-spaces) than the [block scalar
content](#block-scalar-styles). Additional [comment](#comments) lines, if any, are not so restricted. This is the only
case where the [indentation](#indentation-spaces) of [comment](#comments) lines is constrained.


``` rule
[169] l-trail-comments(n) ::=
  s-indent-less-than(n)
  c-nb-comment-text
  b-comment
  l-comment*
```

**Example 8.5 Chomping Trailing Lines**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     # Strip                                               | :::: {.language-json .highlighter-rouge}
|
|       # Comments:                                         | ::: highlight
|
|     strip: |-                                             | ``` highlight
|
|       # text↓                                             | { "strip": "# text",
|
|     ··⇓                                                   |   "clip": "# text\n",
|
|     ·# Clip                                               |   "keep": "# text\n\n" }
|
|     ··# comments:                                         | ```
|
|     ↓                                                     | :::
|
|     clip: |                                               | ::::
|
|       # text↓                                             |
|
|     ·↓                                                    |
|
|     ·# Keep                                               |
|
|     ··# comments:                                         |
|
|     ↓                                                     |
|
|     keep: |+                                              |
|
|       # text↓                                             |
|
|     ↓                                                     |
|
|     ·# Trail                                              |
|
|     ··# comments.                                         |
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`l-strip-empty(n)`{.legend-1}](#rule-l-strip-empty)
- [`l-keep-empty(n)`{.legend-2}](#rule-l-keep-empty)
- [`l-trail-comments(n)`{.legend-3}](#rule-l-trail-comments)

If a [block scalar](#block-scalar-styles) consists only of [empty lines](#empty-lines), then these lines are considered
as trailing lines and hence are affected by chomping.

**Example 8.6 Empty Scalar Chomping**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     strip: >-                                             | :::: {.language-json .highlighter-rouge}
|
|     ↓                                                     | ::: highlight
|
|     clip: >                                               | ``` highlight
|
|     ↓                                                     | { "strip": "",
|
|     keep: |+                                              |   "clip": "",
|
|     ↓                                                     |   "keep": "\n" }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`l-strip-empty(n)`{.legend-1}](#rule-l-strip-empty)
- [`l-keep-empty(n)`{.legend-2}](#rule-l-keep-empty)


### 8.1.2. Literal Style {#812-literal-style}

The *literal style* is denoted by the "`|`{.language-plaintext .highlighter-rouge}" indicator. It is the simplest, most
restricted and most readable [scalar style](#node-styles).


``` rule
[170] c-l+literal(n) ::=
  c-literal                # '|'
  c-b-block-header(t)
  l-literal-content(n+m,t)
```

**Example 8.7 Literal Scalar**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     |↓                                                    | :::: {.language-json .highlighter-rouge}
|
|     ·literal↓                                             | ::: highlight
|
|     ·→text↓                                               | ``` highlight
|
|     ↓                                                     | "literal\n\ttext\n"
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-l+literal(n)`{.legend-1}](#rule-c-l+literal)

Inside literal scalars, all ([indented](#indentation-spaces)) characters are considered to be [content](#nodes),
including [white space](#white-space-characters) characters. Note that all [line break](#line-break-characters)
characters are [normalized](#line-break-characters). In addition, [empty lines](#empty-lines) are not
[folded](#line-folding), though final [line breaks](#line-break-characters) and trailing [empty lines](#empty-lines) are
[chomped](#block-chomping-indicator).

There is no way to escape characters inside literal scalars. This restricts them to [printable](#character-set)
characters. In addition, there is no way to break a long literal line.


``` rule
[171] l-nb-literal-text(n) ::=
  l-empty(n,BLOCK-IN)*
  s-indent(n) nb-char+
```


``` rule
[172] b-nb-literal-next(n) ::=
  b-as-line-feed
  l-nb-literal-text(n)
```


``` rule
[173] l-literal-content(n,t) ::=
  (
    l-nb-literal-text(n)
    b-nb-literal-next(n)*
    b-chomped-last(t)
  )?
  l-chomped-empty(n,t)
```

**Example 8.8 Literal Content**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     |                                                     | :::: {.language-json .highlighter-rouge}
|
|     ·                                                     | ::: highlight
|
|     ··                                                    | ``` highlight
|
|     ··literal↓                                            | "\n\nliteral\n·\n\ntext\n"
|
|     ···↓                                                  | ```
|
|     ··                                                    | :::
|
|     ··text↓                                               | ::::
|
|     ↓                                                     |
|
|     ·# Comment                                            |
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`l-nb-literal-text(n)`{.legend-1}](#rule-l-nb-literal-text)
- [`b-nb-literal-next(n)`{.legend-2}](#rule-b-nb-literal-next)
- [`b-chomped-last(t)`{.legend-3}](#rule-b-chomped-last)
- [`l-chomped-empty(n,t)`{.legend-4}](#rule-l-chomped-empty)


### 8.1.3. Folded Style {#813-folded-style}

The *folded style* is denoted by the "`>`{.language-plaintext .highlighter-rouge}" indicator. It is similar to the
[literal style](#literal-style); however, folded scalars are subject to [line folding](#line-folding).


``` rule
[174] c-l+folded(n) ::=
  c-folded                 # '>'
  c-b-block-header(t)
  l-folded-content(n+m,t)
```

**Example 8.9 Folded Scalar**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     >↓                                                    | :::: {.language-json .highlighter-rouge}
|
|     ·folded↓                                              | ::: highlight
|
|     ·text↓                                                | ``` highlight
|
|     ↓                                                     | "folded text\n"
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-l+folded(n)`{.legend-1}](#rule-c-l+folded)

[Folding](#line-folding) allows long lines to be broken anywhere a single [space](#white-space-characters) character
separates two non-[space](#white-space-characters) characters.


``` rule
[175] s-nb-folded-text(n) ::=
  s-indent(n)
  ns-char
  nb-char*
```


``` rule
[176] l-nb-folded-lines(n) ::=
  s-nb-folded-text(n)
  (
    b-l-folded(n,BLOCK-IN)
    s-nb-folded-text(n)
  )*
```

**Example 8.10 Folded Lines**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     >                                                     | :::: {.language-json .highlighter-rouge}
|
|                                                           | ::: highlight
|
|     ·folded↓                                              | ``` highlight
|
|     ·line↓                                                | "\nfolded line\nnext line\n  \
|
|     ↓                                                     | * bullet\n \n  * list\n  \
|
|     ·next                                                 | * lines\n\nlast line\n"
|
|     ·line↓                                                | ```
|
|        * bullet                                           | :::
|
|                                                           | ::::
|
|        * list                                             |
|
|        * lines                                            |
|
|                                                           |
|
|     ·last↓                                                |
|
|     ·line↓                                                |
|
|                                                           |
|
|     # Comment                                             |
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`l-nb-folded-lines(n)`{.legend-1}](#rule-l-nb-folded-lines)
- [`s-nb-folded-text(n)`{.legend-2}](#rule-s-nb-folded-text)

(The following three examples duplicate this example, each highlighting different productions.)

Lines starting with [white space](#white-space-characters) characters (*more-indented* lines) are not
[folded](#line-folding).


``` rule
[177] s-nb-spaced-text(n) ::=
  s-indent(n)
  s-white
  nb-char*
```


``` rule
[178] b-l-spaced(n) ::=
  b-as-line-feed
  l-empty(n,BLOCK-IN)*
```


``` rule
[179] l-nb-spaced-lines(n) ::=
  s-nb-spaced-text(n)
  (
    b-l-spaced(n)
    s-nb-spaced-text(n)
  )*
```

**Example 8.11 More Indented Lines**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     >                                                     | :::: {.language-json .highlighter-rouge}
|
|                                                           | ::: highlight
|
|      folded                                               | ``` highlight
|
|      line                                                 | "\nfolded line\nnext line\n  \
|
|                                                           | * bullet\n \n  * list\n  \
|
|      next                                                 | * lines\n\nlast line\n"
|
|      line                                                 | ```
|
|     ···* bullet↓                                          | :::
|
|     ↓                                                     | ::::
|
|     ···* list↓                                            |
|
|     ···* lines↓                                           |
|
|                                                           |
|
|      last                                                 |
|
|      line                                                 |
|
|                                                           |
|
|     # Comment                                             |
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`l-nb-spaced-lines(n)`{.legend-1}](#rule-l-nb-spaced-lines)
- [`s-nb-spaced-text(n)`{.legend-2}](#rule-s-nb-spaced-text)

[Line breaks](#line-break-characters) and [empty lines](#empty-lines) separating folded and more-indented lines are also
not [folded](#line-folding).


``` rule
[180] l-nb-same-lines(n) ::=
  l-empty(n,BLOCK-IN)*
  (
      l-nb-folded-lines(n)
    | l-nb-spaced-lines(n)
  )
```


``` rule
[181] l-nb-diff-lines(n) ::=
  l-nb-same-lines(n)
  (
    b-as-line-feed
    l-nb-same-lines(n)
  )*
```

**Example 8.12 Empty Separation Lines**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     >                                                     | :::: {.language-json .highlighter-rouge}
|
|     ↓                                                     | ::: highlight
|
|      folded                                               | ``` highlight
|
|      line↓                                                | "\nfolded line\nnext line\n  \
|
|     ↓                                                     | * bullet\n \n  * list\n  \
|
|      next                                                 | * lines\n\nlast line\n"
|
|      line↓                                                | ```
|
|        * bullet                                           | :::
|
|                                                           | ::::
|
|        * list                                             |
|
|        * lines↓                                           |
|
|     ↓                                                     |
|
|      last                                                 |
|
|      line                                                 |
|
|                                                           |
|
|     # Comment                                             |
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`b-as-line-feed`{.legend-1}](#rule-b-as-line-feed)
- `(separation) `{.legend-2}[`l-empty(n,c)`{.legend-2}](#rule-l-empty)

The final [line break](#line-break-characters) and trailing [empty lines](#empty-lines) if any, are subject to
[chomping](#block-chomping-indicator) and are never [folded](#line-folding).


``` rule
[182] l-folded-content(n,t) ::=
  (
    l-nb-diff-lines(n)
    b-chomped-last(t)
  )?
  l-chomped-empty(n,t)
```

**Example 8.13 Final Empty Lines**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     >                                                     | :::: {.language-json .highlighter-rouge}
|
|                                                           | ::: highlight
|
|      folded                                               | ``` highlight
|
|      line                                                 | "\nfolded line\nnext line\n  \
|
|                                                           | * bullet\n \n  * list\n  \
|
|      next                                                 | * lines\n\nlast line\n"
|
|      line                                                 | ```
|
|        * bullet                                           | :::
|
|                                                           | ::::
|
|        * list                                             |
|
|        * lines                                            |
|
|                                                           |
|
|      last                                                 |
|
|      line↓                                                |
|
|     ↓                                                     |
|
|     # Comment                                             |
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`b-chomped-last(t)`{.legend-1}](#rule-b-chomped-last)
- [`l-chomped-empty(n,t)`{.legend-2}](#rule-l-chomped-empty)


## 8.2. Block Collection Styles {#82-block-collection-styles}

For readability, *block collections styles* are not denoted by any [indicator](#indicator-characters). Instead, YAML
uses a lookahead method, where a block collection is distinguished from a [plain scalar](#plain-style) only when a
[key/value pair](#mapping) or a [sequence entry](#block-sequences) is seen.


### 8.2.1. Block Sequences {#821-block-sequences}

A *block sequence* is simply a series of [nodes](#nodes), each denoted by a leading "`-`{.language-plaintext
.highlighter-rouge}" indicator. The "`-`{.language-plaintext .highlighter-rouge}" indicator must be
[separated](#separation-spaces) from the [node](#nodes) by [white space](#white-space-characters). This allows
"`-`{.language-plaintext .highlighter-rouge}" to be used as the first character in a [plain scalar](#plain-style) if
followed by a non-space character (e.g. "`-42`{.language-plaintext .highlighter-rouge}").


``` rule
[183] l+block-sequence(n) ::=
  (
    s-indent(n+1+m)
    c-l-block-seq-entry(n+1+m)
  )+
```


``` rule
[184] c-l-block-seq-entry(n) ::=
  c-sequence-entry    # '-'
  [ lookahead ≠ ns-char ]
  s-l+block-indented(n,BLOCK-IN)
```

**Example 8.14 Block Sequence**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     block sequence:                                       | :::: {.language-json .highlighter-rouge}
|
|     ··- one↓                                              | ::: highlight
|
|       - two : three↓                                      | ``` highlight
|
|                                                           | { "block sequence": [
|
|                                                           |     "one",
|
|                                                           |     { "two": "three" } ] }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-l-block-seq-entry(n)`{.legend-1}](#rule-c-l-block-seq-entry)
- `auto-detected `{.legend-2}[`s-indent(n)`{.legend-2}](#rule-s-indent)

The entry [node](#nodes) may be either [completely empty](#example-empty-content), be a nested [block
node](#block-nodes) or use a *compact in-line notation*. The compact notation may be used when the entry is itself a
nested [block collection](#block-collection-styles). In this case, both the "`-`{.language-plaintext
.highlighter-rouge}" indicator and the following [spaces](#white-space-characters) are considered to be part of the
[indentation](#indentation-spaces) of the nested [collection](#collections). Note that it is not possible to specify
[node properties](#node-properties) for such a [collection](#collections).


``` rule
[185] s-l+block-indented(n,c) ::=
    (
      s-indent(m)
      (
          ns-l-compact-sequence(n+1+m)
        | ns-l-compact-mapping(n+1+m)
      )
    )
  | s-l+block-node(n,c)
  | (
      e-node    # ""
      s-l-comments
    )
```


``` rule
[186] ns-l-compact-sequence(n) ::=
  c-l-block-seq-entry(n)
  (
    s-indent(n)
    c-l-block-seq-entry(n)
  )*
```

**Example 8.15 Block Sequence Entry Types**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     -° # Empty                                            | :::: {.language-json .highlighter-rouge}
|
|     - |                                                   | ::: highlight
|
|      block node                                           | ``` highlight
|
|     -·- one # Compact                                     | [ null,
|
|     ··- two # sequence                                    |   "block node\n",
|
|     - one: two # Compact mapping                          |   [ "one", "two" ],
|
|                                                           |   { "one": "two" } ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- `Empty`{.legend-1}
- [`s-l+block-node(n,c)`{.legend-2}](#rule-s-l+block-node)
- [`ns-l-compact-sequence(n)`{.legend-3}](#rule-ns-l-compact-sequence)
- [`ns-l-compact-mapping(n)`{.legend-4}](#rule-ns-l-compact-mapping)


### 8.2.2. Block Mappings {#822-block-mappings}

A *Block mapping* is a series of entries, each [presenting](#presenting-the-serialization-tree) a [key/value
pair](#mapping).


``` rule
[187] l+block-mapping(n) ::=
  (
    s-indent(n+1+m)
    ns-l-block-map-entry(n+1+m)
  )+
```

**Example 8.16 Block Mappings**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     block mapping:                                        | :::: {.language-json .highlighter-rouge}
|
|     ·key: value↓                                          | ::: highlight
|
|                                                           | ``` highlight
|
|                                                           | { "block mapping": {
|
|                                                           |     "key": "value" } }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-l-block-map-entry(n)`{.legend-1}](#rule-ns-l-block-map-entry)
- `auto-detected `{.legend-2}[`s-indent(n)`{.legend-2}](#rule-s-indent)

If the "`?`{.language-plaintext .highlighter-rouge}" indicator is specified, the optional value node must be specified
on a separate line, denoted by the "`:`{.language-plaintext .highlighter-rouge}" indicator. Note that YAML allows here
the same [compact in-line notation](#example-block-sequence) described above for [block sequence](#block-sequences)
entries.


``` rule
[188] ns-l-block-map-entry(n) ::=
    c-l-block-map-explicit-entry(n)
  | ns-l-block-map-implicit-entry(n)
```


``` rule
[189] c-l-block-map-explicit-entry(n) ::=
  c-l-block-map-explicit-key(n)
  (
      l-block-map-explicit-value(n)
    | e-node                        # ""
  )
```


``` rule
[190] c-l-block-map-explicit-key(n) ::=
  c-mapping-key                     # '?' (not followed by non-ws char)
  s-l+block-indented(n,BLOCK-OUT)
```


``` rule
[191] l-block-map-explicit-value(n) ::=
  s-indent(n)
  c-mapping-value                   # ':' (not followed by non-ws char)
  s-l+block-indented(n,BLOCK-OUT)
```

**Example 8.17 Explicit Block Mapping Entries**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     ? explicit key # Empty value↓°                        | :::: {.language-json .highlighter-rouge}
|
|     ? |                                                   | ::: highlight
|
|       block key↓                                          | ``` highlight
|
|     :·- one # Explicit compact                            | { "explicit key": null,
|
|     ··- two # block value↓                                |   "block key\n": [
|
|                                                           |     "one",
|
|                                                           |     "two" ] }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-l-block-map-explicit-key(n)`{.legend-1}](#rule-c-l-block-map-explicit-key)
- [`l-block-map-explicit-value(n)`{.legend-2}](#rule-l-block-map-explicit-value)
- [`e-node`{.legend-3}](#rule-e-node)

If the "`?`{.language-plaintext .highlighter-rouge}" indicator is omitted, [parsing](#parsing-the-presentation-stream)
needs to see past the [implicit key](#example-single-pair-explicit-entry), in the same way as in the [single key/value
pair](#mapping) [flow mapping](#flow-mappings). Hence, such [keys](#nodes) are subject to the same restrictions; they
are limited to a single line and must not span more than 1024 Unicode characters.


``` rule
[192] ns-l-block-map-implicit-entry(n) ::=
  (
      ns-s-block-map-implicit-key
    | e-node    # ""
  )
  c-l-block-map-implicit-value(n)
```


``` rule
[193] ns-s-block-map-implicit-key ::=
    c-s-implicit-json-key(BLOCK-KEY)
  | ns-s-implicit-yaml-key(BLOCK-KEY)
```

In this case, the [value](#nodes) may be specified on the same line as the [implicit
key](#example-single-pair-explicit-entry). Note however that in block mappings the [value](#nodes) must never be
adjacent to the "`:`{.language-plaintext .highlighter-rouge}", as this greatly reduces readability and is not required
for [JSON compatibility](#yaml-directives) (unlike the case in [flow mappings](#flow-mappings)).

There is no compact notation for in-line [values](#nodes). Also, while both the [implicit
key](#example-single-pair-explicit-entry) and the [value](#nodes) following it may be empty, the
"`:`{.language-plaintext .highlighter-rouge}" indicator is mandatory. This prevents a potential ambiguity with
multi-line [plain scalars](#plain-style).


``` rule
[194] c-l-block-map-implicit-value(n) ::=
  c-mapping-value           # ':' (not followed by non-ws char)
  (
      s-l+block-node(n,BLOCK-OUT)
    | (
        e-node    # ""
        s-l-comments
      )
  )
```

**Example 8.18 Implicit Block Mapping Entries**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     plain key: in-line value                              | :::: {.language-json .highlighter-rouge}
|
|     °:° # Both empty                                      | ::: highlight
|
|     "quoted key":                                         | ``` highlight
|
|     - entry                                               | { "plain key": "in-line value",
|
|                                                           |   null: null,
|
|                                                           |   "quoted key": [ "entry" ] }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-s-block-map-implicit-key`{.legend-1}](#rule-ns-s-block-map-implicit-key)
- [`c-l-block-map-implicit-value(n)`{.legend-2}](#rule-c-l-block-map-implicit-value)

A [compact in-line notation](#example-block-sequence) is also available. This compact notation may be nested inside
[block sequences](#block-sequences) and explicit block mapping entries. Note that it is not possible to specify [node
properties](#node-properties) for such a nested mapping.


``` rule
[195] ns-l-compact-mapping(n) ::=
  ns-l-block-map-entry(n)
  (
    s-indent(n)
    ns-l-block-map-entry(n)
  )*
```

**Example 8.19 Compact Block Mappings**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     - sun: yellow↓                                        | :::: {.language-json .highlighter-rouge}
|
|     - ? earth: blue↓                                      | ::: highlight
|
|       : moon: white↓                                      | ``` highlight
|
|                                                           | [ { "sun": "yellow" },
|
|                                                           |   { { "earth": "blue" }:
|
|                                                           |       { "moon": "white" } } ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`ns-l-compact-mapping(n)`{.legend-1}](#rule-ns-l-compact-mapping)


### 8.2.3. Block Nodes {#823-block-nodes}

YAML allows [flow nodes](#flow-nodes) to be embedded inside [block collections](#block-collection-styles) (but not
vice-versa). [Flow nodes](#flow-nodes) must be [indented](#indentation-spaces) by at least one more
[space](#white-space-characters) than the parent [block collection](#block-collection-styles). Note that [flow
nodes](#flow-nodes) may begin on a following line.

It is at this point that [parsing](#parsing-the-presentation-stream) needs to distinguish between a [plain
scalar](#plain-style) and an [implicit key](#example-single-pair-explicit-entry) starting a nested [block
mapping](#block-mappings).


``` rule
[196] s-l+block-node(n,c) ::=
    s-l+block-in-block(n,c)
  | s-l+flow-in-block(n)
```


``` rule
[197] s-l+flow-in-block(n) ::=
  s-separate(n+1,FLOW-OUT)
  ns-flow-node(n+1,FLOW-OUT)
  s-l-comments
```

**Example 8.20 Block Node Types**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     -↓                                                    | :::: {.language-json .highlighter-rouge}
|
|     ··"flow in block"↓                                    | ::: highlight
|
|     -·>                                                   | ``` highlight
|
|      Block scalar↓                                        | [ "flow in block",
|
|     -·!!map # Block collection                            |   "Block scalar\n",
|
|       foo : bar↓                                          |   { "foo": "bar" } ]
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`s-l+flow-in-block(n)`{.legend-1}](#rule-s-l+flow-in-block)
- [`s-l+block-in-block(n,c)`{.legend-2}](#rule-s-l+block-in-block)

The block [node's properties](#node-properties) may span across several lines. In this case, they must be
[indented](#indentation-spaces) by at least one more [space](#white-space-characters) than the [block
collection](#block-collection-styles), regardless of the [indentation](#indentation-spaces) of the [block
collection](#block-collection-styles) entries.


``` rule
[198] s-l+block-in-block(n,c) ::=
    s-l+block-scalar(n,c)
  | s-l+block-collection(n,c)
```


``` rule
[199] s-l+block-scalar(n,c) ::=
  s-separate(n+1,c)
  (
    c-ns-properties(n+1,c)
    s-separate(n+1,c)
  )?
  (
      c-l+literal(n)
    | c-l+folded(n)
  )
```

**Example 8.21 Block Scalar Nodes**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     literal: |2                                           | :::: {.language-json .highlighter-rouge}
|
|     ··value                                               | ::: highlight
|
|     folded:↓                                              | ``` highlight
|
|     ···!foo                                               | { "literal": "value",
|
|     ··>1                                                  |   "folded": !<!foo> "value" }
|
|     ·value                                                | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-l+literal(n)`{.legend-1}](#rule-c-l+literal)
- [`c-l+folded(n)`{.legend-2}](#rule-c-l+folded)

Since people perceive the "`-`{.language-plaintext .highlighter-rouge}" indicator as [indentation](#indentation-spaces),
nested [block sequences](#block-sequences) may be [indented](#indentation-spaces) by one less
[space](#white-space-characters) to compensate, except, of course, if nested inside another [block
sequence](#block-sequences) (\[`BLOCK-OUT`{.language-plaintext .highlighter-rouge} context\] versus
\[`BLOCK-IN`{.language-plaintext .highlighter-rouge} context\]).


``` rule
[200] s-l+block-collection(n,c) ::=
  (
    s-separate(n+1,c)
    c-ns-properties(n+1,c)
  )?
  s-l-comments
  (
      seq-space(n,c)
    | l+block-mapping(n)
  )
```


``` rule
[201] seq-space(n,BLOCK-OUT) ::= l+block-sequence(n-1)
    seq-space(n,BLOCK-IN)  ::= l+block-sequence(n)
```

**Example 8.22 Block Collection Nodes**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     sequence: !!seq                                       | :::: {.language-json .highlighter-rouge}
|
|     - entry                                               | ::: highlight
|
|     - !!seq                                               | ``` highlight
|
|      - nested                                             | { "sequence": [
|
|     mapping: !!map                                        |     "entry",
|
|      foo: bar                                             |     [ "nested" ] ],
|
|                                                           |   "mapping": { "foo": "bar" } }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`s-l+block-collection(n,c)`{.legend-1}](#rule-s-l+block-collection)
- [`l+block-sequence(n)`{.legend-2}](#rule-l+block-sequence)
- [`l+block-mapping(n)`{.legend-3}](#rule-l+block-mapping)


# Chapter 9. Document Stream Productions {#chapter-9-document-stream-productions}


## 9.1. Documents {#91-documents}

A YAML character [stream](#streams) may contain several *documents*. Each document is completely independent from the
rest.


### 9.1.1. Document Prefix {#911-document-prefix}

A document may be preceded by a *prefix* specifying the [character encoding](#character-encodings) and optional
[comment](#comments) lines. Note that all [documents](#documents) in a stream must use the same [character
encoding](#character-encodings). However it is valid to re-specify the [encoding](#character-encodings) using a [byte
order mark](#character-encodings) for each [document](#documents) in the stream.

The existence of the optional prefix does not necessarily indicate the existence of an actual [document](#documents).


``` rule
[202] l-document-prefix ::=
  c-byte-order-mark?
  l-comment*
```

**Example 9.1 Document Prefix**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     ⇔# Comment                                            | :::: {.language-json .highlighter-rouge}
|
|     # lines                                               | ::: highlight
|
|     Document                                              | ``` highlight
|
|                                                           | "Document"
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`l-document-prefix`{.legend-1}](#rule-l-document-prefix)


### 9.1.2. Document Markers {#912-document-markers}

Using [directives](#directives) creates a potential ambiguity. It is valid to have a "`%`{.language-plaintext
.highlighter-rouge}" character at the start of a line (e.g. as the first character of the second line of a [plain
scalar](#plain-style)). How, then, to distinguish between an actual [directive](#directives) and a [content](#nodes)
line that happens to start with a "`%`{.language-plaintext .highlighter-rouge}" character?

The solution is the use of two special *marker* lines to control the processing of [directives](#directives), one at the
start of a [document](#documents) and one at the end.

At the start of a [document](#documents), lines beginning with a "`%`{.language-plaintext .highlighter-rouge}" character
are assumed to be [directives](#directives). The (possibly empty) list of [directives](#directives) is terminated by a
*directives end marker* line. Lines following this marker can safely use "`%`{.language-plaintext .highlighter-rouge}"
as the first character.

At the end of a [document](#documents), a *document end marker* line is used to signal the
[parser](#parsing-the-presentation-stream) to begin scanning for [directives](#directives) again.

The existence of this optional *document suffix* does not necessarily indicate the existence of an actual following
[document](#documents).

Obviously, the actual [content](#nodes) lines are therefore forbidden to begin with either of these markers.


``` rule
[203] c-directives-end ::= "---"
```


``` rule
[204] c-document-end ::=
  "..."    # (not followed by non-ws char)
```


``` rule
[205] l-document-suffix ::=
  c-document-end
  s-l-comments
```


``` rule
[206] c-forbidden ::=
  <start-of-line>
  (
      c-directives-end
    | c-document-end
  )
  (
      b-char
    | s-white
    | <end-of-input>
  )
```

**Example 9.2 Document Markers**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     %YAML 1.2                                             | :::: {.language-json .highlighter-rouge}
|
|     ---                                                   | ::: highlight
|
|     Document                                              | ``` highlight
|
|     ... # Suffix                                          | "Document"
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`c-directives-end`{.legend-1}](#rule-c-directives-end)
- [`l-document-suffix`{.legend-2}](#rule-l-document-suffix)
- [`c-document-end`{.legend-3}](#rule-c-document-end)


### 9.1.3. Bare Documents {#913-bare-documents}

A *bare document* does not begin with any [directives](#directives) or [marker](#document-markers) lines. Such documents
are very "clean" as they contain nothing other than the [content](#nodes). In this case, the first non-comment line may
not start with a "`%`{.language-plaintext .highlighter-rouge}" first character.

Document [nodes](#nodes) are [indented](#indentation-spaces) as if they have a parent [indented](#indentation-spaces) at
-1 [spaces](#white-space-characters). Since a [node](#nodes) must be more [indented](#indentation-spaces) than its
parent [node](#nodes), this allows the document's [node](#nodes) to be [indented](#indentation-spaces) at zero or more
[spaces](#white-space-characters).


``` rule
[207] l-bare-document ::=
  s-l+block-node(-1,BLOCK-IN)
  /* Excluding c-forbidden content */
```

**Example 9.3 Bare Documents**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     Bare                                                  | :::: {.language-json .highlighter-rouge}
|
|     document                                              | ::: highlight
|
|     ...                                                   | ``` highlight
|
|     # No document                                         | "Bare document"
|
|     ...                                                   | ---
|
|     |                                                     | "%!PS-Adobe-2.0\n"
|
|     %!PS-Adobe-2.0 # Not the first line                   | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`l-bare-document`{.legend-1}](#rule-l-bare-document)


### 9.1.4. Explicit Documents {#914-explicit-documents}

An *explicit document* begins with an explicit [directives end marker](#document-markers) line but no
[directives](#directives). Since the existence of the [document](#documents) is indicated by this
[marker](#document-markers), the [document](#documents) itself may be [completely empty](#example-empty-content).


``` rule
[208] l-explicit-document ::=
  c-directives-end
  (
      l-bare-document
    | (
        e-node    # ""
        s-l-comments
      )
  )
```

**Example 9.4 Explicit Documents**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     ---                                                   | :::: {.language-json .highlighter-rouge}
|
|     { matches                                             | ::: highlight
|
|     % : 20 }                                              | ``` highlight
|
|     ...                                                   | { "matches %": 20 }
|
|     ---                                                   | ---
|
|     # Empty                                               | null
|
|     ...                                                   | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`l-explicit-document`{.legend-1}](#rule-l-explicit-document)


### 9.1.5. Directives Documents {#915-directives-documents}

A *directives document* begins with some [directives](#directives) followed by an explicit [directives end
marker](#document-markers) line.


``` rule
[209] l-directive-document ::=
  l-directive+
  l-explicit-document
```

**Example 9.5 Directives Documents**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     %YAML 1.2                                             | :::: {.language-json .highlighter-rouge}
|
|     --- |                                                 | ::: highlight
|
|     %!PS-Adobe-2.0                                        | ``` highlight
|
|     ...                                                   | "%!PS-Adobe-2.0\n"
|
|     %YAML 1.2                                             | ---
|
|     ---                                                   | null
|
|     # Empty                                               | ```
|
|     ...                                                   | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`l-explicit-document`{.legend-1}](#rule-l-explicit-document)


## 9.2. Streams {#92-streams}

A YAML *stream* consists of zero or more [documents](#documents). Subsequent [documents](#documents) require some sort
of separation [marker](#document-markers) line. If a [document](#documents) is not terminated by a [document end
marker](#document-markers) line, then the following [document](#documents) must begin with a [directives end
marker](#document-markers) line.


``` rule
[210] l-any-document ::=
    l-directive-document
  | l-explicit-document
  | l-bare-document
```


``` rule
[211] l-yaml-stream ::=
  l-document-prefix*
  l-any-document?
  (
      (
        l-document-suffix+
        l-document-prefix*
        l-any-document?
      )
    | c-byte-order-mark
    | l-comment
    | l-explicit-document
  )*
```

**Example 9.6 Stream**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     Document                                              | :::: {.language-json .highlighter-rouge}
|
|     ---                                                   | ::: highlight
|
|     # Empty                                               | ``` highlight
|
|     ...                                                   | "Document"
|
|     %YAML 1.2                                             | ---
|
|     ---                                                   | null
|
|     matches %: 20                                         | ---
|
|                                                           | { "matches %": 20 }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+

::: legend
**Legend:**

- [`l-any-document`{.legend-1}](#rule-l-any-document)
- [`l-document-suffix`{.legend-2}](#rule-l-document-suffix)
- [`l-explicit-document`{.legend-3}](#rule-l-explicit-document)

A sequence of bytes is a *well-formed stream* if, taken as a whole, it complies with the above
`l-yaml-stream`{.language-plaintext .highlighter-rouge} production.


# Chapter 10. Recommended Schemas {#chapter-10-recommended-schemas}

A YAML *schema* is a combination of a set of [tags](#tags) and a mechanism for [resolving](#resolved-tags) [non-specific
tags](#resolved-tags).


## 10.1. Failsafe Schema {#101-failsafe-schema}

The *failsafe schema* is guaranteed to work with any YAML [document](#documents). It is therefore the recommended
[schema](#recommended-schemas) for generic YAML tools. A YAML [processor](#processes-and-models) should therefore
support this [schema](#recommended-schemas), at least as an option.


### 10.1.1. Tags {#1011-tags}


#### 10.1.1.1. Generic Mapping {#10111-generic-mapping}


URI

:   `tag:yaml.org,2002:map`{.language-plaintext .highlighter-rouge}


Kind

:   [Mapping](#mapping).


Definition

:   [Represents](#representation-graph) an associative container, where each [key](#nodes) is unique in the association
    and mapped to exactly one [value](#nodes). YAML places no restrictions on the type of [keys](#nodes); in particular,
    they are not restricted to being [scalars](#scalars). Example [bindings](#constructing-native-data-structures) to
    [native](#representing-native-data-structures) types include Perl's hash, Python's dictionary and Java's Hashtable.

**Example 10.1 `!!map`{.language-plaintext .highlighter-rouge} Examples**

``` example
Block style: !!map
  Clark : Evans
  Ingy  : döt Net
  Oren  : Ben-Kiki

Flow style: !!map { Clark: Evans, Ingy: döt Net, Oren: Ben-Kiki }
```


#### 10.1.1.2. Generic Sequence {#10112-generic-sequence}


URI

:   `tag:yaml.org,2002:seq`{.language-plaintext .highlighter-rouge}


Kind

:   [Sequence](#sequence).


Definition

:   [Represents](#representation-graph) a collection indexed by sequential integers starting with zero. Example
    [bindings](#constructing-native-data-structures) to [native](#representing-native-data-structures) types include
    Perl's array, Python's list or tuple and Java's array or Vector.

**Example 10.2 `!!seq`{.language-plaintext .highlighter-rouge} Examples**

``` example
Block style: !!seq
- Clark Evans
- Ingy döt Net
- Oren Ben-Kiki

Flow style: !!seq [ Clark Evans, Ingy döt Net, Oren Ben-Kiki ]
```


#### 10.1.1.3. Generic String {#10113-generic-string}


URI

:   `tag:yaml.org,2002:str`{.language-plaintext .highlighter-rouge}


Kind

:   [Scalar](#scalar).


Definition

:   [Represents](#representation-graph) a Unicode string, a sequence of zero or more Unicode characters. This type is
    usually [bound](#representing-native-data-structures) to the [native](#representing-native-data-structures)
    language's string type or, for languages lacking one (such as C), to a character array.


Canonical Form:

:   The obvious.

**Example 10.3 `!!str`{.language-plaintext .highlighter-rouge} Examples**

``` example
Block style: !!str |-
  String: just a theory.

Flow style: !!str "String: just a theory."
```


### 10.1.2. Tag Resolution {#1012-tag-resolution}

All [nodes](#nodes) with the "`!`{.language-plaintext .highlighter-rouge}" non-specific tag are
[resolved](#resolved-tags), by the standard [convention](#resolved-tags), to
"`tag:yaml.org,2002:seq`{.language-plaintext .highlighter-rouge}", "`tag:yaml.org,2002:map`{.language-plaintext
.highlighter-rouge}" or "`tag:yaml.org,2002:str`{.language-plaintext .highlighter-rouge}", according to their
[kind](#nodes).

All [nodes](#nodes) with the "`?`{.language-plaintext .highlighter-rouge}" non-specific tag are left
[unresolved](#resolved-tags). This constrains the [application](#processes-and-models) to deal with a [partial
representation](#loading-failure-points).


## 10.2. JSON Schema {#102-json-schema}

The *JSON schema* is the lowest common denominator of most modern computer languages and allows
[parsing](#parsing-the-presentation-stream) JSON files. A YAML [processor](#processes-and-models) should therefore
support this [schema](#recommended-schemas), at least as an option. It is also strongly recommended that other
[schemas](#recommended-schemas) should be based on it.


### 10.2.1. Tags {#1021-tags}

The JSON [schema](#recommended-schemas) uses the following [tags](#tags) in addition to those defined by the
[failsafe](#failsafe-schema) schema:


#### 10.2.1.1. Null {#10211-null}


URI

:   `tag:yaml.org,2002:null`{.language-plaintext .highlighter-rouge}


Kind

:   [Scalar](#scalar).


Definition

:   [Represents](#representation-graph) the lack of a value. This is typically
    [bound](#representing-native-data-structures) to a [native](#representing-native-data-structures) null-like value
    (e.g., `undef`{.language-plaintext .highlighter-rouge} in Perl, `None`{.language-plaintext .highlighter-rouge} in
    Python). Note that a null is different from an empty string. Also, a [mapping](#mapping) entry with some
    [key](#nodes) and a null [value](#nodes) is valid and different from not having that [key](#nodes) in the
    [mapping](#mapping).


Canonical Form

:   `null`{.language-plaintext .highlighter-rouge}.

**Example 10.4 `!!null`{.language-plaintext .highlighter-rouge} Examples**

``` example
!!null null: value for null key
key with null value: !!null null
```


#### 10.2.1.2. Boolean {#10212-boolean}


URI

:   `tag:yaml.org,2002:bool`{.language-plaintext .highlighter-rouge}


Kind

:   [Scalar](#scalar).


Definition

:   [Represents](#representation-graph) a true/false value. In languages without a
    [native](#representing-native-data-structures) Boolean type (such as C), they are usually
    [bound](#representing-native-data-structures) to a native integer type, using one for true and zero for false.


Canonical Form

:   Either `true`{.language-plaintext .highlighter-rouge} or `false`{.language-plaintext .highlighter-rouge}.

**Example 10.5 `!!bool`{.language-plaintext .highlighter-rouge} Examples**

``` example
YAML is a superset of JSON: !!bool true
Pluto is a planet: !!bool false
```


#### 10.2.1.3. Integer {#10213-integer}


URI

:   `tag:yaml.org,2002:int`{.language-plaintext .highlighter-rouge}


Kind

:   [Scalar](#scalar).


Definition

:   [Represents](#representation-graph) arbitrary sized finite mathematical integers. Scalars of this type should be
    [bound](#representing-native-data-structures) to a [native](#representing-native-data-structures) integer data type,
    if possible.

:   Some languages (such as Perl) provide only a "number" type that allows for both integer and floating-point values. A
    YAML [processor](#processes-and-models) may use such a type for integers as long as they round-trip properly.

:   In some languages (such as C), an integer may overflow the [native](#representing-native-data-structures) type's
    storage capability. A YAML [processor](#processes-and-models) may reject such a value as an error, truncate it with
    a warning or find some other manner to round-trip it. In general, integers representable using 32 binary digits
    should safely round-trip through most systems.


Canonical Form

:   Decimal integer notation, with a leading "`-`{.language-plaintext .highlighter-rouge}" character for negative
    values, matching the regular expression `0 | -? [1-9] [0-9]*`{.language-plaintext .highlighter-rouge}

**Example 10.6 `!!int`{.language-plaintext .highlighter-rouge} Examples**

``` example
negative: !!int -12
zero: !!int 0
positive: !!int 34
```


#### 10.2.1.4. Floating Point {#10214-floating-point}


URI

:   `tag:yaml.org,2002:float`{.language-plaintext .highlighter-rouge}


Kind

:   [Scalar](#scalar).


Definition

:   [Represents](#representation-graph) an approximation to real numbers, including three special values (positive and
    negative infinity and "not a number").

:   Some languages (such as Perl) provide only a "number" type that allows for both integer and floating-point values. A
    YAML [processor](#processes-and-models) may use such a type for floating-point numbers, as long as they round-trip
    properly.

:   Not all floating-point values can be stored exactly in any given [native](#representing-native-data-structures)
    type. Hence a float value may change by "a small amount" when round-tripped. The supported range and accuracy
    depends on the implementation, though 32 bit IEEE floats should be safe. Since YAML does not specify a particular
    accuracy, using floating-point [mapping keys](#nodes) requires great care and is not recommended.


Canonical Form

:   Either `0`{.language-plaintext .highlighter-rouge}, `.inf`{.language-plaintext .highlighter-rouge},
    `-.inf`{.language-plaintext .highlighter-rouge}, `.nan`{.language-plaintext .highlighter-rouge} or scientific
    notation matching the regular expression\
    `-? [1-9] ( \. [0-9]* [1-9] )? ( e [-+] [1-9] [0-9]* )?`{.language-plaintext .highlighter-rouge}.

**Example 10.7 `!!float`{.language-plaintext .highlighter-rouge} Examples**

``` example
negative: !!float -1
zero: !!float 0
positive: !!float 2.3e4
infinity: !!float .inf
not a number: !!float .nan
```


### 10.2.2. Tag Resolution {#1022-tag-resolution}

The [JSON schema](#json-schema) [tag resolution](#tag-resolution) is an extension of the [failsafe
schema](#failsafe-schema) [tag resolution](#tag-resolution).

All [nodes](#nodes) with the "`!`{.language-plaintext .highlighter-rouge}" non-specific tag are
[resolved](#resolved-tags), by the standard [convention](#resolved-tags), to
"`tag:yaml.org,2002:seq`{.language-plaintext .highlighter-rouge}", "`tag:yaml.org,2002:map`{.language-plaintext
.highlighter-rouge}" or "`tag:yaml.org,2002:str`{.language-plaintext .highlighter-rouge}", according to their
[kind](#nodes).

[Collections](#collections) with the "`?`{.language-plaintext .highlighter-rouge}" non-specific tag (that is,
[untagged](#resolved-tags) [collections](#collections)) are [resolved](#resolved-tags) to
"`tag:yaml.org,2002:seq`{.language-plaintext .highlighter-rouge}" or "`tag:yaml.org,2002:map`{.language-plaintext
.highlighter-rouge}" according to their [kind](#nodes).

[Scalars](#scalars) with the "`?`{.language-plaintext .highlighter-rouge}" non-specific tag (that is, [plain
scalars](#plain-style)) are matched with a list of regular expressions (first match wins, e.g. `0`{.language-plaintext
.highlighter-rouge} is resolved as `!!int`{.language-plaintext .highlighter-rouge}). In principle, JSON files should not
contain any [scalars](#scalars) that do not match at least one of these. Hence the YAML
[processor](#processes-and-models) should consider them to be an error.

  Regular expression                                                                                        Resolved to
tag
  --------------------------------------------------------------------------------------------------------- ------------
-------------
  `null`{.language-plaintext .highlighter-rouge}                                                            tag:yaml.org
,2002:null
  `true | false`{.language-plaintext .highlighter-rouge}                                                    tag:yaml.org
,2002:bool
  `-? ( 0 | [1-9] [0-9]* )`{.language-plaintext .highlighter-rouge}                                         tag:yaml.org
,2002:int
  `-? ( 0 | [1-9] [0-9]* ) ( \. [0-9]* )? ( [eE] [-+]? [0-9]+ )?`{.language-plaintext .highlighter-rouge}   tag:yaml.org
,2002:float
  `*`{.language-plaintext .highlighter-rouge}                                                               Error

> Note: The regular expression for `float`{.language-plaintext .highlighter-rouge} does not exactly match the one in the
> JSON specification, where at least one digit is required after the dot: `( \. [0-9]+ )`{.language-plaintext
> .highlighter-rouge}. The YAML 1.2 specification intended to match JSON behavior, but this cannot be addressed in the
> 1.2.2 specification.

**Example 10.8 JSON Tag Resolution**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     A null: null                                          | :::: {.language-json .highlighter-rouge}
|
|     Booleans: [ true, false ]                             | ::: highlight
|
|     Integers: [ 0, -0, 3, -19 ]                           | ``` highlight
|
|     Floats: [ 0., -0.0, 12e03, -2E+05 ]                   | { "A null": null,
|
|     Invalid: [ True, Null,                                |   "Booleans": [ true, false ],
|
|       0o7, 0x3A, +12.3 ]                                  |   "Integers": [ 0, 0, 3, -19 ],
|
|                                                           |   "Floats": [ 0.0, -0.0, 12000, -200000 ],
|
|                                                           |   "Invalid": [ "True", "Null",
|
|                                                           |     "0o7", "0x3A", "+12.3" ] }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+


## 10.3. Core Schema {#103-core-schema}

The *Core schema* is an extension of the [JSON schema](#json-schema), allowing for more human-readable
[presentation](#presentation-stream) of the same types. This is the recommended default [schema](#recommended-schemas)
that YAML [processor](#processes-and-models) should use unless instructed otherwise. It is also strongly recommended
that other [schemas](#recommended-schemas) should be based on it.


### 10.3.1. Tags {#1031-tags}

The core [schema](#recommended-schemas) uses the same [tags](#tags) as the [JSON schema](#json-schema).


### 10.3.2. Tag Resolution {#1032-tag-resolution}

The [core schema](#core-schema) [tag resolution](#tag-resolution) is an extension of the [JSON schema](#json-schema)
[tag resolution](#tag-resolution).

All [nodes](#nodes) with the "`!`{.language-plaintext .highlighter-rouge}" non-specific tag are
[resolved](#resolved-tags), by the standard [convention](#resolved-tags), to
"`tag:yaml.org,2002:seq`{.language-plaintext .highlighter-rouge}", "`tag:yaml.org,2002:map`{.language-plaintext
.highlighter-rouge}" or "`tag:yaml.org,2002:str`{.language-plaintext .highlighter-rouge}", according to their
[kind](#nodes).

[Collections](#collections) with the "`?`{.language-plaintext .highlighter-rouge}" non-specific tag (that is,
[untagged](#resolved-tags) [collections](#collections)) are [resolved](#resolved-tags) to
"`tag:yaml.org,2002:seq`{.language-plaintext .highlighter-rouge}" or "`tag:yaml.org,2002:map`{.language-plaintext
.highlighter-rouge}" according to their [kind](#nodes).

[Scalars](#scalars) with the "`?`{.language-plaintext .highlighter-rouge}" non-specific tag (that is, [plain
scalars](#plain-style)) are matched with an extended list of regular expressions. However, in this case, if none of the
regular expressions matches, the [scalar](#scalar) is [resolved](#resolved-tags) to
`tag:yaml.org,2002:str`{.language-plaintext .highlighter-rouge} (that is, considered to be a string).

  Regular expression                                                                                             Resolve
d to tag
  -------------------------------------------------------------------------------------------------------------- -------
---------------------------------
  `null | Null | NULL | ~`{.language-plaintext .highlighter-rouge}                                               tag:yam
l.org,2002:null
  `/* Empty */`{.language-plaintext .highlighter-rouge}                                                          tag:yam
l.org,2002:null
  `true | True | TRUE | false | False | FALSE`{.language-plaintext .highlighter-rouge}                           tag:yam
l.org,2002:bool
  `[-+]? [0-9]+`{.language-plaintext .highlighter-rouge}                                                         tag:yam
l.org,2002:int (Base 10)
  `0o [0-7]+`{.language-plaintext .highlighter-rouge}                                                            tag:yam
l.org,2002:int (Base 8)
  `0x [0-9a-fA-F]+`{.language-plaintext .highlighter-rouge}                                                      tag:yam
l.org,2002:int (Base 16)
  `[-+]? ( \. [0-9]+ | [0-9]+ ( \. [0-9]* )? ) ( [eE] [-+]? [0-9]+ )?`{.language-plaintext .highlighter-rouge}   tag:yam
l.org,2002:float (Number)
  `[-+]? ( \.inf | \.Inf | \.INF )`{.language-plaintext .highlighter-rouge}                                      tag:yam
l.org,2002:float (Infinity)
  `\.nan | \.NaN | \.NAN`{.language-plaintext .highlighter-rouge}                                                tag:yam
l.org,2002:float (Not a number)
  `*`{.language-plaintext .highlighter-rouge}                                                                    tag:yam
l.org,2002:str (Default)

**Example 10.9 Core Tag Resolution**

+-----------------------------------------------------------+-----------------------------------------------------------
+
|     A null: null                                          | :::: {.language-json .highlighter-rouge}
|
|     Also a null: # Empty                                  | ::: highlight
|
|     Not a null: ""                                        | ``` highlight
|
|     Booleans: [ true, True, false, FALSE ]                | { "A null": null,
|
|     Integers: [ 0, 0o7, 0x3A, -19 ]                       |   "Also a null": null,
|
|     Floats: [                                             |   "Not a null": "",
|
|       0., -0.0, .5, +12e03, -2E+05 ]                      |   "Booleans": [ true, true, false, false ],
|
|     Also floats: [                                        |   "Integers": [ 0, 7, 58, -19 ],
|
|       .inf, -.Inf, +.INF, .NAN ]                          |   "Floats": [
|
|                                                           |     0.0, -0.0, 0.5, 12000, -200000 ],
|
|                                                           |   "Also floats": [
|
|                                                           |     Infinity, -Infinity, Infinity, NaN ] }
|
|                                                           | ```
|
|                                                           | :::
|
|                                                           | ::::
|
+-----------------------------------------------------------+-----------------------------------------------------------
+


## 10.4. Other Schemas {#104-other-schemas}

None of the above recommended [schemas](#recommended-schemas) preclude the use of arbitrary explicit [tags](#tags).
Hence YAML [processors](#processes-and-models) for a particular programming language typically provide some form of
[local tags](#tags) that map directly to the language's [native data structures](#representing-native-data-structures)
(e.g., `!ruby/object:Set`{.language-plaintext .highlighter-rouge}).

While such [local tags](#tags) are useful for ad hoc [applications](#processes-and-models), they do not suffice for
stable, interoperable cross-[application](#processes-and-models) or cross-platform data exchange.

Interoperable [schemas](#recommended-schemas) make use of [global tags](#tags) (URIs) that
[represent](#representation-graph) the same data across different programming languages. In addition, an interoperable
[schema](#recommended-schemas) may provide additional [tag resolution](#tag-resolution) rules. Such rules may provide
additional regular expressions, as well as consider the path to the [node](#nodes). This allows interoperable
[schemas](#recommended-schemas) to use [untagged](#resolved-tags) [nodes](#nodes).

It is strongly recommended that such [schemas](#recommended-schemas) be based on the [core schema](#core-schema) defined
above.

# Reference Links

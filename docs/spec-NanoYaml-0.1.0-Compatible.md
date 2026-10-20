# NanoYaml Format 0.1.0 Compatible Profile Specification

## Chapter 1. Introduction

NanoYaml is a multi-purpose data format. It can be treated as a tight,
schema-less subset of the YAML 1.2.2 format, focused on performance, accuracy,
simplicity and round-trip survival.

Coincidentally, due to spec overlap, any NanoYaml document is also a valid
YAML 1.1 file.

This document specifies the Compatible profile, which can in turn be understood
as a superset of Strict profile.

## Chapter 2. Conventions

Productions are defined using the syntax `production-name ::= term`. A term
is an atomic term (a quoted string, a hexadecimal number `x0A`, a range of
hexadecimal numbers `[x20-x7E]`, or the name of a production), a lookaround
(`[ lookahead = term ]`, `[ lookahead != term ]`, `[ lookbehind = term ]`), a
special production (`<start-of-line>`, `<end-of-input>`, `<empty>`), a
parenthesized term, a concatenation of terms, an alternation of terms, a
difference of terms (`term - term`), or a quantified term (`term?`,
`term*`, `term+`).

All characters mentioned in this specification are Unicode code points. A
document is a sequence of lines. A line feed, a carriage return, or a
carriage return followed by a line feed denotes one line break; the
characters of a line end at its line break.

## Chapter 3. The format

### 3.1. The preamble

All directives, including NanoYaml Strict `%TAG` preamble directive are allowed,
but effectively ignored. A static check for the `%YAML` directive forces it to
carry a valid `x.y` version. A byte order mark may stand in front of the stream.

``` rule
[1] l-profile-tag ::= "%TAG ! nanoyaml:0.1.0:" b-as-line-feed
[2] l-document-start ::= "---" b-as-line-feed
[3] l-directive ::= "%" c-directive-name ( ( b-as-space | b-as-tab )
                                          ( c-printable - b-as-line-feed )* )? b-as-line-feed
[4] c-directive-name ::= ( c-printable - b-as-space - b-as-tab - b-as-line-feed )+
```

### 3.2. The document

A compatible stream contains one document. The content of the document
follows the document start, or the preamble when the document start is left
out. The content may be empty: an empty document denotes null.

A line that holds only `...` marks the end of the document; it is skipped,
and the content carries on after it. A second document start within the
content is invalid: the stream holds one document. A line that holds only a
comment (Chapter 7) belongs to the content wherever it stands.

``` rule
[5] l-nanoyaml-stream ::= ( l-directive | l-profile-tag )* l-document-start? l-content
[6] l-content ::= ( l-node | l-empty-line | l-comment | l-document-end )*
[7] l-node ::= l-scalar | l-block-collection | l-flow-collection
[8] l-scalar ::= l-plain-scalar | l-single-quoted-scalar | l-double-quoted-scalar | l-literal-scalar
               | l-folded-scalar
[9] l-block-collection ::= l-block-mapping | l-block-sequence
[10] l-flow-collection ::= l-flow-mapping | l-flow-sequence
[11] l-empty-line ::= b-as-line-feed
[12] l-document-end ::= "..." b-as-line-feed
```

## Chapter 4. Characters

### 4.1. Character set

A NanoYaml document uses the printable subset of the Unicode character set.
The allowed range excludes the C0 control block (except TAB, LF and CR), DEL,
the C1 control block (except NEL), the surrogate block, the byte order mark,
and the noncharacters xFFFE and xFFFF.

``` rule
[13] c-printable ::=
    x09                  # Tab
  | x0A                  # Line feed
  | x0D                  # Carriage return
  | [x20-x7E]            # Printable ASCII
  | x85                  # Next line
  | [xA0-xD7FF]          # Basic Multilingual Plane
  | [xE000-xFEFE]        # Additional Unicode areas
  | [xFFF0-xFFFD]        # End of the Basic Multilingual Plane
```

A character outside this set is represented in a document by an escape
sequence.

### 4.2. Indicator characters

``` rule
[14] c-indicator ::=
    "-"
  | "?"
  | ":"
  | ","
  | "["
  | "]"
  | "{"
  | "}"
  | "#"
  | "&"
  | "*"
  | "!"
  | "|"
  | ">"
  | "'"
  | "\""
  | "%"
  | "@"
  | "`"
```

``` rule
[15] c-flow-indicator ::= "," | "[" | "]" | "{" | "}"
```

### 4.3. White space and line breaks

The space and the tab separate, and the line feed ends a line (2.3). A tab
may stand within a line. The white space before a line's first content
character holds spaces only - outside of literal and folded scalar content,
which is read as written.

``` rule
[16] b-as-line-feed ::= x0A
[17] b-as-space ::= x20
[18] b-as-tab ::= x09
```

### 4.4. Escaped characters

A double-quoted scalar represents a character outside the printable set, and
the characters listed below, by an escape sequence.

``` rule
[19] c-escaped ::=
    "0"        # Null
  | "a"        # Bell
  | "b"        # Backspace
  | "t"        # Tab
  | "n"        # Line feed
  | "v"        # Vertical tab
  | "f"        # Form feed
  | "r"        # Carriage return
  | "e"        # Escape
  | " "        # Space
  | "\""       # Double quote
  | "\\"       # Backslash
  | "/"        # Solidus
  | "N"        # Next line
  | "_"        # Non-breaking space
  | "L"        # Line separator
  | "P"        # Paragraph separator
  | "x" c-hex-digit c-hex-digit
  | "u" c-hex-digit c-hex-digit c-hex-digit c-hex-digit
  | "U" c-hex-digit c-hex-digit c-hex-digit c-hex-digit c-hex-digit c-hex-digit c-hex-digit c-hex-digit
```

``` rule
[20] c-hex-digit ::= [x30-x39] | [x41-x46] | [x61-x66]
```

## Chapter 5. Scalars

A scalar denotes a string. A scalar is written in one of five styles: plain,
single-quoted, double-quoted, literal, or folded.

### 5.1. Plain style

A plain scalar begins with a printable character that is not an indicator,
and continues with characters that are not indicators. A plain scalar is not
empty, and begins and ends with a character that is not white space. A plain
scalar never writes a document marker: `---` and `...`, and anything that
begins with `--- ` or `... `, is reserved.

Inside a plain scalar, `:` is written when it is not followed by a space or
a line break, and `#` is written when no white space stands in front of it,
since a white space and a `#` start a comment.

A plain scalar spans one line, or several. As the content of a whole
document, every following line that could stand as plain content continues
it. As the value of an entry, only a line indented deeper than the entry
continues it, and the continuation ends at a blank line, at a line that
carries an entry of its own, and at a line that is not indented deeper. Each
line break of a continuation denotes one space.

``` rule
[21] l-plain-scalar ::= ns-plain-first c-ns-plain-char* b-as-line-feed
[22] ns-plain-first ::= ( c-plain-safe - b-as-space - c-indicator )
                      | ( ( "-" | ":" ) ( c-plain-safe - b-as-space ) )
[23] c-plain-safe ::= c-printable - c-quoted
[24] c-ns-plain-char ::= c-plain-safe
                       - ( ":" [ lookahead = b-as-space | b-as-line-feed ] )
                       - ( "#" [ lookbehind = b-as-space ] )
[25] c-quoted ::= "\"" | "'" | "\\" | x85 | xA0 | x2028 | x2029
```

The production writes one line of the scalar. A continuation line is written
by the same production, and joins the scalar as the prose says.

### 5.2. Single-quoted style

A single-quoted scalar begins with `'` and ends with `'`. A `'` inside the
scalar is written twice.

``` rule
[26] l-single-quoted-scalar ::= "'" ( c-single-quoted-char* "'" ) b-as-line-feed
[27] c-single-quoted-char ::= c-printable - "'" | "''"
```

### 5.3. Double-quoted style

A double-quoted scalar begins with `"` and ends with `"`. A character of the
scalar is written as itself, or as an escape sequence.

``` rule
[28] l-double-quoted-scalar ::= "\"" ( c-double-quoted-char* "\"" ) b-as-line-feed
[29] c-double-quoted-char ::= c-printable - "\"" - "\\" | "\" c-escaped
```

### 5.4. Literal style

A literal scalar begins with `|` and denotes its content verbatim, keeping
its line breaks. It is written as the value of a mapping entry. The content
lines are indented more than the line that carries the indicator, and the
indentation of the first content line is removed from every content line.

The chomping indicator tells what happens to the line breaks at the end of
the content: `-` removes them all, `+` keeps them all, and without one a
single line break remains. The indentation indicator is one digit from one
to nine, written before or after the chomping indicator; it is accepted and
changes nothing, since the content lines carry their own indentation.

``` rule
[30] l-literal-scalar ::= "|" c-b-block-header l-literal-content
[31] c-b-block-header ::= ( c-as-digit )? c-chomping-indicator? ( c-as-digit )?
[32] c-chomping-indicator ::= "-" | "+"
[33] c-as-digit ::= [x31-x39]
[34] l-literal-content ::= ( l-block-line | l-empty-line )*
[35] l-block-line ::= s-indent ( c-printable - b-as-line-feed )* b-as-line-feed
```

### 5.5. Folded style

A folded scalar begins with `>` and denotes its content with the line breaks
of a paragraph folded into spaces. It is written as the value of a mapping
entry, and its content is collected like the content of a literal scalar. A
blank line ends the paragraph and its line break remains, and a content line
indented deeper than the first content line keeps its line break and stands
on its own.

``` rule
[36] l-folded-scalar ::= ">" c-b-block-header l-folded-content
[37] l-folded-content ::= l-literal-content
```

## Chapter 6. Collections

A collection denotes a mapping or a sequence. A mapping denotes an ordered
set of key/value pairs, each key unique within the mapping. A sequence
denotes an ordered set of nodes.

### 6.1. Block mappings

A block mapping is a sequence of entries, each on its own line, each
indented by the same amount. An entry consists of a key, a `:`, and a value.
The key is a scalar. The value is written on the same line as the `:` - a
scalar or a flow collection - or, when the same line holds nothing after the
`:`, as a block collection in the lines beneath the entry, indented deeper
than the entry. A sequence may also be written level with its entry, from
the line after the `:` until the line that is not a sequence entry. An entry
without a value denotes null.

``` rule
[38] l-block-mapping ::= l-block-mapping-entry+
[39] l-block-mapping-entry ::= s-indent l-scalar ":" ( s-separate-in-line+ ( l-flow-node | l-block-node )
                                                     | l-block-node )?
[40] s-indent ::= ( b-as-space )*
[41] s-separate-in-line ::= ( b-as-space | b-as-tab )*
[42] l-block-node ::= l-block-mapping | l-block-sequence
```

### 6.2. Block sequences

A block sequence is a sequence of entries, each on its own line, each
indented by the same amount. An entry consists of a `-` and a node: a scalar
or a flow collection written after the `-`, or a block collection in the
lines beneath a `-` that carries nothing after it. An entry without a node
is invalid.

``` rule
[43] l-block-sequence ::= l-block-sequence-entry+
[44] l-block-sequence-entry ::= s-indent "-" s-separate-in-line? ( l-flow-node | l-block-node )
```

### 6.3. Flow mappings

A flow mapping is delimited by `{` and `}`. Its entries are separated by
`,`, and a trailing `,` may stand before the closing `}`. An entry consists
of a key, a `:`, and a value. The key is a flow node. Inside a flow
collection a plain scalar also excludes the flow indicator characters, and a
`:` that is followed by a white space or by a `,`, `]`, or `}` separates the
pair and needs a value after it.

``` rule
[45] l-flow-mapping ::= "{" s-flow-separate ( l-flow-pair
                       ( "," s-flow-separate l-flow-pair )*
                       ( "," s-flow-separate )? )? "}"
[46] l-flow-pair ::= l-flow-node ":" s-flow-separate+ l-flow-node
[47] l-flow-node ::= l-flow-collection | l-scalar
```

### 6.4. Flow sequences

A flow sequence is delimited by `[` and `]`. Its entries are separated by
`,`, and a trailing `,` may stand before the closing `]`. An entry is a flow
node, or a pair `key: value`, which denotes a mapping of that one pair.

``` rule
[48] l-flow-sequence ::= "[" s-flow-separate ( l-flow-sequence-entry
                       ( "," s-flow-separate l-flow-sequence-entry )*
                       ( "," s-flow-separate )? )? "]"
[49] l-flow-sequence-entry ::= l-flow-node | l-flow-pair
[50] s-flow-separate ::= ( b-as-space | b-as-line-feed )*
```

## Chapter 7. Comments

A comment starts with a `#` at the beginning of a line or after a white
space character, and extends to the end of the line. A comment may close a
line that carries content, and a line that holds only a comment is a comment
line, which the content accepts wherever it stands.

``` rule
[51] c-comment ::= "#"
[52] l-comment ::= s-indent c-comment s-comment-char* b-as-line-feed
[53] s-comment-char ::= c-printable - b-as-line-feed
```

## Chapter 8. Resolution

### 8.1. Scalars

A scalar denotes a string. A quoted scalar denotes the string its characters
spell. A plain scalar denotes the string its characters spell.

### 8.2. Null

A plain scalar denotes null when it is `~`, `null`, `Null` or `NULL`. Any
other spelling, `nUll` among them, denotes the string it spells.

``` rule
[54] l-null ::= "~" | "null" | "Null" | "NULL"
```

### 8.3. Numbers

A plain scalar denotes a number when it is written in one of the following
forms. A number carries no digit group separator. The forms follow the YAML
1.2.2 core schema: a leading zero is allowed, and the octal form is written
with `0o`.

``` rule
[55] l-integer ::= ( "-" | "+" )? [0-9]+
[56] l-octal-integer ::= "0o" [0-7]+
[57] l-hex-integer ::= "0x" ( [0-9] | [a-f] | [A-F] )+
[58] l-float ::= ( "-" | "+" )? ( [0-9]+ ( "." [0-9]* )? | "." [0-9]+ )
                  ( [eE] ( "-" | "+" )? [0-9]+ )?
[59] l-infinity ::= ".inf" | "+.inf" | "-.inf"
                  | ".Inf" | ".INF" | "+.Inf" | "+.INF" | "-.Inf" | "-.INF"
[60] l-nan ::= ".nan" | ".NaN" | ".NAN"
```

### 8.4. Booleans

A plain scalar denotes a boolean when it is written in one of the following
forms; any other spelling, `yes` and `no` among them, denotes a string.

``` rule
[61] l-boolean ::= "true" | "false" | "True" | "TRUE" | "False" | "FALSE"
```

# NanoYaml Strict Format Specification

## Revision 0.1.0 (2026-10-22)

NanoYaml is a multi-purpose data format. It can be treated as a tight,
schema-less subset of the YAML 1.2.2 format, focused on performance, accuracy,
simplicity and round-trip survival.

Coincidentally, due to spec overlap, any NanoYaml document is also a valid
YAML 1.1 file.

This document specifies the Strict profile, which can in turn be understood as
a subset of Compatible profile.

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

### 3.1. The preamble tag

A Strict NanoYaml stream begins with the directive line
`%TAG ! nanoyaml:0.1.0:` and the line `---`. The Strict profile carries no
other directive: the tag line is the whole preamble, and the document start
follows it directly. A byte order mark may stand in front of the tag; it is
dropped, it is not part of the stream.

``` yaml
%TAG ! nanoyaml:0.1.0:
---
```

``` rule
[1] l-profile-tag ::= "%TAG ! nanoyaml:0.1.0:" b-as-line-feed
[2] l-document-start ::= "---" b-as-line-feed
```

### 3.2. The document

A NanoYaml stream contains one document. The content of the document follows
the document start. The content holds at least one node: a stream whose
content is nothing but empty lines and comments is invalid.

A line that holds only a comment (Chapter 7) belongs to the content wherever
it stands.

``` rule
[3] l-nanoyaml-stream ::= l-profile-tag l-document-start l-content
[4] l-content ::= ( l-node | l-empty-line | l-comment )*
[5] l-node ::= l-scalar | l-block-collection | l-flow-collection
[6] l-scalar ::= l-plain-scalar | l-single-quoted-scalar | l-double-quoted-scalar | l-literal-scalar
               | l-folded-scalar
[7] l-block-collection ::= l-block-mapping | l-block-sequence
[8] l-flow-collection ::= l-flow-mapping | l-flow-sequence
[9] l-empty-line ::= b-as-line-feed
```

## Chapter 4. Characters

### 4.1. Character set

A NanoYaml document uses the printable subset of the Unicode character set.
The allowed range excludes the C0 control block (except LF and CR), DEL, the
C1 control block (except NEL), the surrogate block, the byte order mark, and
the noncharacters xFFFE and xFFFF. Any tab must be written as the escape
sequence `\t` if in a value, or replaced by spaces in indent.

``` rule
[10] c-printable ::=
    x0A                  # Line feed
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
[11] c-indicator ::=
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
[12] c-flow-indicator ::= "," | "[" | "]" | "{" | "}"
```

### 4.3. White space and line breaks

A strict document is written in the space character; the line feed ends a
line (2.3), and the tab character does not appear (4.1).

``` rule
[13] b-as-line-feed ::= x0A
[14] b-as-space ::= x20
```

### 4.4. Escaped characters

A double-quoted scalar represents a character outside the printable set, and
the characters listed below, by an escape sequence.

``` rule
[15] c-escaped ::=
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
[16] c-hex-digit ::= [x30-x39] | [x41-x46] | [x61-x66]
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
[17] l-plain-scalar ::= ns-plain-first c-ns-plain-char* b-as-line-feed
[18] ns-plain-first ::= ( c-plain-safe - b-as-space - c-indicator )
                      | ( ( "-" | ":" ) ( c-plain-safe - b-as-space ) )
[19] c-plain-safe ::= c-printable - c-quoted
[20] c-ns-plain-char ::= c-plain-safe
                       - ( ":" [ lookahead = b-as-space | b-as-line-feed ] )
                       - ( "#" [ lookbehind = b-as-space ] )
[21] c-quoted ::= "\"" | "'" | "\\" | x85 | xA0 | x2028 | x2029
```

The production writes one line of the scalar. A continuation line is written
by the same production, and joins the scalar as the prose says.

### 5.2. Single-quoted style

A single-quoted scalar begins with `'` and ends with `'`. A `'` inside the
scalar is written twice.

``` rule
[22] l-single-quoted-scalar ::= "'" ( c-single-quoted-char* "'" ) b-as-line-feed
[23] c-single-quoted-char ::= c-printable - "'" | "''"
```

### 5.3. Double-quoted style

A double-quoted scalar begins with `"` and ends with `"`. A character of the
scalar is written as itself, or as an escape sequence.

``` rule
[24] l-double-quoted-scalar ::= "\"" ( c-double-quoted-char* "\"" ) b-as-line-feed
[25] c-double-quoted-char ::= c-printable - "\"" - "\\" | "\" c-escaped
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
[26] l-literal-scalar ::= "|" c-b-block-header l-literal-content
[27] c-b-block-header ::= ( c-as-digit )? c-chomping-indicator? ( c-as-digit )?
[28] c-chomping-indicator ::= "-" | "+"
[29] c-as-digit ::= [x31-x39]
[30] l-literal-content ::= ( l-block-line | l-empty-line )*
[31] l-block-line ::= s-indent ( c-printable - b-as-line-feed )* b-as-line-feed
```

### 5.5. Folded style

A folded scalar begins with `>` and denotes its content with the line breaks
of a paragraph folded into spaces. It is written as the value of a mapping
entry, and its content is collected like the content of a literal scalar. A
blank line ends the paragraph and its line break remains, and a content line
indented deeper than the first content line keeps its line break and stands
on its own.

``` rule
[32] l-folded-scalar ::= ">" c-b-block-header l-folded-content
[33] l-folded-content ::= l-literal-content
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
`:`, as the block collection in the lines beneath the entry, indented deeper
than the entry. An entry without a value is invalid.

``` rule
[34] l-block-mapping ::= l-block-mapping-entry+
[35] l-block-mapping-entry ::= s-indent l-scalar ":" ( s-separate-in-line+ ( l-flow-node | l-block-node )
                                                     | l-block-node )?
[36] s-indent ::= ( b-as-space )*
[37] s-separate-in-line ::= ( b-as-space )*
[38] l-block-node ::= l-block-mapping | l-block-sequence
```

### 6.2. Block sequences

A block sequence is a sequence of entries, each on its own line, each
indented by the same amount. An entry consists of a `-` and a node: a scalar
or a flow collection written after the `-`, or a block collection in the
lines beneath a `-` that carries nothing after it. An entry without a node
is invalid.

``` rule
[39] l-block-sequence ::= l-block-sequence-entry+
[40] l-block-sequence-entry ::= s-indent "-" s-separate-in-line? ( l-flow-node | l-block-node )
```

### 6.3. Flow mappings

A flow mapping is delimited by `{` and `}`. Its entries are separated by
`,`, and a trailing `,` may stand before the closing `}`. An entry consists
of a key, a `:`, and a value. The key is a flow node. Inside a flow
collection a plain scalar also excludes the flow indicator characters, and a
`:` that is followed by a white space or by a `,`, `]`, or `}` separates the
pair and needs a value after it.

``` rule
[41] l-flow-mapping ::= "{" s-flow-separate ( l-flow-pair
                       ( "," s-flow-separate l-flow-pair )*
                       ( "," s-flow-separate )? )? "}"
[42] l-flow-pair ::= l-flow-node ":" s-flow-separate+ l-flow-node
[43] l-flow-node ::= l-flow-collection | l-scalar
```

### 6.4. Flow sequences

A flow sequence is delimited by `[` and `]`. Its entries are separated by
`,`, and a trailing `,` may stand before the closing `]`. An entry is a flow
node, or a pair `key: value`, which denotes a mapping of that one pair.

``` rule
[44] l-flow-sequence ::= "[" s-flow-separate ( l-flow-sequence-entry
                       ( "," s-flow-separate l-flow-sequence-entry )*
                       ( "," s-flow-separate )? )? "]"
[45] l-flow-sequence-entry ::= l-flow-node | l-flow-pair
[46] s-flow-separate ::= ( b-as-space | b-as-line-feed )*
```

## Chapter 7. Comments

A comment starts with a `#` at the beginning of a line or after a white
space character, and extends to the end of the line. A comment may close a
line that carries content, and a line that holds only a comment is a comment
line, which the content accepts wherever it stands.

``` rule
[47] c-comment ::= "#"
[48] l-comment ::= s-indent c-comment s-comment-char* b-as-line-feed
[49] s-comment-char ::= c-printable - b-as-line-feed
```

## Chapter 8. Resolution

### 8.1. Scalars

A scalar denotes a string. A quoted scalar denotes the string its characters
spell. A plain scalar denotes the string its characters spell.

### 8.2. Null

A plain scalar denotes null when it is `~` or `null`. Any other spelling of
the word, `Null` and `NULL` among them, is rejected: the document does not
parse.

``` rule
[50] l-null ::= "~" | "null"
```

### 8.3. Numbers

A plain scalar denotes a number when it is written in one of the following
forms. A number carries no digit group separator.

``` rule
[51] l-integer ::= ( "-" | "+" )? ( "0" | [1-9] [0-9]* )
[52] l-hex-integer ::= "0x" ( [0-9] | [a-f] | [A-F] )+
[53] l-float ::= ( "-" | "+" )? ( ( "0" | [1-9] [0-9]* ) ( "." [0-9]* )? | "." [0-9]+ )
                  ( [eE] ( "-" | "+" )? [0-9]+ )?
[54] l-infinity ::= ".inf" | "+.inf" | "-.inf"
[55] l-nan ::= ".nan"
```

### 8.4. Booleans

A plain scalar denotes a boolean when it is `true` or `false`. Any other
spelling, `True` and `TRUE` among them, denotes a string.

``` rule
[56] l-boolean ::= "true" | "false"
```

#!/usr/bin/env bash
# _tools/check-text-hygiene.sh - text file hygiene: every text file ends with
# a newline and carries no trailing whitespace, docs keep their line
# widths (80 soft, reported; 120 hard, fails), C# keeps its prose comments to
# the empty // line-break (documentation comments are the library's public
# API docs and are allowed), and multi-line C# lists end with a comma.
#
# Sourced by _tools/check-style.sh (which checks the whole tracked tree).
# Run directly it checks the given files, or every tracked file when given
# none; that is how .githooks/pre-commit checks the staged files.
# Functions only, so no side effects when sourced. Pure ASCII.

# Globs that make a file a text file here: the extensions and names the
# ASCII rule and the build tooling already cover, plus the extensionless
# files that are text (git hooks, global.json). The icon is a PNG and the
# vendored spec texts are markdown, so both are handled by their own rules.
text_file_globs() {
    printf '%s\n' \
        '*.cs' '*.json' '*.md' '*.sh' '*.py' '*.csproj' '*.yml' '*.yaml' \
        '*.sln' '*.slnx' '*.uid' 'LICENSE' 'CHANGELOG.md' '.gitignore' \
        '.editorconfig' 'global.json' '.githooks/*'
}

is_text_file() {
    local file="$1" glob
    while IFS= read -r glob; do
        case "$file" in
            $glob) return 0 ;;
        esac
    done < <(text_file_globs)
    return 1
}

# Markdown line widths: 80 is advice and only reported, 120 fails. Fenced
# code is exempt from the soft limit, since a path or a command does not wrap,
# but not from the hard one - past 120 columns it is a wall of text. A table
# row and a line holding a link cannot be wrapped at all, so they are exempt
# from both. The vendored spec texts are not our prose and keep their own
# widths.
check_doc_width() {
    local file="$1" report
    case "$file" in
        *.md) ;;
        *) return 0 ;;
    esac
    case "$file" in
        docs/spec-*.md) return 0 ;;
    esac
    report=$(awk '
        /^[[:space:]]*```/ { fence = !fence; next }
        /^[[:space:]]*\|/ { next }
        /\]\(http/ { next }
        !fence && length > 80 { soft = soft NR "," }
        length > 120 { hard = hard NR "," }
        END { if (soft != "") print "soft " soft; if (hard != "") print "hard " hard }
    ' "$file")
    local fail=0 kind lines
    while read -r kind lines; do
        [ -n "$kind" ] || continue
        lines="${lines%,}"
        if [ "$kind" = soft ]; then
            echo "  $file: over 80 columns (soft) on line(s) $lines"
        else
            echo "  $file: over 120 columns on line(s) $lines"
            fail=1
        fi
    done <<< "$report"
    return $fail
}

# C# comments: the skill allows no prose comments at all, only the
# empty line comment `//` used as a forced line-break. A documentation
# comment is the library's public API doc and is allowed. Anything else -
# a note, a trailing remark - is a style failure, reported per line. A
# `//` inside a string literal is not a comment, so the scan tracks
# string state.
check_cs_comments() {
    local file="$1" lines
    case "$file" in
        *.cs) ;;
        *) return 0 ;;
    esac
    lines=$(awk '
        {
            line = $0
            in_str = 0
            i = 1
            comment = 0
            while (i <= length(line)) {
                c = substr(line, i, 1)
                if (in_str) {
                    if (c == "\\") { i += 2; continue }
                    if (c == "\"") { in_str = 0 }
                } else {
                    if (c == "\"") { in_str = 1 }
                    else if (c == "/" && substr(line, i+1, 1) == "/") {
                        if (substr(line, i+2, 1) == "/") { break }
                        # a forced line-break: nothing but whitespace
                        # follows the //, and the // is not the whole
                        # line content either (it ends a code line)
                        after = substr(line, i+2)
                        gsub(/[ \t]/, "", after)
                        if (after != "") { comment = 1 }
                        break
                    }
                }
                i++
            }
            if (comment) print NR
        }
    ' "$file" | paste -sd, -)
    if [ -n "$lines" ]; then
        echo "  $file: comment on line(s) $lines (only empty // line-breaks allowed)"
        return 1
    fi
    return 0
}

# C# trailing commas: a multi-line list ends with a comma, so a diff
# shows one line, not two. Lists are initializers, enum bodies, switch
# expression arms and collection expressions; argument lists, blocks,
# conditions and indexers are not lists and take no comma. The open
# brackets say which is which: each opener is classified as it opens,
# and a line is missing its comma when it ends an element, the next
# line closes the list, and the open bracket is a list. Strings are
# not code, so the scan strips them first, tracking the string state
# that runs over lines (verbatim and raw strings).
check_cs_trailing_commas() {
    local file="$1" lines
    case "$file" in
        *.cs) ;;
        *) return 0 ;;
    esac
    lines=$(awk -v q="'" '
        function push(br, k) { depth++; brk[depth] = br; kind[depth] = k }
        function pop() { lastclosed = kind[depth]; depth-- }

        # A paren list never takes a comma. A constructor call
        # still marks the initializer that follows it, so new
        # Foo() { is a list while if (x) { is a block.
        function classify_paren() {
            if (tok1 == "new")
                return 1
            if (tok1 ~ /^[A-Za-z_][A-Za-z0-9_]*$/ && tok2 == "new")
                return 1
            return 0
        }

        # A bracket is a collection expression, or an indexer or
        # an array type, which is not one.
        function classify_bracket() {
            if (tok1 == "return" || tok1 == "in" || tok1 == "is")
                return 1
            if (tok1 ~ /^[A-Za-z0-9_]/ || tok1 == ")" || tok1 == "]" || tok1 == "\"" || tok1 == ">" || tok1 == ".")
                return 0
            return 1
        }

        # A brace is an initializer, an enum body or a switch
        # expression, or a block, which is not one. A type body
        # is a block. A brace after a paren takes the paren kind,
        # so the constructor call above reaches the initializer.
        function classify_brace() {
            if (typebody) { typebody = 0; return 0 }
            if (tok1 == "=" || tok1 == "," || tok1 == "{" || tok1 == "switch" || tok1 == "with" || tok1 == "new" || tok1 == "]")
                return 1
            if (tok1 == ")")
                return lastclosed
            if (tok1 == ">")
                return tok2 == "=" ? 0 : 1
            if (tok1 ~ /^[A-Za-z_][A-Za-z0-9_]*$/) {
                if (tok2 == "new" || tok2 == "enum")
                    return 1
                return 0
            }
            return 0
        }

        # Strip comments and string literals, keeping a space
        # where whitespace was so words stay apart. Verbatim and
        # raw strings run over lines, so their state carries.
        function strip(line,    i, n, c, out, prev) {
            out = ""
            i = 1
            n = length(line)
            prev = ""
            while (i <= n) {
                c = substr(line, i, 1)
                if (raw) {
                    if (c == "\"" && substr(line, i+1, 1) == "\"" && substr(line, i+2, 1) == "\"") { raw = 0; i += 3; out = out "\""; prev = "\"" }
                    else i++
                    continue
                }
                if (verb) {
                    if (c == "\"") {
                        if (substr(line, i+1, 1) == "\"") { i += 2; continue }
                        verb = 0
                        i++
                        out = out "\""
                        prev = "\""
                        continue
                    }
                    i++
                    continue
                }
                # A character literal is one quoted character, plain
                # or escaped: swallow it whole so its brackets
                # and quotes never read as code.
                if (c == q) {
                    if (substr(line, i+1, 1) == "\\") { i += 4 }
                    else if (substr(line, i+2, 1) == q) { i += 3 }
                    else { out = out c; prev = c; i++; continue }
                    out = out q
                    prev = q
                    continue
                }
                if (c == "\"") {
                    if (substr(line, i+1, 1) == "\"" && substr(line, i+2, 1) == "\"") { raw = 1; i += 3; continue }
                    if (prev == "@") { verb = 1; i++; continue }
                    i++
                    while (i <= n) {
                        if (substr(line, i, 1) == "\\") { i += 2; continue }
                        if (substr(line, i, 1) == "\"") { i++; break }
                        i++
                    }
                    out = out "\""
                    prev = "\""
                    continue
                }
                if (c == "/" && substr(line, i+1, 1) == "/") break
                if (c == " " || c == "\t") { out = out " "; i++; continue }
                out = out c
                prev = c
                i++
            }
            return out
        }

        { lines[NR] = $0 }

        END {
            for (i = 1; i <= NR; i++) code[i] = strip(lines[i])
            for (i = 1; i <= NR; i++) {
                c = code[i]
                j = 1
                m = length(c)
                lastcode = ""
                while (j <= m) {
                    ch = substr(c, j, 1)
                    if (ch == " ") { j++; continue }
                    if (ch ~ /[A-Za-z0-9_]/) {
                        w = ch; j++
                        while (j <= m && substr(c, j, 1) ~ /[A-Za-z0-9_]/) { w = w substr(c, j, 1); j++ }
                        tok2 = tok1; tok1 = w
                        if (w == "class" || w == "struct" || w == "record" || w == "interface" || w == "namespace")
                            typebody = 1
                        lastcode = substr(w, length(w), 1)
                    } else {
                        if (ch == "(") k = classify_paren()
                        else if (ch == "[") k = classify_bracket()
                        else if (ch == "{") k = classify_brace()
                        tok2 = tok1; tok1 = ch
                        lastcode = ch
                        if (ch == "(") push("(", k)
                        else if (ch == "[") push("[", k)
                        else if (ch == "{") push("{", k)
                        else if (ch == ")" || ch == "]" || ch == "}") { if (depth > 0) pop() }
                        j++
                    }
                }
                if (lastcode ~ /[A-Za-z0-9_)\]}"]/ || lastcode == q) {
                    if (depth > 0 && kind[depth] == 1 && brk[depth] != "(") {
                        n = i + 1
                        while (n <= NR && code[n] ~ /^ *$/) n++
                        if (n <= NR) {
                            nxt = code[n]
                            sub(/^ +/, "", nxt)
                            nch = substr(nxt, 1, 1)
                            br = brk[depth]
                            if ((br == "(" && nch == ")") || (br == "[" && nch == "]") || (br == "{" && nch == "}"))
                                print i
                        }
                    }
                }
            }
        }
    ' "$file" | paste -sd, -)
    if [ -n "$lines" ]; then
        echo "  $file: no trailing comma on line(s) $lines (multi-line lists end with a comma)"
        return 1
    fi
    return 0
}

# Echoes one line per problem, returns 1 when the file has a hard one.
check_text_file() {
    local file="$1" lines fail=0
    [ -f "$file" ] || return 0
    if [ -n "$(tail -c 1 -- "$file")" ]; then
        echo "  $file: no newline at end of file"
        fail=1
    fi
    lines=$(grep -nE '[[:space:]]+$' -- "$file" | cut -d: -f1 | paste -sd, -)
    if [ -n "$lines" ]; then
        echo "  $file: trailing whitespace on line(s) $lines"
        fail=1
    fi
    check_doc_width "$file" || fail=1
    check_cs_comments "$file" || fail=1
    check_cs_trailing_commas "$file" || fail=1
    return $fail
}

# Checks the given files, ignoring generated directories and non-text ones.
check_text_files() {
    local file fail=0
    for file in "$@"; do
        case "$file" in
            artifacts/* | .godot/*) continue ;;
        esac
        is_text_file "$file" || continue
        check_text_file "$file" || fail=1
    done
    [ "$fail" -eq 0 ]
}

if [ "${BASH_SOURCE[0]}" = "$0" ]; then
    set -uo pipefail
    cd "$(git rev-parse --show-toplevel)" || exit 1
    if [ "$#" -gt 0 ]; then
        check_text_files "$@"
    else
        check_text_files $(git ls-files)
    fi
fi

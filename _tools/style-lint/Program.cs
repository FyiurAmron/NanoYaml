using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Vaxquis.NanoYaml.StyleLint;

static class Program {
    static int Main(string[] args) {
        List<string> inputs = [.. args];
        bool fix = inputs.Remove("--fix");
        List<string> roots = inputs.Any() ? inputs : ["."];
        int fail = 0;
        List<string> files = [];
        roots.ForEach(root => {
            if (Directory.Exists(root)) {
                Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories).Where(path => !Skipped(path)).ForEach(files.Add);
            } else if (File.Exists(root)) {
                if (!Skipped(root)) {
                    files.Add(root);
                }
            } else {
                Console.WriteLine($"    missing path: {root}");
                fail = 1;
            }
        });
        files.Sort();
        files.ForEach(path => {
            string text = File.ReadAllText(path);
            if (fix) {
                text = ApplyFixes(path, text);
            }
            SyntaxNode root = CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.Preview), path).GetRoot();
            AllProblems(root).Concat(Extensions(path, text)).ForEach(problem => {
                Console.WriteLine($"    {path}: {problem}");
                fail = 1;
            });
            TernaryHint(root).ForEach(hint => Console.WriteLine($"    {path}: {hint}"));
        });
        return fail;
    }

    static bool Skipped(string path) =>
        Flat(path).Contains("/bin/") || Flat(path).Contains("/obj/");

    static string Flat(string path) =>
        $"/{path.Replace('\\', '/').Trim('/')}/";

    static IEnumerable<string> AllProblems(SyntaxNode root) =>
        Checks.SelectMany(check => check(root));

    static readonly List<Func<SyntaxNode, IEnumerable<string>>> Checks = [
        Comments,
        Statements,
        Arrays,
        Access,
        Groups,
        Counts,
        TestVars,
        Periods,
        Commas,
        TargetTypedNew,
        Interpolation,
        ArrowBreak,
        JoinToString,
        AppendChain,
        PartialType,
        OpenTypes,
    ];

    static int Line(SyntaxNode node) =>
        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

    static int Line(SyntaxToken token) =>
        token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

    static int Line(SyntaxTrivia trivia) =>
        trivia.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

    static IEnumerable<string> Comments(SyntaxNode root) {
        foreach (SyntaxTrivia trivia in root.DescendantTrivia()) {
            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)) {
                continue;
            }

            if ((trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                && trivia.ToString().Trim() != "//") {
                yield return $"line {Line(trivia)}: comment - only a bare // break survives";
            }
        }
    }

    static IEnumerable<string> Statements(SyntaxNode root) {
        foreach (BlockSyntax block in root.DescendantNodes().OfType<BlockSyntax>()) {
            if (block.Statements.Any()
                && Line(block.OpenBraceToken) == Line(block.CloseBraceToken)
                && Line(block.Statements[0]) == Line(block.OpenBraceToken)) {
                yield return $"line {Line(block.Statements[0])}: shared line - one statement per line";
            }
            for (int i = 1; i < block.Statements.Count; i++) {
                if (Line(block.Statements[i]) == Line(block.Statements[i - 1])) {
                    yield return $"line {Line(block.Statements[i])}: shared line - one statement per line";
                }
            }
        }
        foreach (SwitchSectionSyntax section in root.DescendantNodes().OfType<SwitchSectionSyntax>()) {
            for (int i = 1; i < section.Statements.Count; i++) {
                if (Line(section.Statements[i]) == Line(section.Statements[i - 1])) {
                    yield return $"line {Line(section.Statements[i])}: shared line - one statement per line";
                }
            }
        }
    }

    static readonly HashSet<string> BufferElements = [
        "double", "float", "byte", "char",
    ];

    static IEnumerable<string> Arrays(SyntaxNode root) {
        foreach (ArrayTypeSyntax array in root.DescendantNodes().OfType<ArrayTypeSyntax>()) {
            if (array.Parent is not ArrayCreationExpressionSyntax && !ExemptArray(array)) {
                yield return $"line {Line(array)}: array type - collections are List<>";
            }
        }
        foreach (ArrayCreationExpressionSyntax made in root.DescendantNodes().OfType<ArrayCreationExpressionSyntax>()) {
            if (!ExemptArray(made.Type)) {
                yield return $"line {Line(made)}: array allocation - collections are List<>";
            }
        }
        foreach (ImplicitArrayCreationExpressionSyntax made in root.DescendantNodes().OfType<ImplicitArrayCreationExpressionSyntax>()) {
            yield return $"line {Line(made)}: array allocation - collections are List<>";
        }
        foreach (InvocationExpressionSyntax call in root.DescendantNodes().OfType<InvocationExpressionSyntax>()) {
            if (call.Expression is MemberAccessExpressionSyntax made
                && made.Name.Identifier.Text == "ToArray"
                && !call.ArgumentList.Arguments.Any()) {
                yield return $"line {Line(call)}: ToArray call - collections are List<>";
            }
        }
    }

    static bool ExemptArray(ArrayTypeSyntax array) =>
        array.Parent is ParameterSyntax given
        && (given.Modifiers.Any(m => m.IsKind(SyntaxKind.ParamsKeyword)) || given.Identifier.Text == "args")
        || ExemptElement(array.ElementType)
        || ForcedSignature(array);

    static bool ForcedSignature(ArrayTypeSyntax array) =>
        array.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault() is MemberDeclarationSyntax member
        && ForcedMember(member)
        && !array.Ancestors().TakeWhile(node => node != member)
            .Any(node => node is BlockSyntax or ArrowExpressionClauseSyntax or AccessorDeclarationSyntax or AttributeArgumentSyntax);

    static bool ForcedMember(MemberDeclarationSyntax member) =>
        member switch {
            MethodDeclarationSyntax routine => routine.ExplicitInterfaceSpecifier is not null
                || routine.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)),
            PropertyDeclarationSyntax held => held.ExplicitInterfaceSpecifier is not null
                || held.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)),
            IndexerDeclarationSyntax kept => kept.ExplicitInterfaceSpecifier is not null
                || kept.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)),
            _ => false,
        };

    static bool ExemptElement(TypeSyntax? type) =>
        type switch {
            ArrayTypeSyntax nested => ExemptElement(nested.ElementType),
            PredefinedTypeSyntax head => BufferElements.Contains(head.Keyword.Text),
            IdentifierNameSyntax named => BufferElements.Contains(named.Identifier.Text),
            _ => false,
        };

    static IEnumerable<string> Access(SyntaxNode root) {
        foreach (MemberDeclarationSyntax member in root.DescendantNodes().OfType<MemberDeclarationSyntax>()) {
            if (HasPrivate(member)) {
                yield return $"line {Line(member)}: default modifier - never emit it";
            }
        }
        foreach (AccessorDeclarationSyntax accessor in root.DescendantNodes().OfType<AccessorDeclarationSyntax>()) {
            if (HasPrivate(accessor)
                && accessor.Parent?.Parent is PropertyDeclarationSyntax held
                && !Exposed(held)) {
                yield return $"line {Line(accessor)}: default modifier - never emit it";
            }
        }
    }

    static bool HasPrivate(MemberDeclarationSyntax member) =>
        member.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword));

    static bool HasPrivate(AccessorDeclarationSyntax accessor) =>
        accessor.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword));

    static bool Exposed(MemberDeclarationSyntax member) =>
        member.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)
        || m.IsKind(SyntaxKind.InternalKeyword)
        || m.IsKind(SyntaxKind.ProtectedKeyword));

    static IEnumerable<string> Groups(SyntaxNode root) {
        foreach (LambdaExpressionSyntax lambda in root.DescendantNodes().OfType<LambdaExpressionSyntax>()) {
            if (lambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword)) {
                continue;
            }
            if (Params(lambda) is not List<string> inputs) {
                continue;
            }
            if (lambda.Body is not InvocationExpressionSyntax call) {
                continue;
            }
            if (call.Expression is not IdentifierNameSyntax && call.Expression is not MemberAccessExpressionSyntax) {
                continue;
            }
            if (call.Expression is IdentifierNameSyntax direct && inputs.Contains(direct.Identifier.Text)) {
                continue;
            }
            List<string> passed = [.. call.ArgumentList.Arguments.Select(a => a.Expression.ToString())];
            if (inputs.SequenceEqual(passed)) {
                yield return $"line {Line(lambda)}: single-call lambda - use a method group";
            }
        }
    }

    static List<string>? Params(LambdaExpressionSyntax lambda) =>
        lambda switch {
            SimpleLambdaExpressionSyntax single => [single.Parameter.Identifier.Text],
            ParenthesizedLambdaExpressionSyntax listed => [.. listed.ParameterList.Parameters.Select(p => p.Identifier.Text)],
            _ => null,
        };

    static IEnumerable<string> Counts(SyntaxNode root) {
        foreach (MemberAccessExpressionSyntax access in root.DescendantNodes().OfType<MemberAccessExpressionSyntax>()) {
            if (access.Name.Identifier.Text != "Count") {
                continue;
            }
            if (access.Parent is InvocationExpressionSyntax call) {
                if (call.Expression != access || call.ArgumentList.Arguments.Any()) {
                    continue;
                }
                if (call.Parent is BinaryExpressionSyntax upper && Fold(upper, call, Line(access)) is string first) {
                    yield return first;
                }
            } else if (access.Parent is BinaryExpressionSyntax plain && Fold(plain, access, Line(access)) is string second) {
                yield return second;
            }
        }
    }

    static string? Fold(BinaryExpressionSyntax compared, ExpressionSyntax count, int line) {
        bool flipped = compared.Right == count;
        if (compared.Left != count && !flipped) {
            return null;
        }
        if ((flipped ? compared.Left : compared.Right) is not LiteralExpressionSyntax limit) {
            return null;
        }
        SyntaxKind kind = flipped ? Mirror(compared.Kind()) : compared.Kind();
        bool? any = (kind, limit.Token.Text) switch {
            (SyntaxKind.GreaterThanExpression, "0") => true,
            (SyntaxKind.GreaterThanOrEqualExpression, "1") => true,
            (SyntaxKind.NotEqualsExpression, "0") => true,
            (SyntaxKind.EqualsExpression, "0") => false,
            (SyntaxKind.LessThanOrEqualExpression, "0") => false,
            (SyntaxKind.LessThanExpression, "1") => false,
            _ => null,
        };
        return any switch {
            true => $"line {line}: count check - prefer Any()",
            false => $"line {line}: count check - prefer !Any()",
            _ => null,
        };
    }

    static SyntaxKind Mirror(SyntaxKind kind) =>
        kind switch {
            SyntaxKind.GreaterThanExpression => SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanExpression => SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression => SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.LessThanOrEqualExpression => SyntaxKind.GreaterThanOrEqualExpression,
            _ => kind,
        };

    static IEnumerable<string> TestVars(SyntaxNode root) {
        if (!Flat(root.SyntaxTree.FilePath).Contains("/test/")) {
            yield break;
        }
        foreach (LocalDeclarationStatementSyntax local in root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>()) {
            if (local.Declaration.Type.ToString() == "var" || local.Declaration.Variables.Count != 1) {
                continue;
            }
            if (local.Declaration.Type is ArrayTypeSyntax) {
                continue;
            }
            if (local.Declaration.Variables[0].Initializer?.Value is not ExpressionSyntax value) {
                continue;
            }
            if (StatesType(value)) {
                yield return $"line {Line(local)}: stated type - test locals take var";
            }
        }
    }

    static bool StatesType(ExpressionSyntax value) =>
        value switch {
            InvocationExpressionSyntax call when call.Expression is GenericNameSyntax => true,
            InvocationExpressionSyntax call => Chained(call.Expression),
            CastExpressionSyntax => true,
            MemberAccessExpressionSyntax => true,
            _ => false,
        };

    static bool Chained(ExpressionSyntax target) =>
        target switch {
            IdentifierNameSyntax => false,
            GenericNameSyntax => true,
            MemberAccessExpressionSyntax nested => nested.Expression is MemberAccessExpressionSyntax or InvocationExpressionSyntax,
            _ => false,
        };

    static IEnumerable<string> Periods(SyntaxNode root) {
        foreach (LiteralExpressionSyntax literal in root.DescendantNodes().OfType<LiteralExpressionSyntax>()) {
            if (literal.IsKind(SyntaxKind.StringLiteralExpression)
                && ErrorTail(literal.Token.ValueText)) {
                yield return $"line {Line(literal)}: trailing period - error messages should have none";
            }
        }
        foreach (ThrowStatementSyntax thrown in root.DescendantNodes().OfType<ThrowStatementSyntax>()) {
            if (Tail(thrown.Expression) is string first
                && Terse(first)
                && first.EndsWith(".", StringComparison.Ordinal)) {
                yield return $"line {Line(thrown)}: trailing period - error messages should have none";
            }
        }
        foreach (ThrowExpressionSyntax thrown in root.DescendantNodes().OfType<ThrowExpressionSyntax>()) {
            if (Tail(thrown.Expression) is string second
                && Terse(second)
                && second.EndsWith(".", StringComparison.Ordinal)) {
                yield return $"line {Line(thrown)}: trailing period - error messages should have none";
            }
        }
        foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>()) {
            if (method.Identifier.Text.StartsWith("Validate", StringComparison.Ordinal)) {
                foreach (LiteralExpressionSyntax literal in method.DescendantNodes().OfType<LiteralExpressionSyntax>()) {
                    if (literal.IsKind(SyntaxKind.StringLiteralExpression)
                        && literal.Token.ValueText.EndsWith(".", StringComparison.Ordinal)) {
                        yield return $"line {Line(literal)}: trailing period - error messages should have none";
                    }
                }
            }
        }
        foreach (InvocationExpressionSyntax call in root.DescendantNodes().OfType<InvocationExpressionSyntax>()) {
            if (call.Expression is MemberAccessExpressionSyntax made
                && (made.Name.Identifier.Text == "Add" || made.Name.Identifier.Text == "AddRange")
                && made.Expression.ToString().Contains("Error")) {
                if (Tail(call.ArgumentList.Arguments.LastOrDefault()?.Expression) is string third
                    && third.EndsWith(".", StringComparison.Ordinal)) {
                    yield return $"line {Line(call)}: trailing period - error messages should have none";
                }
            }
        }
    }

    static bool ErrorTail(string value) {
        int error = value.LastIndexOf("</error>", StringComparison.Ordinal);
        return error > 0 && value[error - 1] == '.';
    }

    static string? Tail(ExpressionSyntax? node) =>
        node?.DescendantNodesAndSelf()
        .OfType<LiteralExpressionSyntax>()
        .LastOrDefault(l => l.IsKind(SyntaxKind.StringLiteralExpression))?.Token.ValueText;

    static bool Terse(string message) =>
        !message.Contains(". ", StringComparison.Ordinal);

    static IEnumerable<string> Commas(SyntaxNode root) {
        foreach (InitializerExpressionSyntax init in root.DescendantNodes().OfType<InitializerExpressionSyntax>()) {
            if (Comma(init.OpenBraceToken, init.CloseBraceToken, init.Expressions) is string first) {
                yield return first;
            }
        }
        foreach (CollectionExpressionSyntax made in root.DescendantNodes().OfType<CollectionExpressionSyntax>()) {
            if (Comma(made.OpenBracketToken, made.CloseBracketToken, made.Elements) is string second) {
                yield return second;
            }
        }
        foreach (SwitchExpressionSyntax vexed in root.DescendantNodes().OfType<SwitchExpressionSyntax>()) {
            if (Comma(vexed.OpenBraceToken, vexed.CloseBraceToken, vexed.Arms) is string arm) {
                yield return arm;
            }
        }
        foreach (EnumDeclarationSyntax elected in root.DescendantNodes().OfType<EnumDeclarationSyntax>()) {
            if (Comma(elected.OpenBraceToken, elected.CloseBraceToken, elected.Members) is string third) {
                yield return third;
            }
        }
    }

    static string? Comma<T>(SyntaxToken open, SyntaxToken close, SeparatedSyntaxList<T> items) where T : SyntaxNode {
        bool listed = Line(open) != Line(close) && items.Any() && items.SeparatorCount != items.Count;
        return listed ? $"line {Line(close)}: multi-line list - end it with a trailing comma" : null;
    }

    static IEnumerable<string> TargetTypedNew(SyntaxNode root) {
        foreach (ObjectCreationExpressionSyntax made in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()) {
            string type = made.Type.ToString();
            SyntaxNode site = made;
            while (site.Parent is ConditionalExpressionSyntax cond
                && (cond.WhenTrue == site || cond.WhenFalse == site)) {
                site = cond;
            }
            if (site.Parent is EqualsValueClauseSyntax worth
                && worth.Parent is VariableDeclaratorSyntax declarator
                && declarator.Parent is VariableDeclarationSyntax declaration
                && (declaration.Type.ToString() == type || ArgOf(declaration.Type, type))) {
                yield return $"line {Line(made)}: repeated type - the compiler already knows it";
            } else if (site.Parent is ReturnStatementSyntax
                  && made.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault() is BaseMethodDeclarationSyntax method
                  && ReturnType(method)?.ToString() == type) {
                yield return $"line {Line(made)}: repeated type - the compiler already knows it";
            } else if (site.Parent is ReturnStatementSyntax
                  && made.Ancestors().OfType<AccessorDeclarationSyntax>().FirstOrDefault()?.Parent is PropertyDeclarationSyntax held
                  && held.Type.ToString() == type) {
                yield return $"line {Line(made)}: repeated type - the compiler already knows it";
            } else if (site.Parent is AssignmentExpressionSyntax pair
                  && pair.IsKind(SyntaxKind.SimpleAssignmentExpression)
                  && pair.Left is IdentifierNameSyntax kept
                  && ScopeType(made, kept.Identifier.Text) == type) {
                yield return $"line {Line(made)}: repeated type - the compiler already knows it";
            } else if (site.Parent is AssignmentExpressionSyntax at
                  && at.Left is ImplicitElementAccessSyntax
                  && AncestorTarget(made, type)) {
                yield return $"line {Line(made)}: repeated type - the compiler already knows it";
            }
        }
    }

    static bool ArgOf(TypeSyntax declared, string type) =>
        declared is GenericNameSyntax generic
        && generic.TypeArgumentList.Arguments.Any(a => a.ToString() == type);

    static TypeSyntax? ReturnType(BaseMethodDeclarationSyntax method) =>
        method switch {
            MethodDeclarationSyntax routine => routine.ReturnType,
            _ => null,
        };

    static string? ScopeType(ObjectCreationExpressionSyntax made, string name) {
        BaseMethodDeclarationSyntax? method = made.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
        if (method != null) {
            foreach (VariableDeclaratorSyntax declarator in method.DescendantNodes().OfType<VariableDeclaratorSyntax>()) {
                if (declarator.Identifier.Text == name
                    && declarator.Parent is VariableDeclarationSyntax declaration) {
                    return declaration.Type.ToString();
                }
            }
            foreach (ParameterSyntax given in method.ParameterList.Parameters) {
                if (given.Identifier.Text == name && given.Type != null) {
                    return given.Type.ToString();
                }
            }
        }
        TypeDeclarationSyntax? holder = made.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (holder != null) {
            foreach (MemberDeclarationSyntax member in holder.Members) {
                if (member is FieldDeclarationSyntax field
                    && field.Declaration.Variables.Any(v => v.Identifier.Text == name)) {
                    return field.Declaration.Type.ToString();
                }
            }
        }
        return null;
    }

    static bool AncestorTarget(ObjectCreationExpressionSyntax made, string type) =>
        made.Ancestors()
        .OfType<EqualsValueClauseSyntax>()
        .Select(worth => worth.Parent)
        .OfType<VariableDeclaratorSyntax>()
        .Select(declarator => declarator.Parent)
        .OfType<VariableDeclarationSyntax>()
        .Any(declaration => ArgOf(declaration.Type, type));

    static IEnumerable<string> Extensions(string path, string text) {
        List<string> subjects = [];
        Regex.Matches(text, @"extension\s*(<[^<>]*>\s*)?\(").Cast<Match>().ForEach(hit => {
            int depth = 1;
            int i = hit.Index + hit.Length;
            while (i < text.Length && depth > 0) {
                if (text[i] == '(') {
                    depth++;
                } else if (text[i] == ')') {
                    depth--;
                }
                i++;
            }
            string inner = text.Substring(hit.Index + hit.Length, i - hit.Index - hit.Length - 1);
            string first = inner.Split([',', ' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
            string subject = first.Split('<')[0].Split('.').Last();
            if (subject.Length > 0 && !subjects.Contains(subject)) {
                subjects.Add(subject);
            }
        });
        if (subjects.Any()
            && (subjects.Count > 1 || SubjectFile(subjects[0]) != Path.GetFileName(path))) {
            yield return $"extension block - one subject per file, this one holds {subjects.JoinToString()}";
        }
    }

    static string SubjectFile(string subject) =>
        (subject switch {
            "int" => "IntExtensions.cs",
            "string" => "StringExtensions.cs",
            "bool" => "BoolExtensions.cs",
            "char" => "CharExtensions.cs",
            "double" => "DoubleExtensions.cs",
            "float" => "FloatExtensions.cs",
            "long" => "LongExtensions.cs",
            "object" => "ObjectExtensions.cs",
            "IEnumerable" => "EnumerableExtensions.cs",
            _ => $"{subject}Extensions.cs",
        });

    record Edit(int Start, int End, string Text);

    static string ApplyFixes(string path, string text) {
        SyntaxNode root = CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.Preview)).GetRoot();
        List<Edit> edits = [.. InterpEdits(root).Concat(ArrowEdits(root))];
        if (!edits.Any()) {
            return text;
        }
        edits.OrderByDescending(e => e.Start).ForEach(edit => {
            text = text[..edit.Start] + edit.Text + text[edit.End..];
        });
        File.WriteAllText(path, text);
        Console.WriteLine($"    {path}: fixed {edits.Count}");
        return text;
    }

    static IEnumerable<string> Interpolation(SyntaxNode root) {
        foreach (BinaryExpressionSyntax top in root.DescendantNodes().OfType<BinaryExpressionSyntax>()) {
            if (IsConcat(top)) {
                yield return $"line {Line(top)}: string concat - compose it with interpolation";
            }
        }
    }

    static bool IsConcat(BinaryExpressionSyntax top) =>
        Claimed(top)
        && !top.Ancestors().OfType<BinaryExpressionSyntax>().Any(Claimed);

    static bool Claimed(BinaryExpressionSyntax top) =>
        top.IsKind(SyntaxKind.AddExpression)
        && !SkippedContext(top) && !HasComment(top) && !HasVerbatim(top)
        && Operands(top).Any(HasString);

    static List<ExpressionSyntax> Operands(ExpressionSyntax node) =>
        node is BinaryExpressionSyntax join
        && join.IsKind(SyntaxKind.AddExpression)
        ? [.. Operands(join.Left), join.Right]
        : [node];

    static bool HasString(ExpressionSyntax part) =>
        part switch {
            LiteralExpressionSyntax literal => literal.IsKind(SyntaxKind.StringLiteralExpression),
            InterpolatedStringExpressionSyntax => true,
            _ => false,
        };

    static bool SkippedContext(SyntaxNode node) =>
        node.Ancestors().Any(a => a is AttributeSyntax || a is SwitchLabelSyntax)
        || node.Ancestors().OfType<EqualsValueClauseSyntax>().Any(e => e.Parent is ParameterSyntax);

    static bool HasComment(SyntaxNode node) =>
        node.DescendantTrivia().Any(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia)
        || t.IsKind(SyntaxKind.MultiLineCommentTrivia)
        || t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
        || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));

    static bool HasVerbatim(SyntaxNode node) =>
        node.DescendantNodesAndSelf()
        .OfType<LiteralExpressionSyntax>()
        .Any(l => l.Token.Text.StartsWith("@\"") || l.Token.Text.StartsWith("$@") || l.Token.Text.StartsWith("@$"))
        || node.DescendantNodesAndSelf()
        .OfType<InterpolatedStringExpressionSyntax>()
        .Any(s => s.ToString().StartsWith("$@") || s.ToString().StartsWith("@$"));

    static IEnumerable<Edit> InterpEdits(SyntaxNode root) {
        foreach (BinaryExpressionSyntax top in root.DescendantNodes().OfType<BinaryExpressionSyntax>()) {
            if (!IsConcat(top)) {
                continue;
            }
            if (Build(top) is string text) {
                yield return new Edit(top.SpanStart, top.Span.End, text);
            }
        }
    }

    static string? Build(BinaryExpressionSyntax top) {
        List<string> holes = [];
        foreach (ExpressionSyntax part in Operands(top)) {
            if (part is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression)) {
                holes.Add(Escape(literal.Token.Text));
            } else if (part is InterpolatedStringExpressionSyntax already) {
                if (Merge(already) is string merged) {
                    holes.Add(merged);
                } else {
                    return null;
                }
            } else {
                holes.Add($"{{{part}}}");
            }
        }
        return $"$\"{string.Concat(holes)}\"";
    }

    static string Escape(string raw) {
        int open = raw.IndexOf('"');
        string inner = raw.Substring(open + 1, raw.LastIndexOf('"') - open - 1);
        return inner.Replace("{", "{{").Replace("}", "}}");
    }

    static string? Merge(InterpolatedStringExpressionSyntax already) {
        string raw = already.ToString();
        if (raw.StartsWith("$@") || raw.StartsWith("@$")) {
            return null;
        }
        int open = raw.IndexOf('"');
        return raw.Substring(open + 1, raw.LastIndexOf('"') - open - 1);
    }

    static IEnumerable<string> JoinToString(SyntaxNode root) {
        if (Flat(root.SyntaxTree.FilePath).Contains("/EnumerableExtensions.cs/")) {
            yield break;
        }
        foreach (InvocationExpressionSyntax call in root.DescendantNodes().OfType<InvocationExpressionSyntax>()) {
            if (call.Expression is MemberAccessExpressionSyntax made
                && made.Name.Identifier.Text == "Join"
                && (made.Expression is IdentifierNameSyntax held && (held.Identifier.Text == "string" || held.Identifier.Text == "String")
                || made.Expression is PredefinedTypeSyntax keyword && keyword.Keyword.Text == "string")) {
                yield return $"line {Line(call)}: string.Join - let the subject lead with JoinToString";
            }
        }
    }

    static IEnumerable<string> AppendChain(SyntaxNode root) {
        foreach (BlockSyntax block in root.DescendantNodes().OfType<BlockSyntax>()) {
            for (int i = 0; i + 1 < block.Statements.Count; i++) {
                if (block.Statements[i] is ExpressionStatementSyntax first
                    && first.Expression is InvocationExpressionSyntax a
                    && a.Expression is MemberAccessExpressionSyntax one
                    && one.Name.Identifier.Text == "Append"
                    && block.Statements[i + 1] is ExpressionStatementSyntax second
                    && second.Expression is InvocationExpressionSyntax b
                    && b.Expression is MemberAccessExpressionSyntax two
                    && two.Name.Identifier.Text == "Append"
                    && two.Expression.ToString() == one.Expression.ToString()) {
                    yield return $"line {Line(second)}: chainable Append - join the chain";
                }
            }
        }
    }

    static IEnumerable<string> OpenTypes(SyntaxNode root) {
        foreach (TypeDeclarationSyntax type in root.DescendantNodes().OfType<TypeDeclarationSyntax>()) {
            if (type.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword))) {
                yield return $"line {Line(type)}: sealed type - library code stays open";
            }
        }
    }

    static IEnumerable<string> TernaryHint(SyntaxNode root) {
        foreach (IfStatementSyntax ask in root.DescendantNodes().OfType<IfStatementSyntax>()) {
            if (ask.Statement is BlockSyntax then
                && then.Statements.Count == 1
                && then.Statements[0] is ExpressionStatementSyntax first
                && first.Expression is InvocationExpressionSyntax a
                && a.ArgumentList.Arguments.Count == 1
                && ask.Else is ElseClauseSyntax els
                && els.Statement is BlockSyntax other
                && other.Statements.Count == 1
                && other.Statements[0] is ExpressionStatementSyntax second
                && second.Expression is InvocationExpressionSyntax b
                && b.ArgumentList.Arguments.Count == 1
                && a.Expression.ToString() == b.Expression.ToString()
                && a.ArgumentList.Arguments[0].ToString() != b.ArgumentList.Arguments[0].ToString()
                && ArgCompatible(a.ArgumentList.Arguments[0], b.ArgumentList.Arguments[0])) {
                yield return $"line {Line(ask)}: consider ternary - {a.Expression}({ask.Condition} ? {a.ArgumentList.Arguments[0]} : {b.ArgumentList.Arguments[0]})";
            }
        }
    }

    static bool ArgCompatible(ArgumentSyntax x, ArgumentSyntax y) =>
        x.Expression is not LiteralExpressionSyntax || y.Expression is LiteralExpressionSyntax or InterpolatedStringExpressionSyntax;

    static IEnumerable<string> PartialType(SyntaxNode root) {
        bool godot = root is CompilationUnitSyntax unit
            && unit.Usings.Any(u => u.Name.ToString() == "Godot");
        foreach (TypeDeclarationSyntax type in root.DescendantNodes().OfType<TypeDeclarationSyntax>()) {
            if (!type.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))) {
                continue;
            }
            if (godot && type.BaseList?.Types.Any() == true) {
                continue;
            }
            yield return $"line {Line(type)}: partial type - banned unless tooling needs it";
        }
    }

    static IEnumerable<string> ArrowBreak(SyntaxNode root) {
        foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>()) {
            if (method.ExpressionBody is ArrowExpressionClauseSyntax arrow
                && Line(arrow.ArrowToken) == Line(arrow.Expression)) {
                yield return $"line {Line(arrow.ArrowToken)}: arrow body - start it on the next line";
            }
        }
        foreach (LocalFunctionStatementSyntax local in root.DescendantNodes().OfType<LocalFunctionStatementSyntax>()) {
            if (local.ExpressionBody is ArrowExpressionClauseSyntax arrow
                && Line(arrow.ArrowToken) == Line(arrow.Expression)) {
                yield return $"line {Line(arrow.ArrowToken)}: arrow body - start it on the next line";
            }
        }
    }

    static IEnumerable<Edit> ArrowEdits(SyntaxNode root) {
        foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>()) {
            if (method.ExpressionBody is ArrowExpressionClauseSyntax arrow
                && Line(arrow.ArrowToken) == Line(arrow.Expression)) {
                yield return ArrowEdit(arrow);
            }
        }
        foreach (LocalFunctionStatementSyntax local in root.DescendantNodes().OfType<LocalFunctionStatementSyntax>()) {
            if (local.ExpressionBody is ArrowExpressionClauseSyntax arrow
                && Line(arrow.ArrowToken) == Line(arrow.Expression)) {
                yield return ArrowEdit(arrow);
            }
        }
    }

    static Edit ArrowEdit(ArrowExpressionClauseSyntax arrow) =>
        new(
        arrow.ArrowToken.Span.End,
        arrow.Expression.SpanStart,
        $"\n{Indent(arrow.ArrowToken)}");

    static string Indent(SyntaxToken token) {
        string line = token.SyntaxTree.GetText().Lines[token.GetLocation().GetLineSpan().StartLinePosition.Line].ToString();
        int i = 0;
        while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) {
            i++;
        }
        return $"{line[..i]}    ";
    }
}

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GDUTSharp.SourceGen.Generators
{
    [Generator]
    public class Gen_OverrideToString : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.ForAttributeWithMetadataName
                (
                    GenConstant.OverrideToString,
                    static (node, token) => node is ClassDeclarationSyntax or StructDeclarationSyntax,
                    (GeneratorAttributeSyntaxContext syntaxContext, CancellationToken token) => GetTypeToGenerate(syntaxContext)
                ).Where(static m => m is not null);
            context.RegisterSourceOutput(provider, static (spc, typeInfo) =>
                ExecuteGeneration(spc, typeInfo));
        }

        private class TypeToGenerate(
            string nameSpace,
            string typeName,
            TypeKind typeKind,
            ImmutableArray<(string Name, bool IsCollection)> propertyInfos,
            string? displayName)
        {
            public string Namespace { get; set; } = nameSpace;
            public string TypeName { get; set; } = typeName;
            public string TypeKind { get; set; } = typeKind switch
            {
                Microsoft.CodeAnalysis.TypeKind.Class => "class",
                Microsoft.CodeAnalysis.TypeKind.Struct => "struct",
                _ => throw new InvalidDataException()
            };
            public ImmutableArray<(string Name, bool IsCollection)> PropertyInfos { get; set; } = propertyInfos;
            public string DisplayName { get; set; } = displayName ?? typeName;
        }

        private static TypeToGenerate? GetTypeToGenerate(GeneratorAttributeSyntaxContext context)
        {
            if (context.TargetSymbol is not INamedTypeSymbol typeSymbol) return null;
            var properties = typeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
                .Select(p => (p.Name, IsCollectionLike(p.Type, context.SemanticModel.Compilation)))
                .ToImmutableArray();
            string? displayName = null;
            foreach (var attribute in context.Attributes)
            {
                if (attribute.ConstructorArguments.Length > 0)
                {
                    var arg = attribute.ConstructorArguments[0];
                    if (arg.Value is string name)
                    {
                        displayName = name;
                        break;
                    }
                }
                foreach (var namedArg in attribute.NamedArguments)
                {
                    if (namedArg.Key == "DisplayName" &&
                        namedArg.Value.Value is string name)
                    {
                        displayName = name;
                        break;
                    }
                }

                if (displayName is not null)
                    break;
            }
            return new TypeToGenerate(
                typeSymbol.ContainingNamespace.ToDisplayString(),
                typeSymbol.Name,
                typeSymbol.TypeKind,
                properties,
                displayName);
        }

        private static bool IsCollectionLike(ITypeSymbol type, Compilation compilation)
        {
            if (type.SpecialType == SpecialType.System_String) return false;
            if (type is IArrayTypeSymbol) return true;
            var iEnumerableT = compilation.GetTypeByMetadataName(GenConstant.IEnumerableT);
            var iEnumerable = compilation.GetTypeByMetadataName(GenConstant.IEnumerable);
            foreach (var item in type.AllInterfaces)
            {
                var symbol = item.OriginalDefinition;
                if (symbol is null) continue;
                if (SymbolEqualityComparer.Default.Equals(symbol, iEnumerableT)
                    || SymbolEqualityComparer.Default.Equals(symbol, iEnumerable)) return true;
            }
            return false;
        }

        private static void ExecuteGeneration(SourceProductionContext context, TypeToGenerate? typeInfo)
        {
            if (typeInfo is null) return;
            var sb = new StringBuilder();
            sb.Append($"            {typeInfo.DisplayName}:");
            for (int i = 0; i < typeInfo.PropertyInfos.Length; i++)
            {
                var (name, isCollection) = typeInfo.PropertyInfos[i];
                if (isCollection)
                {
                    sb.Append($$"""{{"\n"}}              - {{name}}:[{string.Join(",", {{name}})}]""");
                }
                else
                {
                    sb.Append($$"""{{"\n"}}              - {{name}}:{{{name}}}""");
                }
            }
            var content = sb.ToString();
            sb.Clear();
            sb.AppendLine($$""""
                // <auto-generated/>
                namespace {{typeInfo.Namespace}}
                {
                    partial {{typeInfo.TypeKind}} {{typeInfo.TypeName}}
                    {
                        /// <summary>此 <see cref="ToString"/> 已由 GDUTSharp.SourceGen 自动覆写</summary>
                        public override string ToString() {
                            return $"""
                {{content}}
                            """;
                        }
                    }
                }
                """");
            context.AddSource($"{typeInfo.TypeName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }
}
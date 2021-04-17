using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorRoutes
{
    [Generator]
    public class RoutesClassGenerator : ISourceGenerator
    {
        private const string RouteAttributeName = "Microsoft.AspNetCore.Components.RouteAttribute";

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var pageRoutes = GetPageRoutes(context.Compilation);
            string sourceCode = GenerateCode(pageRoutes);

            context.AddSource("Routes", sourceCode);
        }

        private static IEnumerable<PageRoutes> GetPageRoutes(Compilation compilation)
        {
            return compilation.SyntaxTrees
                              .SelectMany(s => s.GetRoot().DescendantNodes())
                              .Where(d => d.IsKind(SyntaxKind.ClassDeclaration))
                              .Cast<ClassDeclarationSyntax>()
                              .Where(c => c.AttributeLists
                                           .SelectMany(x => x.Attributes)
                                           .Any(attr => attr.Name.ToString() == RouteAttributeName))
                              .Select(component => new PageRoutes()
                              {
                                  PageName = component.Identifier.ToString(),
                                  Routes = GetRoutes(compilation, component),
                              });
        }

        private static IEnumerable<Route> GetRoutes(Compilation compilation, ClassDeclarationSyntax component)
        {
            var semanticModel = compilation.GetSemanticModel(component.SyntaxTree);

            return component.AttributeLists
                            .SelectMany(x => x.Attributes)
                            .Where(attribute => attribute.Name.ToString() == RouteAttributeName)
                            .Select(attribute =>
                            {
                                var argument = attribute.ArgumentList!.Arguments[0];
                                string path = semanticModel.GetConstantValue(argument.Expression).ToString();
                                return new Route(path);
                            });
        }

        private static string GenerateCode(IEnumerable<PageRoutes> pageRoutes)
        {
            const string Inline = "    ";
            var builder = new StringBuilder(512);

            builder.Append(
@"using System;
using System.Collections.Generic;
using System.Globalization;

#nullable enable

public static class Routes
{
");
            bool firstRoute = true;

            foreach (var page in pageRoutes)
            {
                foreach (var route in page.Routes)
                {
                    if (!firstRoute)
                    {
                        builder.AppendLine();
                    }
                    else
                    {
                        firstRoute = false;
                    }

                    builder.Append(Inline)
                           .Append("public static string ")
                           .Append(page.PageName)
                           .Append('(');

                    bool firstParameter = true;
                    foreach (var parameter in route.Parameters)
                    {
                        if (!firstParameter)
                        {
                            builder.Append(", ");
                        }
                        else
                        {
                            firstParameter = false;
                        }

                        if (parameter.IsOptional)
                        {
                            builder.Append(parameter.Type)
                                   .Append("? ")
                                   .Append(parameter.Name)
                                   .Append(" = null");
                        }
                        else
                        {
                            builder.Append(parameter.Type)
                                   .Append(" ")
                                   .Append(parameter.Name);
                        }
                    }

                    if (route.Parameters.Count > 0)
                    {
                        builder.AppendLine(")")
                               .Append(Inline)
                               .AppendLine("{")
                               .Append(Inline).Append(Inline)
                               .Append("return TrimEndSlash(string.Format(CultureInfo.InvariantCulture, \"")
                               .Append(route.FormatPattern)
                               .Append("\"");

                        foreach (var parameter in route.Parameters)
                        {
                            builder.Append(", ");

                            string parameterExpression = parameter.Name;

                            switch (parameter.Type)
                            {
                                case "bool":
                                    AddNullCheckingBegin();
                                    builder.Append(parameterExpression)
                                           .Append(" ? \"true\" : \"false\"");
                                    AddNullCheckingEnd();
                                    break;

                                case nameof(DateTime):
                                    AddNullCheckingBegin();
                                    builder.Append(parameterExpression)
                                           .Append(".TimeOfDay == TimeSpan.Zero ? ")
                                           .Append(parameterExpression)
                                           .Append(".ToString(\"yyyy-MM-dd\")")
                                           .Append(" : ")
                                           .Append(parameterExpression)
                                           .Append(".ToString(\"yyyy-MM-dd'T'HH':'mm':'ss\")");
                                    AddNullCheckingEnd();
                                    break;

                                default:
                                    builder.Append(parameterExpression);
                                    break;
                            }

                            void AddNullCheckingBegin()
                            {
                                if (parameter.IsOptional)
                                {
                                    builder.Append(parameter.Name);
                                    builder.Append(" != null ? (");
                                    parameterExpression += ".Value";
                                }
                            }

                            void AddNullCheckingEnd()
                            {
                                if (parameter.IsOptional)
                                {
                                    builder.Append(") : null");
                                }
                            }
                        }

                        builder.AppendLine("));")
                               .Append(Inline)
                               .Append("}");
                    }
                    else
                    {
                        builder.Append(") => \"")
                               .Append(route.Path)
                               .Append("\";");
                    }

                    builder.AppendLine();
                }
            }

            builder.AppendLine()
                   .Append(Inline)
                   .AppendLine("private static string TrimEndSlash(string path)")
                   .Append(Inline)
                   .AppendLine("{")
                   .Append(Inline).Append(Inline)
                   .AppendLine("return (path.EndsWith(\"/\") && path.Length > 1 ? path.Remove(path.Length - 1) : path);")
                   .Append(Inline)
                   .AppendLine("}");

            builder.AppendLine("}")
                   .AppendLine()
                   .AppendLine("#nullable restore");

            return builder.ToString();
        }
    }
}

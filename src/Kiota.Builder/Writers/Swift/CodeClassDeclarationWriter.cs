using System;
using System.Linq;
using System.Collections.Generic;
using Kiota.Builder.Extensions;

namespace Kiota.Builder.Writers.Swift;
public class CodeClassDeclarationWriter : BaseElementWriter<ClassDeclaration, SwiftConventionService>
{
    public CodeClassDeclarationWriter(SwiftConventionService conventionService): base(conventionService) { }
    public override void WriteCodeElement(ClassDeclaration codeElement, LanguageWriter writer)
    {
        if(codeElement == null) throw new ArgumentNullException(nameof(codeElement));
        if(writer == null) throw new ArgumentNullException(nameof(writer));
        if (codeElement.Parent?.Parent is CodeNamespace ns)
        {
            var importSegments = codeElement
                                    .Usings
                                    .Where(x => x.Declaration.IsExternal)
                                    .Select(x => x.Declaration.Name)
                                    .Distinct()
                                .OrderBy(x => x.Count(y => y == '.'))
                                .ThenBy(x => x)
                                .ToList();
            if (importSegments.Any())
            {
                importSegments.ForEach(x => writer.WriteLine($"import {x}"));
                writer.WriteLine(string.Empty);
            }
        }

        if(codeElement?.Parent?.Parent is CodeNamespace && !(codeElement.Parent is CodeClass currentClass && currentClass.IsOfKind(CodeClassKind.BarrelInitializer))) {
            writer.WriteLine($"extension {codeElement.Parent.Parent.Name} {{");
            writer.IncreaseIndent();
        }

        var derivedTypes = new List<string>{codeElement.Inherits?.Name}
                                        .Union(codeElement.Implements.Select(x => x.Name))
                                        .Where(x => x != null);
        var derivation = derivedTypes.Any() ? ": " +  derivedTypes.Select(x => x.ToFirstCharacterUpperCase()).Aggregate((x, y) => $"{x}, {y}") + " " : string.Empty;
        if(codeElement.Parent is CodeClass parentClass)
            conventions.WriteShortDescription(parentClass.Description, writer);
        writer.WriteLine($"public class {codeElement.Name.ToFirstCharacterUpperCase()} {derivation}{{");
        writer.IncreaseIndent();
    }
}

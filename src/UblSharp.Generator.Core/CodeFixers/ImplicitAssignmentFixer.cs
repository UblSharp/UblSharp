using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;
using Microsoft.CSharp;
using UblSharp.Generator.Extensions;

namespace UblSharp.Generator.CodeFixers
{
    /// <summary>
    /// Generate implicit assignment for types that have
    /// - XmlTextAttribute on a member
    /// - Inherit from a type with the above condition
    ///
    /// Do not generate imlicit assignment for types that have
    /// - xsd use="required" on any member
    /// - inherit from a type with the above condition
    /// - is abstract
    /// </summary>
    public class ImplicitAssignmentFixer : CodeNamespaceVisitor
    {
        private static CSharpCodeProvider _codeProvider = new CSharpCodeProvider();

        private class MemberTypeTuple
        {
            public Type BaseMemberType;
            public CodeTypeDeclaration CodeDecl;
        }

        protected override void VisitNamespace(CodeNamespace codeNamespace)
        {
            // Ignore TimeType and DateType .Value
            // do this before adding the implicit operators, because we will have our own implementation in the partial class
            var fields = codeNamespace.Types.Cast<CodeTypeDeclaration>().SelectMany(x => x.Members.OfType<CodeMemberField>().Select(f => new { Type = x, Field = f }));
            foreach (var fieldt in fields)
            {
                var type = fieldt.Type;
                var member = fieldt.Field;

                // Ignore Value of TimeType
                if ((type.Name == "TimeType" || type.Name == "DateType")
                    && member.Name == "Value")
                {
                    var attributes = member.CustomAttributes.Cast<CodeAttributeDeclaration>().ToList();
                    var attr = attributes.Single(x => x.Name == "System.Xml.Serialization.XmlTextAttribute");
                    member.CustomAttributes.Remove(attr);
                    member.CustomAttributes.Add(new CodeAttributeDeclaration("System.Xml.Serialization.XmlIgnoreAttribute"));
                    member.Type = new CodeTypeReference(typeof(System.DateTimeOffset));
                }
            }

            var classes = codeNamespace.Types.Cast<CodeTypeDeclaration>().Where(c => c.IsClass).ToList();

            var codeDeclsBase = classes.Where(c =>
                    !c.HasAnyRequiredMembers() &&
                    c.Members.OfType<CodeTypeMember>().Any(m => m.Name == "Value"
                        && (m.Attributes.HasFlag(MemberAttributes.Public))
                        && m.HasAnyXmlTextAttribute())
                    ).ToList();

            var accumulatedTupleList = (from cdecl in codeDeclsBase
                                        let xmlMember = cdecl.GetXmlSchemaType() as XmlSchemaComplexType
                                        where xmlMember.BaseXmlSchemaType.Datatype != null
                                        select new MemberTypeTuple { CodeDecl = cdecl, BaseMemberType = xmlMember.BaseXmlSchemaType.Datatype.ValueType }).ToList();

            IEnumerable<MemberTypeTuple> decendantsAtNextLevel = accumulatedTupleList;

            do
            {
                var baseTypeNameFilter = decendantsAtNextLevel.Select(d => d.CodeDecl.Name).ToArray();
                decendantsAtNextLevel = classes
                    .Where(c =>
                        c.BaseTypes.Count > 0 &&
                        baseTypeNameFilter.Contains(c.BaseTypes[0].BaseType) &&
                        !c.HasAnyRequiredMembers()
                        )
                    .Select(c => new MemberTypeTuple
                    {
                        BaseMemberType = decendantsAtNextLevel.Where(d => d.CodeDecl.Name == c.BaseTypes[0].BaseType).Select(d => d.BaseMemberType).First(),
                        CodeDecl = c
                    }).ToList();
                accumulatedTupleList.AddRange(decendantsAtNextLevel);
            } while (decendantsAtNextLevel.Any());

            // Dont generate implicit assignment if the type of Value property doesn't match
            foreach (var lowLevelAbstractTuple in accumulatedTupleList.ToList())
            {
                var valueMember = lowLevelAbstractTuple.CodeDecl.Members.OfType<CodeTypeMember>().FirstOrDefault(x => x.Name == "Value");
                if (valueMember == null) continue;

                var valueType = (valueMember as CodeMemberProperty)?.Type ?? (valueMember as CodeMemberField)?.Type;
                if (valueType == null) continue;

                string valueTypeName = valueType.BaseType;
                if (valueType.ArrayRank > 0)
                {
                    valueTypeName = valueTypeName + "[]";
                }

                if (valueTypeName.ToLowerInvariant() != lowLevelAbstractTuple.BaseMemberType?.FullName?.ToLowerInvariant())
                {
                    accumulatedTupleList.Remove(lowLevelAbstractTuple);
                }
            }

            // Dont generate implicit assignment for abstract types
            foreach (var lowLevelAbstractTuple in accumulatedTupleList.Where(t => t.CodeDecl.TypeAttributes.HasFlag(TypeAttributes.Abstract)).ToList())
            {
                accumulatedTupleList.Remove(lowLevelAbstractTuple);
            }

            foreach (var tuple in accumulatedTupleList)
            {
                AddStaticImplicitAssignmentOperators(tuple.CodeDecl, tuple.BaseMemberType, "Value");
            }
        }

        /// <summary>
        /// 0 = value == null (for strings and reference types)
        /// 1 = ubltype
        /// 2 = clrtype
        /// 3 = property name ("Value")
        /// </summary>
        private static readonly string ImplicitAssignCodeStringFormat =
@"        public static implicit operator {1}({2} value)
        {{
             return {0}new {1} {{ {3} = value }};
        }}

        public static implicit operator {2}({1} value)
        {{
             return value.{3};
        }}" + Environment.NewLine;

        private static void AddStaticImplicitAssignmentOperators(CodeTypeDeclaration codeDecl, Type parameterType, string propName)
        {
            var nullTest = parameterType == typeof(string) || parameterType.IsByRef ? "value == null ? null : " : "";
            var typeRef = new CodeTypeReference(parameterType);
            var snipCodeString = string.Format(ImplicitAssignCodeStringFormat, nullTest, codeDecl.Name, _codeProvider.GetTypeOutput(typeRef), propName);
            var codeSnippet = new CodeSnippetTypeMember(snipCodeString);
            codeDecl.Members.Add(codeSnippet);
        }
    }
}

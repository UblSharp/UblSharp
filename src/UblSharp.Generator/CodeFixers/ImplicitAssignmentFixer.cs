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
            var classes = codeNamespace.Types.Cast<CodeTypeDeclaration>().Where(c => c.IsClass).ToList();

            var codeDeclsBase = classes.Where(c =>
                    !c.HasAnyRequiredMembers() &&
                    c.Members.OfType<CodeTypeMember>().Any(m =>
                        (m is CodeMemberField || m is CodeMemberProperty)
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
                        BaseMemberType = decendantsAtNextLevel.Where(d => d.CodeDecl.Name == c.BaseTypes[0].BaseType).Select(d => d.BaseMemberType).Single(),
                        CodeDecl = c
                    }).ToList();
                accumulatedTupleList.AddRange(decendantsAtNextLevel);
            } while (decendantsAtNextLevel.Any());

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
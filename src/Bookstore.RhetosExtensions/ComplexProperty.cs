using System.ComponentModel.Composition;
using Rhetos.Compiler;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Extensibility;

namespace Bookstore.RhetosExtensions
{
    [Export(typeof(IConceptInfo))]
    [ConceptKeyword("ComplexProperty")]
    public class ComplexPropertyInfo : PropertyInfo
    {
        public DataStructureInfo PropertyType { get; set; }
    }

    [Export(typeof(IConceptCodeGenerator))]
    [ExportMetadata(MefProvider.Implements, typeof(ComplexPropertyInfo))]
    public class ComplexPropertyCodeGenerator : IConceptCodeGenerator
    {
        public void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {
            var info = (ComplexPropertyInfo)conceptInfo;

            string typeName = info.PropertyType.Module.Name + "." + info.PropertyType.Name;

            if (DslUtility.IsQueryable(info.PropertyType))
                GeneratePropertyWithSimpleSerializer(codeBuilder, info, typeName);
            else
                PropertyHelper.GenerateCodeForType((PropertyInfo)conceptInfo, codeBuilder, typeName);
        }

        /// <summary>
        /// This will suppress some issues with the property serialization in web response:
        /// Most serializers are trying to serialize complete instance type, which can sometimes be a queryable class or an Entity Framework proxy class
        /// but we only need to serialize the base properties from the simple class.
        /// </summary>
        /// <remarks>
        /// As the most common JSON serializers for .NET are Newtonsoft.Json and System.Text.Json we are supporing both of them in the generated code.
        /// </remarks>
        private static void GeneratePropertyWithSimpleSerializer(ICodeBuilder codeBuilder, ComplexPropertyInfo info, string typeName)
        {
            string querybleTypeName = "Common.Queryable." + info.PropertyType.Module.Name + "_" + info.PropertyType.Name;

            string propertySnippet =
    $@"[Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public {typeName} {info.Name} {{ get; set; }}

        [Newtonsoft.Json.JsonProperty(""{info.Name}"")]
        [System.Text.Json.Serialization.JsonPropertyName(""{info.Name}"")]
        public {typeName} {info.Name}_ToSimple
        {{
            get {{ return {info.Name} is {querybleTypeName} ? (({querybleTypeName}){info.Name}).ToSimple() : {info.Name}; }}
            private set {{ {info.Name} = value; }}
        }}
        ";
            codeBuilder.InsertCode(propertySnippet, DataStructureCodeGenerator.BodyTag, info.DataStructure);

            if (DslUtility.IsQueryable(info.DataStructure))
                codeBuilder.InsertCode(
                    string.Format(",\r\n                {0} = item.{0}", info.Name),
                    RepositoryHelper.AssignSimplePropertyTag, info.DataStructure);
        }
    }
}

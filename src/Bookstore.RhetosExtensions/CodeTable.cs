using System.Collections.Generic;
using System.ComponentModel.Composition;
using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;

namespace Bookstore.RhetosExtensions
{
    /// <summary>
    /// CodeTable is a custom DSL concept that generates the following 4 DSL statements on an entity:
    /// <code>
    /// ShortString Code { AutoCode; }
    /// ShortString Name { Required; }
    /// </code> 
    /// </summary>
    [Export(typeof(IConceptInfo))]
    [ConceptKeyword("CodeTable")]
    public class CodeTableInfo : IConceptInfo
    {
        [ConceptKey]
        public EntityInfo Entity { get; set; }
    }

    [Export(typeof(IConceptMacro))]
    public class CodeTableMacro : IConceptMacro<CodeTableInfo>
    {
        public IEnumerable<IConceptInfo> CreateNewConcepts(CodeTableInfo conceptInfo, IDslModel existingConcepts)
        {
            var newConcepts = new List<IConceptInfo>();

            var code = new ShortStringPropertyInfo
            {
                DataStructure = conceptInfo.Entity,
                Name = "Code"
            };
            newConcepts.Add(code);

            newConcepts.Add(new AutoCodePropertyInfo
            {
                Property = code
            });

            var name = new ShortStringPropertyInfo
            {
                DataStructure = conceptInfo.Entity,
                Name = "Name"
            };
            newConcepts.Add(name);

            newConcepts.Add(new RequiredPropertyInfo
            {
                Property = name
            });

            return newConcepts;
        }
    }
}

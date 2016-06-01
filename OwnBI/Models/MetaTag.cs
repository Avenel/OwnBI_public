using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.Models
{
    [ElasticsearchType(IdProperty = "Id", Name = "meta_tag")]
    public class MetaTag
    {
        
            [String(Name = "id", Index = FieldIndexOption.NotAnalyzed)]
            public Guid? Id { get; set; }

            [String(Name = "name", Index = FieldIndexOption.Analyzed)]
            public string Name { get; set; }

            [String(Name = "description", Index = FieldIndexOption.Analyzed)]
            public string Description { get; set; }

            public override string ToString()
            {
                return string.Format("Id: '{0}', Name: '{1}', Description: '{2}'", Id, Name, Description);
            }
    }
}
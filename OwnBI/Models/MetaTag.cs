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

            [String(Name = "dataType", Index = FieldIndexOption.Analyzed)]
            public string DataType { get; set; } // string, number, object, date, datetime

            public override string ToString()
            {
                return string.Format("Id: '{0}', Name: '{1}', Description: '{2}', DataType: '{3}'", Id, Name, Description, DataType);
            }
    }
}
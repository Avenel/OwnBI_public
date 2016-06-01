using Nest;
using Newtonsoft.Json;
using OwnBI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.Models
{
    [ElasticsearchType(IdProperty = "Id", Name = "doc_type")]
    public class DocType
    {
        [String(Name = "id", Index = FieldIndexOption.NotAnalyzed)]
        public Guid? Id { get; set; }

        [String(Name = "name", Index = FieldIndexOption.Analyzed)]
        public string Name { get; set; }

        [String(Name = "description", Index = FieldIndexOption.Analyzed)]
        public string Description { get; set; }

        [String(Name = "meta_tags", Index = FieldIndexOption.Analyzed)]
        public List<Guid> MetaTags{ get; set; }

        public override string ToString()
        {
            var metaTags = MetaTagRepository.ReadMany(MetaTags);
            var metaTagsString = JsonConvert.SerializeObject(metaTags);
            return string.Format("Id: '{0}', Name: '{1}', Description: '{2}', MetaTags: '{3}'", Id, Name, Description, metaTagsString);
        }
    }
}
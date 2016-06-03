using Elasticsearch.Net;
using Nest;
using OwnBI.DataAccess;
using OwnBI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.Repositories
{
    public static class MetaTagRepository
    {
        public static List<MetaTag> Index()
        {
            var res = ElasticClientFactory.Client.Search<MetaTag>(s => s
               .From(0)
               .Size(100)
               .Query(q => q.MatchAll())
               .Sort(o => o.Ascending(f => f.Name)));

            var list = new List<MetaTag>();
            if (res.Total > 0)
            {
                list = res.Hits.Select(h => h.Source).ToList();
            }

            return list;
        }

        public static MetaTag Create(string name, string description, string dataType)
        {
            var metaTag = new MetaTag()
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                DataType = dataType
            };

            var response = ElasticClientFactory.Client.Index(metaTag);
            ElasticClientFactory.Client.Refresh("metatags");
            return metaTag;
        }

        public static MetaTag Read(Guid id)
        {
            var docSearch = ElasticClientFactory.Client.Get<MetaTag>(id);

            if (docSearch.Found)
            {
                return docSearch.Source;
            }
            throw new KeyNotFoundException();
        }

        public static List<MetaTag> ReadMany(List<Guid> ids)
        {
            var list = new List<MetaTag>();

            foreach (var id in ids)
            {
                list.Add(MetaTagRepository.Read(id));
            }

            return list;
        }

        public static MetaTag Update(Guid id, string name, string description, string dataType)
        {

            var doc = MetaTagRepository.Read(id);
            doc.Name = name;
            doc.Description = description;
            doc.DataType = dataType;

            var response = ElasticClientFactory.Client.Index(doc);
            ElasticClientFactory.Client.Refresh("metatags");
            return doc;

        }

        public static MetaTag Delete(Guid id)
        {
            var doc = MetaTagRepository.Read(id);
            var resDel = ElasticClientFactory.Client
                .Delete<MetaTag>(doc);

            ElasticClientFactory.Client.Refresh("metatags");

            return doc;
        }
    }
}
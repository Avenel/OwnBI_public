using OwnBI.DataAccess;
using OwnBI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.Repositories
{
    public static class DocTypeRepository
    {
        public static List<DocType> Index()
        {
            var res = ElasticClientFactory.Client.Search<DocType>(s => s
               .From(0)
               .Size(100)
               .Query(q => q.MatchAll())
               .Sort(o => o.Ascending(f => f.Name)));

            var list = new List<DocType>();
            if (res.Total > 0)
            {
                list = res.Hits.Select(h => h.Source).ToList();
            }

            return list;
        }

        public static DocType Create(string name, string description, List<Guid> metaTags)
        {
            var docType = new DocType()
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                MetaTags = (metaTags != null)? metaTags : new List<Guid>()
            };

            var response = ElasticClientFactory.Client.Index(docType);
            ElasticClientFactory.Client.Refresh("doctypes");

            return docType;
        }

        public static DocType Read(Guid id)
        {
            var docSearch = ElasticClientFactory.Client.Get<DocType>(id);

            if (docSearch.Found)
            {
                return docSearch.Source;
            }
            throw new KeyNotFoundException();
        }

        public static DocType Update(Guid id, string name, string description, List<Guid> metaTags)
        {

            var doc = DocTypeRepository.Read(id);
            doc.Name = name;
            doc.Description = description;
            doc.MetaTags = (metaTags != null) ? metaTags : new List<Guid>();

            var response = ElasticClientFactory.Client.Index(doc);
            ElasticClientFactory.Client.Refresh("doctypes");
            return doc;
        }

        public static DocType Delete(Guid id)
        {
            var doc = DocTypeRepository.Read(id);
            var resDel = ElasticClientFactory.Client
                .Delete<DocType>(doc);

            ElasticClientFactory.Client.Refresh("doctypes");

            return doc;
        }
    }
}
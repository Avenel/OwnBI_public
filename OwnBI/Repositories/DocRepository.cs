using OwnBI.DataAccess;
using OwnBI.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace OwnBI.Repositories
{
    public static class DocRepository
    {

        public static List<dynamic> Index()
        {
            var res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
               .Index("docs")
               .Size(50)
               .MatchAll()
            );

            var list = new List<dynamic>();
            if (res.Total > 0)
            {
                list = res.Hits.Select(h => h.Source as dynamic).ToList();
            }

            return list;
        }

        public static List<dynamic> Search(string query)
        {
            var res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
               .Index("docs")
               .Size(50)
               .Query(q => q
                .MatchPhrasePrefix(m => m.Field("name").Query(query))
                )
            );

            var list = new List<dynamic>();
            if (res.Total > 0)
            {
                list = res.Hits.Select(h => h.Source as dynamic).ToList();
            }

            return list;
        }

        public static dynamic Create(Guid type, ExpandoObject content)
        {
            (content as dynamic).Id = Guid.NewGuid();
            (content as dynamic).Type = type;

            var response = ElasticClientFactory.Client.Index(content, p =>
                p.Index("docs")
                .Id((content as dynamic).Id.ToString())
                .Refresh());

            return content;
        }

        public static dynamic Read(Guid id)
        {
            ElasticClientFactory.Settings.DefaultIndex("docs");
            var docSearch = ElasticClientFactory.Client.Get<ExpandoObject>(id);
            ElasticClientFactory.Settings.DefaultIndex("ownbi");

            if (docSearch.Found)
            {
                return docSearch.Source as dynamic;
            }
            throw new KeyNotFoundException();
        }

        public static dynamic Update(ExpandoObject content)
        {
            var response = ElasticClientFactory.Client.Index(content, p =>
                p.Index("docs")
                .Id((content as dynamic).Id.ToString())
                .Refresh());

            return content as dynamic;
        }

        public static dynamic Delete(Guid id)
        {
            var doc = DocRepository.Read(id) as ExpandoObject;
            var resDel = ElasticClientFactory.Client
                .Delete<ExpandoObject>(id, p => p.Index("docs"));

            ElasticClientFactory.Client.Refresh("docs");

            return doc;
        }

    }
}
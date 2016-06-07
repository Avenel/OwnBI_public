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
            var queryNodes = query.Split(',');
            var listOfQueries = new List<Nest.QueryContainer>();

            // build query 
            foreach (var queryNode in queryNodes)
            {   
                var queryBase = new Nest.MatchPhrasePrefixQuery();

                if (queryNode.IndexOf(':') >= 0)
                {
                    var typeAndValue = queryNode.Split(':');
                    var type = typeAndValue[0].Trim().ToLower();
                    var value = typeAndValue[1].Trim();
                    queryBase.Field = new Nest.Field();
                    queryBase.Field.Name = type;
                    queryBase.Query = value;
                } else {
                    // use _allField
                    queryBase.Field = new Nest.Field();
                    queryBase.Field.Name = "_all";
                    queryBase.Query = queryNode;
                }
                listOfQueries.Add(new Nest.QueryContainer(queryBase));
            }

            var res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
               .Index("docs")
               .Size(50)
               .Query(q => q
                   .Bool(b => b
                        .Must(listOfQueries.ToArray())
                    )
                )
            );

            var list = new List<dynamic>();
            if (res.Total > 0)
            {
                list = res.Hits.Select(h => h.Source as dynamic).ToList();
            }

            return list;
        }

        public static Dictionary<string, float> Aggregate(string category, string fact)
        {
            
            var res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
                .Index("docs")
                .From(0)
                .MatchAll()
                .Aggregations(a =>
                    a.Terms("tagcloud", ta => ta.Field(category.ToLower())
                        .Aggregations(aa =>
                            aa.Sum("summe", ts => ts.Field(fact.ToLower()))
                        )
                    )
                )
            );

            var values = new Dictionary<string, float>();
            foreach (var tag in (res.Aggregations["tagcloud"] as Nest.BucketAggregate).Items)
            {
                var nestTag = tag as Nest.KeyedBucket;
                double sum = 0.0;
                var sumAggs = (nestTag.Aggregations["summe"] as Nest.ValueAggregate);
                sum = sumAggs.Value.Value;

                values.Add(nestTag.Key, (float)sum);
            }

            return values;
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
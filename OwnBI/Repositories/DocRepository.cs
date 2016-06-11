using Nest;
using OwnBI.DataAccess;
using OwnBI.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
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
                   .Bool(b => b
                        .Must(BuildQueryContainer(query))
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

        // level<category, value>
        public static Dictionary<string, float> Aggregate(List<string> categories, string fact, string query)
        {
            var values = new Dictionary<string, float>();

            if (categories != null && categories.Count > 0 && fact != null && fact.Length > 0)
            {
                var searchQuery = new SearchDescriptor<ExpandoObject>()
                    .Index("docs")
                    .From(0)
                    .Size(1000)
                    .Query(q => q
                        .Bool(b => b
                            .Must(BuildQueryContainer(query))
                        )
                    )
                    .Aggregations(a =>
                    {
                        if (categories.Count == 1)
                        {
                            return a.Terms("level0", ta => ta.Size(1000).Field(categories[0].ToLower())
                                .Aggregations(aa =>
                                    aa.Sum("summe", ts => ts.Field(fact.ToLower()))
                                )
                            );
                        }
                        else
                        {
                            return BuildAggregationContainer(a, 0, categories.Count - 1, categories, fact.ToLower());         
                        }
                    });

                var filenameQuery = @"d:\query.txt";
                using (FileStream SourceStream = File.Open(filenameQuery, FileMode.Create))
                {
                    ElasticClientFactory.Client.Serializer.Serialize(searchQuery, SourceStream);
                }

                var res = ElasticClientFactory.Client.Search<ExpandoObject>(searchQuery);

                var filenameResult = @"d:\result.txt";
                using (FileStream SourceStream = File.Open(filenameResult, FileMode.Create))
                {
                    ElasticClientFactory.Client.Serializer.Serialize(res.Aggregations, SourceStream);
                }

                Nest.BucketAggregate firstBucketAggregate = res.Aggregations["level0"] as Nest.BucketAggregate;
                ExtractKeyAndValues(values, firstBucketAggregate, 0, "");
                
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

        private static Nest.QueryContainer[] BuildQueryContainer(string query)
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
                }
                else
                {
                    // use _allField
                    queryBase.Field = new Nest.Field();
                    queryBase.Field.Name = "_all";
                    queryBase.Query = queryNode;
                }
                listOfQueries.Add(new Nest.QueryContainer(queryBase));
            }

            return listOfQueries.ToArray();
        }

        private static AggregationContainerDescriptor<ExpandoObject> BuildAggregationContainer(AggregationContainerDescriptor<ExpandoObject> a, 
                                                                                                int i, int max, List<string> categories, string fact)
        {
            return a.Terms("level" + i, ta => ta.Size(1000).Field(categories[i].ToLower())
                .Aggregations(aa =>
                    {
                        if (i == max)
                        {
                            return aa.Sum("summe", ts => ts.Field(fact.ToLower()));
                        }
                        else
                        {
                            i++;
                            return BuildAggregationContainer(aa, i, categories.Count - 1, categories, fact.ToLower());        
                        }
                    }
                )
            );
        }

        private static void ExtractKeyAndValues(Dictionary<string, float> values, Nest.BucketAggregate bucket, int i, string key)
        {
            i++;
            foreach (var lvl in bucket.Items)
            {
                var nestTag = lvl as Nest.KeyedBucket;
                var nestTagKey = key + ((key.Length > 0)? "_" : "") + nestTag.Key;

                if (nestTag.Aggregations != null && nestTag.Aggregations.ContainsKey("level" + i) )
                {
                    ExtractKeyAndValues(values, nestTag.Aggregations["level" + i] as Nest.BucketAggregate, i, nestTagKey);
                }
                else
                {
                    double sum = 0.0;
                    var sumAggs = (nestTag.Aggregations["summe"] as Nest.ValueAggregate);
                    sum = sumAggs.Value.Value;
                    values.Add(key + "_" + nestTag.Key, (float)sum);
                }
            }
         
        }

    }
}
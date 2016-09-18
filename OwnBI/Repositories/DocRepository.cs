using Nest;
using OwnBI.DataAccess;
using OwnBI.Models;
using OwnBI.ViewModels;
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

		public static List<string> GetUniqueStringValuesByMetaTagName(string metaTag)
		{
			var res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
			   .Index("docs")
			   .Aggregations(aa => 
				   aa.Terms("level0", ta => ta.Size(1000).Field(metaTag.ToLower()))
				)
			);

			var items = ((Nest.BucketAggregate)res.Aggregations.Values.First()).Items;

			var listOfUniqueNames = new List<string>();
			foreach (var item in items)
			{
				string name = ((KeyedBucket)item).Key;
				int number = 0;
				if (name.Length > 0 && !int.TryParse(name, out number))
				{
					listOfUniqueNames.Add(name);
				}				
			}

			return listOfUniqueNames;
		}

        public static List<dynamic> Search(string query, int? diffFromDays, int? diffToDays, int? count)
        {
            var res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
               .Index("docs")
			   .From(0)
               .Size((count.HasValue)? count.Value : 50)
               .Query(q => q
                   .Bool(b => b
                        .Must(BuildQueryContainer(query, diffFromDays, diffToDays))
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
        public static Dictionary<string, float> Aggregate(List<string> categories, string fact, string query, string aggFunc, int? diffFromDays, int? diffToDays)
        {
            var values = new Dictionary<string, float>();

            try
            {
                if (categories != null && categories.Count > 0 && fact != null && fact.Length > 0)
                {
                    var searchQuery = new SearchDescriptor<ExpandoObject>()
                        .Index("docs")
                        .From(0)
                        .Size(1000)
                        .Query(q => q
                            .Bool(b => b
                                .Must(BuildQueryContainer(query, diffFromDays, diffToDays))
                            )
                        )
                        .Aggregations(a =>
                        {
                            if (categories.Count == 1)
                            {
                                return a.Terms("level0", ta => ta.Size(1000).Field(categories[0].ToLower())
                                    .Aggregations(aa =>
                                    {
                                        if (aggFunc == "avg")
                                            return aa.Average("summe", ts => ts.Field(fact.ToLower()));

                                        if (aggFunc == "min")
                                            return aa.Min("summe", ts => ts.Field(fact.ToLower()));

                                        if (aggFunc == "max")
                                            return aa.Max("summe", ts => ts.Field(fact.ToLower()));

                                        return aa.Sum("summe", ts => ts.Field(fact.ToLower()));
                                    })
                                );
                            }
                            else
                            {
                                return BuildAggregationContainer(a, 0, categories.Count - 1, categories, fact.ToLower(), aggFunc);
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
                    ExtractKeyAndValues(values, firstBucketAggregate, 0, "", (aggFunc == "count"));

                }
            }
            catch (Exception e)
            {
                var filenameError = @"d:\error.txt";
                if (!File.Exists(filenameError))
                {
                    // Create a file to write to.
                    File.WriteAllText(filenameError, String.Format("{0:dd.MM.yyyy hh:MM:ss}", DateTime.Now) + "\n" + e.Message + ": \n" + e.StackTrace);
                }
                File.AppendAllText(filenameError, "\n" + String.Format("{0:dd.MM.yyyy hh:MM:ss}", DateTime.Now) + "\n" + e.Message + ": \n" + e.StackTrace);
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

        private static Nest.QueryContainer[] BuildQueryContainer(string query, int? diffFromDays, int? diffToDays)
        {
            var queryNodes = query.Split(',');
            var listOfQueries = new List<Nest.QueryContainer>();

            // build query 
            foreach (var queryNode in queryNodes)
            {
                var queryBase = new Nest.MatchQuery();// MatchPhrasePrefixQuery();

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

            // DateTime Range
            if (diffFromDays.HasValue || diffToDays.HasValue)
            {
                var queryRange = new Nest.DateRangeQuery();
                queryRange.Field = new Nest.Field();
                queryRange.Field.Name = "datum";
                queryRange.GreaterThanOrEqualTo = DateTime.Now.ToUniversalTime().Date.AddDays((diffFromDays.HasValue)? diffFromDays.Value : 0);
                queryRange.LessThanOrEqualTo = DateTime.Now.ToUniversalTime().Date.AddDays((diffToDays.HasValue)? diffToDays.Value : 0);
                listOfQueries.Add(new Nest.QueryContainer(queryRange));
            }

            return listOfQueries.ToArray();
        }

        private static AggregationContainerDescriptor<ExpandoObject> BuildAggregationContainer(AggregationContainerDescriptor<ExpandoObject> a, 
                                                                                                int i, int max, List<string> categories, string fact,
                                                                                                string aggFunc)
        {
            return a.Terms("level" + i, ta => ta
                    .Size(1000)
                    .Field(categories[i].ToLower())
                .Aggregations(aa =>
                    {
                        if (i == max)
                        {
                            if (aggFunc == "avg")
                                return aa.Average("summe", ts => ts.Field(fact.ToLower()));

                            if (aggFunc == "min")
                                return aa.Min("summe", ts => ts.Field(fact.ToLower()));

                            if (aggFunc == "max")
                                return aa.Max("summe", ts => ts.Field(fact.ToLower()));

                            return aa.Sum("summe", ts => ts.Field(fact.ToLower()));
                        }
                        else
                        {
                            i++;
                            return BuildAggregationContainer(aa, i, categories.Count - 1, categories, fact.ToLower(), aggFunc);        
                        }
                    }
                )
            );
        }

		public static List<DocViewModel> GetMostRecentDocumentsForDocType(Guid docType, int diffDays)
		{
			// Filter: type
			var queryBase = new Nest.MatchQuery();
			queryBase.Field = new Nest.Field();
            queryBase.Field.Name = "type";
            queryBase.Query = docType.ToString();

			var queries = new List<Nest.QueryContainer>();
			queries.Add(new Nest.QueryContainer(queryBase));

			// Filter: DateRange
			var queryRange = new Nest.DateRangeQuery();
			queryRange.Field = new Nest.Field();
			queryRange.Field.Name = "datum";
			queryRange.GreaterThanOrEqualTo = DateTime.Now.ToUniversalTime().Date.AddDays(diffDays);
			queryRange.LessThanOrEqualTo = DateTime.Now.ToUniversalTime().Date.AddDays(0);
			
			queries.Add(new Nest.QueryContainer(queryRange));

			var res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
               .Index("docs")
			   .From(0)
			   .Size(1000)
			   .Query(q => q
                   .Bool(b => b
                        .Must(queries.ToArray())
                    )
                )
				.Aggregations(a => 
                    a.Terms("tagcloud", ta => ta.Field("name"))
                )
			);

			var list = new List<dynamic>();
			if (res.Total > 0)
			{
				list = res.Hits.Select(h => h.Source as dynamic).ToList();
			}

			var docViewModelList = new List<DocViewModel>();
			foreach (dynamic doc in list)
			{
				var docModel = new DocViewModel();
				DocType type = DocTypeRepository.Read(Guid.Parse(doc.type));
				docModel.Id = Guid.Parse(doc.id);
				docModel.Type = type.Name;
				try
				{
					docModel.Name = doc.name;
				}
				catch (Exception e)
				{
					docModel.Name = "";
				}

				docModel.MetaTags = MetaTagRepository.ReadMany(type.MetaTags);
				docViewModelList.Add(docModel);
			}

			var docsPerNameList = new Dictionary<string, long>();
			foreach (var tag in (res.Aggregations["tagcloud"] as Nest.BucketAggregate).Items)
			{
				var nestTag = tag as Nest.KeyedBucket;
				docsPerNameList.Add(nestTag.Key, nestTag.DocCount.Value);
			}

			var rankedList = new List<DocViewModel>();
			var i = 0;
			foreach (var docName in docsPerNameList.Keys)
			{
				if (i<10)
				{
					var doc = docViewModelList.FirstOrDefault(d => d.Name == docName);
					doc.DocCountInDb = (int) docsPerNameList[docName];
					rankedList.Add(doc);
				} else
				{
					break;
				}
			}

			return rankedList;
		}

        private static void ExtractKeyAndValues(Dictionary<string, float> values, Nest.BucketAggregate bucket, int i, string key, bool docCount)
        {
            i++;
            foreach (var lvl in bucket.Items)
            {
                var nestTag = lvl as Nest.KeyedBucket;
                var nestTagKey = key + ((key.Length > 0)? "_" : "") + ((nestTag.KeyAsString != null)? nestTag.KeyAsString : nestTag.Key);

                if (nestTag.Aggregations != null && nestTag.Aggregations.ContainsKey("level" + i) )
                {
                    ExtractKeyAndValues(values, nestTag.Aggregations["level" + i] as Nest.BucketAggregate, i, nestTagKey, docCount);
                }
                else
                {
                    if (docCount)
                    {
                        values.Add(key + "_" + ((nestTag.KeyAsString != null) ? nestTag.KeyAsString : nestTag.Key), (float)nestTag.DocCount);
                    }
                    else
                    {
                        double sum = 0.0;
                        var sumAggs = (nestTag.Aggregations["summe"] as Nest.ValueAggregate);
                        sum = sumAggs.Value.Value;
                        values.Add(key + "_" + ((nestTag.KeyAsString != null) ? nestTag.KeyAsString : nestTag.Key), (float)sum);
                    }
                }
            }
         
        }

    }
}
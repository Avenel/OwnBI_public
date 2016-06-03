﻿using Nest;
using OwnBI.DataAccess;
using OwnBI.Repositories;
using OwnBI.ViewModels;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OwnBI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var model = new HomeViewModel();

            // Name TagCloud
            var nameTagCloudModel = new TagCloudViewModel();
            nameTagCloudModel.Title = "Meist verwendete Namen";
            nameTagCloudModel.Tags = new Dictionary<string, float>();
            var res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
                .Index("docs")
                .From(0)
                .MatchAll()
                .Aggregations(a => 
                    a.Terms("tagcloud", ta => ta.Field("name"))
                )
            );   

            foreach (var tag in (res.Aggregations["tagcloud"] as Nest.BucketAggregate).Items )
            {
                var nestTag = tag as Nest.KeyedBucket;
                nameTagCloudModel.Tags.Add(nestTag.Key, nestTag.DocCount.Value);
            }
            model.NameTagCloud = nameTagCloudModel;

            // DocType TagCloud
            var docTagCloudModel = new TagCloudViewModel();
            docTagCloudModel.Title = "Meist verwendete DocTypes";
            docTagCloudModel.Tags = new Dictionary<string, float>();

            foreach (var docType in DocTypeRepository.Index())
            {
                res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
                    .Index("docs")
                    .From(0)
                    .Query(q => q.Match(m => m.Field("type").Query(docType.Id.ToString())))
                );
                docTagCloudModel.Tags.Add(docType.Name, res.Total);
            }
            model.DocTypeTagCloud = docTagCloudModel;

            // Ort TagCloud
            var ortTagCloudModel = new TagCloudViewModel();
            ortTagCloudModel.Title = "Meist verwendete Orte";
            ortTagCloudModel.Tags = new Dictionary<string, float>();
            res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
                .Index("docs")
                .From(0)
                .MatchAll()
                .Aggregations(a =>
                    a.Terms("tagcloud", ta => ta.Field("ort"))
                )
            );

            foreach (var tag in (res.Aggregations["tagcloud"] as Nest.BucketAggregate).Items)
            {
                var nestTag = tag as Nest.KeyedBucket;
                ortTagCloudModel.Tags.Add(nestTag.Key, nestTag.DocCount.Value);
            }
            model.OrtTagCloud = ortTagCloudModel;

            // Kategorie TagCloud
            var kategorieTagCloudModel = new TagCloudViewModel();
            kategorieTagCloudModel.Title = "Meist verwendete Kategorien";
            kategorieTagCloudModel.Tags = new Dictionary<string, float>();
            res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
                .Index("docs")
                .From(0)
                .MatchAll()
                .Aggregations(a =>
                    a.Terms("tagcloud", ta => ta.Field("kategorie"))
                )
            );

            foreach (var tag in (res.Aggregations["tagcloud"] as Nest.BucketAggregate).Items)
            {
                var nestTag = tag as Nest.KeyedBucket;
                kategorieTagCloudModel.Tags.Add(nestTag.Key, nestTag.DocCount.Value);
            }
            model.KategorieTagCloud = kategorieTagCloudModel;

            // Handel TagCloud
            var handelTagCloudModel = new TagCloudViewModel();
            handelTagCloudModel.Title = "Meist verwendete Handel";
            handelTagCloudModel.Tags = new Dictionary<string, float>();
            res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
                .Index("docs")
                .From(0)
                .MatchAll()
                .Aggregations(a =>
                    a.Terms("tagcloud", ta => ta.Field("handel"))
                )
            );

            foreach (var tag in (res.Aggregations["tagcloud"] as Nest.BucketAggregate).Items)
            {
                var nestTag = tag as Nest.KeyedBucket;
                handelTagCloudModel.Tags.Add(nestTag.Key, nestTag.DocCount.Value);
            }
            model.HandelTagCloud = handelTagCloudModel;

            // Preis Summe
            res = ElasticClientFactory.Client.Search<ExpandoObject>(s => s
                .Index("docs")
                .From(0)
                .Size(1000)
                .Query(q => q.Match(m => m.Field("type").Query("e0c4f438-4509-449b-a491-3fa3093b4129")))
                .Aggregations(a =>
                    a.Sum("summe", aa => aa
                        .Field("preis"))
                )                
            );

            model.SummeAusgaben = res.Aggs.Sum("summe").Value.Value;
            model.SummeEinnahmen = 0.0;

            //foreach (var tag in (res.Aggregations["summe"] as Nest.BucketAggregate).Items)
            //{
            //    var nestTag = tag as Nest.KeyedBucket;
            //    model.SummePreis = 0.00m;//nestTag.val;
            //}

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
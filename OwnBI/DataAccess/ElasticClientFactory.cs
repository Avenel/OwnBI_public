using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using OwnBI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.DataAccess
{
    public static class ElasticClientFactory
    {

        public static Uri Node = new Uri("http://localhost:9200");
        public static SingleNodeConnectionPool connectionPool = new SingleNodeConnectionPool(Node);
        
        public static ConnectionSettings Settings = 
            new ConnectionSettings(connectionPool, c =>
                new MyJsonNetSerializer(c))
            .DefaultIndex("ownbi")
            .MapDefaultTypeIndices(m => m
                .Add(typeof(MetaTag), "metatags")
                .Add(typeof(DocType), "doctypes")
            );

        public static ElasticClient Client = new ElasticClient(Settings);

        public class MyJsonNetSerializer : JsonNetSerializer
        {
            public MyJsonNetSerializer(IConnectionSettingsValues settings)
                : base(settings)
            {
            }

            protected override void ModifyJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings)
            {
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            }
        }

    }
}
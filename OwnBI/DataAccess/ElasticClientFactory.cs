using Nest;
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
        public static ConnectionSettings Settings = 
            new ConnectionSettings(Node)
            .DefaultIndex("ownbi")
            .MapDefaultTypeIndices(m => m
                .Add(typeof(MetaTag), "metatags")
                .Add(typeof(DocType), "doctypes")
            );

        public static ElasticClient Client = new ElasticClient(Settings);


    }
}
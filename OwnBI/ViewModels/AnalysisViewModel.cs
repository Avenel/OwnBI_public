using OwnBI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.ViewModels
{
    public class AnalysisViewModel
    {
        public ChartViewModel ChartModel { get; set; }

        public string Query { get; set; }

        public string Date { get; set; }

        public string DocType { get; set; }

        public Dictionary<string, MetaTag> Facts { get; set; }

        public List<string> SelectedFacts { get; set; }

        public Dictionary<string, MetaTag> Categories { get; set; }

        public List<string> SelectedCategories { get; set; }

        public List<DocType> DocTypes { get; set; }

        public string ChartType { get; set; }

        public string AggFunc { get; set; }

    }
}
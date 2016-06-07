using OwnBI.Models;
using OwnBI.Repositories;
using OwnBI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OwnBI.Controllers
{
    public class AnalysisController : Controller
    {
        // GET: Analysis
        public ActionResult Index(List<string> categories, List<string> facts, string query, string  docType)
        {
            var model = new AnalysisViewModel();
            
            if (docType == "_all" || docType == null || docType == "")
            {
                model.Categories = MetaTagRepository.Index().Where(mt => mt.DataType == "string").ToDictionary<MetaTag, string>(mt => mt.Id.ToString());
                model.Facts = MetaTagRepository.Index().Where(mt => mt.DataType == "number").ToDictionary<MetaTag, string>(mt => mt.Id.ToString());
            }
            else
            {
                model.Categories = MetaTagRepository.ReadMany(DocTypeRepository.Read(Guid.Parse(docType)).MetaTags)
                                    .Where(mt => mt.DataType == "string")
                                    .ToDictionary<MetaTag, string>(mt => mt.Id.ToString());

                model.Facts = MetaTagRepository.ReadMany(DocTypeRepository.Read(Guid.Parse(docType)).MetaTags)
                                    .Where(mt => mt.DataType == "number")
                                    .ToDictionary<MetaTag, string>(mt => mt.Id.ToString());
            }

            model.SelectedCategories = (categories != null) ? categories  : new List<string>();
            model.SelectedFacts = (facts != null)? facts : new List<string>();

            var selectedCategoryNames = model.Categories.Where(c => categories.Contains(c.Key.ToString())).Select(c => c.Value.Name).ToList<string>();
            var selectedFactNames = model.Facts.Where(c => facts.Contains(c.Key.ToString())).Select(c => c.Value.Name).ToList<string>();

            var valueList = new List<float>();
            var labelList = new List<string>();

            // Todo Build Datasets for chartjs markup
            foreach(var category in selectedCategoryNames)
            {
                var aggs = DocRepository.Aggregate(selectedCategoryNames[0], selectedFactNames[0]);
                var values = (selectedCategoryNames.Count > 0 && selectedFactNames.Count > 0) ? aggs.Values.ToList<float>() : new List<float>();
                valueList = values;
                labelList = aggs.Keys.ToList<string>();
            }
            

            model.ChartModel = new ChartViewModel()
            {
                Height = 400,
                Labels = labelList,
                Values = valueList,
                Type = "bar",
                Title = "Bla."
            };

            return View(model);
        }
    }
}
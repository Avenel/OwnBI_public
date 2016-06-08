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
                model.Categories = MetaTagRepository.Index().Where(mt => mt.DataType == "string" || mt.DataType == "datetime").ToDictionary<MetaTag, string>(mt => mt.Id.ToString());
                model.Facts = MetaTagRepository.Index().Where(mt => mt.DataType == "number").ToDictionary<MetaTag, string>(mt => mt.Id.ToString());
            }
            else
            {
                model.Categories = MetaTagRepository.ReadMany(DocTypeRepository.Read(Guid.Parse(docType)).MetaTags)
                                    .Where(mt => mt.DataType == "string" || mt.DataType == "datetime")
                                    .ToDictionary<MetaTag, string>(mt => mt.Id.ToString());

                model.Facts = MetaTagRepository.ReadMany(DocTypeRepository.Read(Guid.Parse(docType)).MetaTags)
                                    .Where(mt => mt.DataType == "number")
                                    .ToDictionary<MetaTag, string>(mt => mt.Id.ToString());
            }

            model.SelectedCategories = (categories != null) ? categories  : new List<string>();
            model.SelectedFacts = (facts != null)? facts : new List<string>();

            var selectedCategoryNames = (categories != null) ? model.Categories.Where(c => categories.Contains(c.Key.ToString())).Select(c => c.Value.Name).ToList<string>() : new List<string>();
            var selectedCategoryTypes = (categories != null) ? model.Categories.Where(c => categories.Contains(c.Key.ToString())).Select(c => c.Value.DataType).ToList<string>() : new List<string>();
            var selectedFactNames = (facts != null) ? model.Facts.Where(c => facts.Contains(c.Key.ToString())).Select(c => c.Value.Name).ToList<string>() : new List<string>();

            var valueList = new List<List<float>>();
            var labelList = new List<string>();
            var titleList = new List<string>();

            // Todo Build Datasets for chartjs markup
            var i=0;
            foreach(var category in selectedCategoryNames)
            {
                foreach (var fact in selectedFactNames)
                {
                    var aggs = DocRepository.Aggregate(category, fact);
                    var values = (selectedCategoryNames.Count > 0 && selectedFactNames.Count > 0) ? aggs.Values.ToList<float>() : new List<float>();
                    valueList.Add(values);
                    if (selectedCategoryTypes[i] == "datetime")
                    {
                        labelList = aggs.Keys.ToList<string>().Select(s => String.Format("{0:dd.MM.yyyy}", new DateTime(long.Parse(s)))).ToList<string>();
                    } else 
                    {
                        labelList = aggs.Keys.ToList<string>();
                    }
                    
                    titleList.Add(fact);
                }
                i++;
            }
            

            model.ChartModel = new ChartViewModel()
            {
                Height = 400,
                Categories = labelList,
                SeriesNames = titleList,
                Values = valueList,
                Type = "bar",
                Title = "Bla."
            };

            model.DocType = docType;
            model.DocTypes = DocTypeRepository.Index();

            return View(model);
        }
    }
}
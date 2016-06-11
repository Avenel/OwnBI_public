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
        public ActionResult Index(List<string> categories, List<string> facts, string query, string  docType, string chartType)
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

            categories = (categories != null) ? categories  : new List<string>();
            model.SelectedCategories = categories;
            model.SelectedFacts = (facts != null)? facts : new List<string>();

            if (chartType == null)
                chartType = "bar";

            model.ChartType = chartType;

            model.Query = query;
            Guid docTypeGuid;
            if (Guid.TryParse(docType, out docTypeGuid))
            {
                if (query == null)
                    query = "";

                if (query.Length > 0)
                    query += ", ";

                query += "type:" + docType;
            }


            var selectedCategoryNames = new List<string>();
            var selectedCategoryTypes = new List<string>();

            // in order to save order of selected categories, use iteration
            foreach(var cat in categories)
            {
                var catName = model.Categories.Where(c => c.Value.Id.ToString() == cat).Select(c => c.Value.Name).FirstOrDefault();
                var catType = model.Categories.Where(c => c.Value.Id.ToString() == cat).Select(c => c.Value.DataType).FirstOrDefault();
                selectedCategoryNames.Add(catName);
                selectedCategoryTypes.Add(catType);
            }
            
            var selectedFactNames = (facts != null) ? model.Facts.Where(c => facts.Contains(c.Key.ToString())).Select(c => c.Value.Name).ToList<string>() : new List<string>();

            var valueList = new List<List<float>>();
            var labelList = new List<string>();
            var titleList = new List<string>();

            // Todo Build Datasets for chartjs markup
            if (selectedCategoryNames.Count > 1)
            {
                var aggs = DocRepository.Aggregate(selectedCategoryNames, selectedFactNames[0], query);
                titleList = aggs.Keys.Select(k => k.Split('_')[1]).Distinct().ToList();

                var groupKeys = aggs.Keys.Select(k => k.Split('_')[0]).Distinct().ToList();
                labelList = groupKeys;

                foreach (var group in groupKeys)
                {
                    var valuePairs = aggs.Where(a => a.Key.ToLower().Contains(group.ToLower())).ToList();
                    
                    var values = new List<float>();
                    foreach (var title in titleList)
                    {
                        var pair = valuePairs.Where(p => p.Key.ToLower().Contains(title.ToLower())).ToList();

                        if (pair.Count> 0)
                        {
                            values.Add(pair[0].Value);
                        }
                        else
                        {
                            values.Add(0.0f);
                        }
                    }

                    valueList.Add(values);
                }
            }
            else
            {
                foreach (var fact in selectedFactNames)
                {
                    var aggs = DocRepository.Aggregate(selectedCategoryNames, fact, query);
                    var values = (selectedCategoryNames.Count > 0 && selectedFactNames.Count > 0) ? aggs.Values.ToList<float>() : new List<float>();
                    valueList.Add(values);
                    titleList = aggs.Keys.Select(k => k.Replace("_", "")).ToList<string>().Distinct().ToList();
                    labelList.Add(fact);
                }
            }

            model.ChartModel = new ChartViewModel()
            {
                Height = 400,
                Categories = titleList,
                SeriesNames = labelList,
                Values = valueList,
                Type = model.ChartType,
                Title = "",
                HidePanel = true
            };

            model.DocType = docType;
            model.DocTypes = DocTypeRepository.Index();

            return View(model);
        }
    }
}
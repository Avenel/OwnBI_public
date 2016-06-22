using OfficeOpenXml;
using OwnBI.Models;
using OwnBI.Repositories;
using OwnBI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OwnBI.Controllers
{
    public class AnalysisController : Controller
    {
        // GET: Analysis
        public ActionResult Index(List<string> categories, List<string> facts, string query, string  docType, string chartType, string aggFunc,
                                    string from, string to)
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

            if (aggFunc == null)
                aggFunc = "sum";

            model.ChartType = chartType;
            model.AggFunc = aggFunc;

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

            // from and to date
            model.From = from;
            model.To = to;

            DateTime fromDate;
            DateTime toDate;
            int? diffFomDays = null;
            int? diffToDays = null;

            if (DateTime.TryParse(from, out fromDate))
            {
                diffFomDays = (fromDate - DateTime.Now).Days;
            }

            if (DateTime.TryParse(from, out toDate))
            {
                diffToDays = (DateTime.Now - toDate).Days;
            }
            DateTime.TryParse(from, out toDate); 

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
                var aggs = DocRepository.Aggregate(selectedCategoryNames, selectedFactNames[0], query, aggFunc, diffFomDays, diffToDays);
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
                    var aggs = DocRepository.Aggregate(selectedCategoryNames, fact, query, aggFunc, diffFomDays, diffToDays);
                    var values = (selectedCategoryNames.Count > 0 && selectedFactNames.Count > 0) ? aggs.Values.ToList<float>() : new List<float>();
                    valueList.Add(values);
                    titleList = aggs.Keys.Select(k => k.Replace("_", "")).ToList<string>().Distinct().ToList();

                    labelList.Add(fact);
                    labelList = (model.AggFunc != "count") ? labelList : new List<string>() { "Anzahl" };

                    if (model.AggFunc == "count")
                        break;
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

        public ActionResult GetExcel()
        {
            /*HttpResponseMessage response;
            response = Request.CreateResponse(HttpStatusCode.OK);
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content = new StreamContent(GetExcelSheet());
            response.Content.Headers.ContentType = mediaType;
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "Orders.xls";
            return response;*/

            var cd = new System.Net.Mime.ContentDisposition
            {
                // for example foo.bak
                FileName = "Analysis.xlsx",

                // always prompt the user for downloading, set to true if you want 
                // the browser to try to show the file inline
                Inline = false,
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());
            return File(CreateExcelFile(), "application/octet-stream");
        }

        public List<string> GetOrders()
        {
            return new List<string>(){"Hello", "World"};
        }

        private  MemoryStream CreateExcelFile()
        {
            FileInfo newFile = new FileInfo("D:\\Analysis.xlsx");

            using (var package = new ExcelPackage(newFile))
            {
                var worksheet = package.Workbook.Worksheets.Where(w => w.Name == "Orders").FirstOrDefault();
                worksheet.Cells["B1"].LoadFromCollection(GetOrders(), false);
                // package.Save();
                var stream = new MemoryStream(package.GetAsByteArray());
                return stream;
            }
        }
    }
}
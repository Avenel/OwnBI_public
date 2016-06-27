using Newtonsoft.Json;
using OwnBI.Models;
using OwnBI.Repositories;
using OwnBI.ViewModels;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace OwnBI.Controllers
{
    public class DocController : Controller
    {
        // GET: Doc
        public ActionResult Index(string query, string docType, string from, string to)
        {
            var model = new DocIndexViewModel();
            model.Query = (query != null) ? query : "";
            model.DocType = docType;
            model.From = from;
            model.To = to;

            bool docTypeIsSet = (docType != null && docType.Length > 0);

            var queryWithFilters = model.Query;
            queryWithFilters += (docTypeIsSet && queryWithFilters.Length > 0) ? ", " : "";
            queryWithFilters += (docTypeIsSet) ? " type:" + docType : "";

            DateTime fromDate;
            DateTime toDate;
            int? diffFomDays = null;
            int? diffToDays = null;

            if (DateTime.TryParse(from, out fromDate))
            {
                diffFomDays = (fromDate - DateTime.Now).Days;
            }

            if (DateTime.TryParse(to, out toDate))
            {
                diffToDays = (toDate - DateTime.Now).Days;
            }
            DateTime.TryParse(from, out toDate); 

            var docs = new List<dynamic>();

            if (queryWithFilters.Length > 0 || diffFomDays.HasValue || diffToDays.HasValue)
            {
                docs = DocRepository.Search(queryWithFilters, diffFomDays, diffToDays);
            }
            else
            {
                docs = DocRepository.Index();
            }
            
            var list = new List<DocViewModel>();
            foreach(dynamic doc in docs)
            {
                var docModel = new DocViewModel();
                DocType type = DocTypeRepository.Read(Guid.Parse(doc.type));
                docModel.Id = Guid.Parse(doc.id);
                docModel.Type = type.Name;
                try
                {
                    docModel.Name = doc.name;
                } catch(Exception e)
                {
                    docModel.Name = "";
                }
                
                docModel.MetaTags = MetaTagRepository.ReadMany(type.MetaTags);
                list.Add(docModel);
            }
            model.Docs = list;

            model.DocTypes = DocTypeRepository.Index();

            return View(model);
        }

        // GET: Doc/Details/5
        public ActionResult Details(Guid id)
        {
            var model = new DocViewModel();

            var doc = DocRepository.Read(id);

            var docType = DocTypeRepository.Read(Guid.Parse(doc.type));
            model.MetaTags = MetaTagRepository.ReadMany(docType.MetaTags);
            model.Id = Guid.Parse(doc.id);
            model.Type = docType.Name;
            model.TypeId = docType.Id;

            model.Values = mapMetaTagsAndDocValue(model.MetaTags, doc);

            return View(model);
        }

        // GET: Doc/Create
        public ActionResult Create(Guid type)
        {
            var model = new DocViewModel();
            var docType = DocTypeRepository.Read(type);
            model.MetaTags = MetaTagRepository.ReadMany(docType.MetaTags);
            model.Type = docType.Name;

            return View(model);
        }

        // POST: Doc/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection, Guid type, int anzahl)
        {
            try
            {
                for (int i = 0; i < anzahl; i++)
                {
                    DocRepository.Create(type, parseFormToObject());
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View();
            }
        }

        // GET: Doc/Edit/5
        public ActionResult Edit(Guid id)
        {
            var model = new DocViewModel();

            var doc = DocRepository.Read(id);

            var docType = DocTypeRepository.Read(Guid.Parse(doc.type));
            model.MetaTags = MetaTagRepository.ReadMany(docType.MetaTags);
            model.Id = Guid.Parse(doc.id);
            model.Type = docType.Name;
            model.TypeId = docType.Id;

            model.Values = mapMetaTagsAndDocValue(model.MetaTags, doc);
            return View(model);
        }

        // POST: Doc/Edit/5
        [HttpPost]
        public ActionResult Edit(Guid id, FormCollection collection)
        {
            try
            {
                DocRepository.Update(parseFormToObject());
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Doc/Delete/5
        public ActionResult Delete(Guid id)
        {
            var model = new DocViewModel();

            var doc = DocRepository.Read(id);

            var docType = DocTypeRepository.Read(Guid.Parse(doc.type));
            model.MetaTags = MetaTagRepository.ReadMany(docType.MetaTags);
            model.Type = docType.Name;
            model.TypeId = docType.Id;

            model.Values = mapMetaTagsAndDocValue(model.MetaTags, doc);
            return View(model);
        }

        // POST: Doc/Delete/5
        [HttpPost]
        public ActionResult Delete(Guid id, FormCollection collection)
        {
            try
            {
                DocRepository.Delete(id);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        private Dictionary<string, string> mapMetaTagsAndDocValue(List<MetaTag> metaTags, ExpandoObject docValues)
        {
            Dictionary<string, string> mapped = new Dictionary<string, string>();

            foreach (var metaTag in metaTags)
            {
                object val;
                if (((IDictionary<String, Object>) docValues).TryGetValue(metaTag.Name.ToLower(), out val))
                {
                    mapped.Add(metaTag.Id.ToString(), val.ToString());
                }
                else
                {
                    mapped.Add(metaTag.Id.ToString(), "");
                }
            }

            return mapped;
        }

        private ExpandoObject parseFormToObject ()
        {
            IDictionary<string, object> doc = new Dictionary<string, object>();

            // Entnehme Request Params alle Doc relevanten Parameter (Meta Tags)
            var docMetaTags = new Dictionary<MetaTag, object>();
            foreach (var param in Request.Params.AllKeys)
            {
                if (param.Contains("Doc."))
                {
                    var paramName = param.Replace("Doc.", "");
                    MetaTag metaTag;
                    if (paramName != "Id" && paramName != "Type")
                    {
                        metaTag = MetaTagRepository.Read(Guid.Parse(paramName));
                        doc.Add(metaTag.Name, CastMetaTagValueToDataType(metaTag, Request.Params[param]));
                    } else {
                        doc.Add(paramName, Request.Params[param]);
                    }
                }
            }

            // convert dictionary to object
            var eo = new ExpandoObject();
            var eoColl = (ICollection<KeyValuePair<string, object>>)eo;

            foreach (var kvp in doc)
            {
                eoColl.Add(kvp);
            }

            return eo;
        }

        private object CastMetaTagValueToDataType(MetaTag metaTag, string paramValue)
        {
            try
            {
                if (metaTag.DataType == "number")
                {
                    double result = 0.0;
                    Double.TryParse(paramValue, out result);
                    return result;
                }

                if (metaTag.DataType == "datetime")
                {
                    return DateTime.Parse(paramValue);
                }

                if (metaTag.DataType == "object")
                {
                    return JsonConvert.DeserializeObject<dynamic>(paramValue);
                }
            } catch (Exception e)
            {
                paramValue = e.ToString();
            }
            

            return paramValue;
        }

    }
}

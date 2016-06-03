﻿using Newtonsoft.Json;
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
        public ActionResult Index()
        {
            var model = new DocIndexViewModel();

            var docs = DocRepository.Index();
            var list = new List<DocViewModel>();
            foreach(dynamic doc in docs)
            {
                var docModel = new DocViewModel();
                DocType type = DocTypeRepository.Read(Guid.Parse(doc.type));
                docModel.Id = Guid.Parse(doc.id);
                docModel.Type = type.Name;
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
        public ActionResult Create(FormCollection collection, Guid type)
        {
            try
            {
                DocRepository.Create(type, parseFormToObject());

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

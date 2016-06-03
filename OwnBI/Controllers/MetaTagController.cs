using OwnBI.DataAccess;
using OwnBI.Models;
using OwnBI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OwnBI.Controllers
{
    public class MetaTagController : Controller
    {
        // GET: MetaTag
        public ActionResult Index()
        {
            return View(MetaTagRepository.Index());
        }

        // GET: MetaTag/Details/5
        public ActionResult Details(Guid id)
        {
            try
            {
                var doc = MetaTagRepository.Read(id);
                return View(doc);
            }
            catch(Exception e)
            {
                return RedirectToAction("Index");
            }
        }

        // GET: MetaTag/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MetaTag/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection, string name, string description, string dataType)
        {
            try
            {
                MetaTagRepository.Create(name, description, dataType);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: MetaTag/Edit/5
        public ActionResult Edit(Guid id)
        {
            try
            {
                var doc = MetaTagRepository.Read(id);
                return View(doc);
            } catch(Exception e)
            {
                return RedirectToAction("Index");
            }
            
        }

        // POST: MetaTag/Edit/5
        [HttpPost]
        public ActionResult Edit(Guid id, FormCollection collection, string name, string description, string dataType)
        {
            try
            {
                MetaTagRepository.Update(id, name, description, dataType);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: MetaTag/Delete/5
        public ActionResult Delete(Guid id)
        {
            try
            {
                var doc = MetaTagRepository.Read(id);
                return View(doc);
            }
            catch (Exception e)
            {
                return RedirectToAction("Index");
            }
        }

        // POST: MetaTag/Delete/5
        [HttpPost]
        public ActionResult Delete(Guid id, FormCollection collection)
        {
            try
            {
                MetaTagRepository.Delete(id);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

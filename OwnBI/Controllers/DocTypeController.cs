using OwnBI.Repositories;
using OwnBI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OwnBI.Controllers
{
    public class DocTypeController : Controller
    {
        // GET: DocType
        public ActionResult Index()
        {
            return View(DocTypeRepository.Index());
        }

        // GET: DocType/Details/5
        public ActionResult Details(Guid id)
        {
            var model = new DocTypeViewModel();
            model.DocType = DocTypeRepository.Read(id);

            return View(model);
        }

        // GET: DocType/Create
        public ActionResult Create()
        {
            var model = new DocTypeViewModel();
            return View(model);
        }

        // POST: DocType/Create
        [HttpPost]
        public ActionResult Create(DocTypeViewModel model)
        {
            try
            {
                var docType = DocTypeRepository.Create(model.DocType.Name, model.DocType.Description, model.DocType.MetaTags);
                return RedirectToAction("Index");
            }
            catch
            {
                return View(model);
            }
        }

        // GET: DocType/Edit/5
        public ActionResult Edit(Guid id)
        {
            var model = new DocTypeViewModel();
            model.DocType = DocTypeRepository.Read(id);
            return View(model);
        }

        // POST: DocType/Edit/5
        [HttpPost]
        public ActionResult Edit(DocTypeViewModel model)
        {
            try
            {
                // TODO: Add update logic here
                var docType = DocTypeRepository.Update(model.DocType.Id.Value, model.DocType.Name, model.DocType.Description, model.DocType.MetaTags);
                return RedirectToAction("Index");
            }
            catch
            {
                return View(model);
            }
        }

        // GET: DocType/Delete/5
        public ActionResult Delete(Guid id)
        {
            var model = new DocTypeViewModel();
            model.DocType = DocTypeRepository.Read(id);
            return View(model);
        }

        // POST: DocType/Delete/5
        [HttpPost]
        public ActionResult Delete(Guid id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
                var model = DocTypeRepository.Delete(id);
                return RedirectToAction("Index");
            }
            catch
            {
                var model = new DocTypeViewModel();
                model.DocType = DocTypeRepository.Read(id);
                return View(model);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyEcommerceAdmin.Models;
namespace MyEcommerceAdmin.Controllers
{
    public class CategoryController : Controller
    {
        MyEcommerceDbContext db = new MyEcommerceDbContext();
        // GET: Category
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ManageCategoty()
        {
            return View(db.Categories.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }  
        
        public ActionResult Delete()
        {
            return View();
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Category Category = db.Categories.Find(id);
            if (Category == null)
            {
                return HttpNotFound();
            }
            return View(Category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            Category Category = db.Categories.Find(id);
            db.Categories.Remove(Category);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Create(Category ctg)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(ctg);
                db.SaveChanges();
                return PartialView("_Success");
            }
            return PartialView("_Error");
        }


    }
}
using ListingTest.Data;
using ListingTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace ListingTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataBaseAccess _db;
        public HomeController(ILogger<HomeController> logger, DataBaseAccess db)
        {
            _logger = logger;
            _db = db;
        }
        [HttpPost]         
        public ActionResult LoadData(string trm, string Name_tb = "", string Iddate = "")
        {
            #region Pagination Area
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            #endregion

            var dd = _db.Products.DefaultIfEmpty();
            #region Searching Area
            if (Name_tb != null && Name_tb != "")
            {
                dd = dd.Where(d => d.Name.ToLower().Contains(Name_tb.ToLower()));
            }
            if (!string.IsNullOrEmpty(Iddate))
            {
                DateTime dt = DateTime.Parse(Iddate);
                dd = dd.Where(v => v.Date == dt);
            }
            #endregion
            var v = dd.ToList();       
            recordsTotal = v.Count();
            #region Getting Required Field
            var data = v.Skip(skip).Take(pageSize).DefaultIfEmpty();

            if (data.FirstOrDefault()!=null)
            {
              var  data1= data.ToList().Select(s => new
                   {
                       id = s.Id,
                       name = s.Name,
                       description = s.Description,
                       price = s.Price,
                       date = s.Date.ToString("dd-MM-yyyy"),
                   }).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data1 });

            }

            #endregion
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data.ToList() });
        }
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( Product product)
        {
            try
            {
                // Checking for already exist in system
                if (_db.Products.AsNoTracking().Where(d => d.Name.Trim().ToLower() == product.Name.Trim().ToLower()).Any())
                {
                    return Json(new { message = "The Product is already exist", returnStatus = 1 });
                }
                _db.Products.Add(product);
                _db.SaveChanges();
                return Json(new { message = "Successfully Added", returnStatus = 0, create = "true" });
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, returnStatus = 1 });
            }
        }


        public ActionResult Edit(int id)
        {
            var data = _db.Products.AsNoTracking().Where(a => a.Id == id).FirstOrDefault();
            return PartialView("Edit", data);
        }
          [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Product product)
        {
            try
            {
                // Checking if any Item except the current item already exist then Not allow to enter
                if (_db.Products.AsNoTracking().Where(d => d.Name.Trim().ToLower() == product.Name.Trim().ToLower() && d.Id!=product.Id).Any())
                {
                    return Json(new { message = "The Product is already exist with same name", returnStatus = 1 });
                }

                Product acc = new Product();
                acc = _db.Products.Where(i => i.Id == product.Id).FirstOrDefault();
                acc.Id = product.Id;
                acc.Name = product.Name;
                acc.Description = product.Description;
               _db.Products.Update(acc);
               _db.SaveChanges();
                return Json(new { message = "Successfully Updated", returnStatus = 0, create = "true" });
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, returnStatus = 1 });
            }
        }
        public ActionResult Delete(int id)
        {

            var data = _db.Products.AsNoTracking().Where(a => a.Id == id).FirstOrDefault();
            return PartialView("Delete", data);
        }
        [HttpPost]
        public ActionResult Delete(int id, Product collection)
        {
            try
            {
                _db.Products.Remove(collection);
                _db.SaveChanges();

                var d = _db.Products.Count();
                if (d < 1)
                {
                    return Json(new { message = "Deleted", returnStatus = 3, create = "true" });
                }
                return Json(new { message = "Deleted", returnStatus = 0, create = "true" });
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, returnStatus = 1 });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

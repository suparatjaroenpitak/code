using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyEcommerceAdmin.Models;
using System.Data;
using System.IO;

namespace MyEcommerceAdmin.Controllers
{
    public class AccountController : Controller
    {
        MyEcommerceDbContext db = new MyEcommerceDbContext();

        // GET: Account
        public ActionResult Index()
        {
            this.GetDefaultData();

            var usr = db.Customers.Find(TempShpData.UserID);
            return View(usr);

        }


        //REGISTER CUSTOMER
        [HttpPost]
        public ActionResult Register(Customer cust)
        {
            if (ModelState.IsValid)
            {
                db.Customers.Add(cust);
                db.SaveChanges();

                Session["username"] = cust.UserName;
                TempShpData.UserID = GetUser(cust.UserName).CustomerID;
                return RedirectToAction("Index", "Home");
            }
            return View();
        }



        //LOG IN
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection formColl)
        {
            string usrName = formColl["UserName"];
            string Pass = formColl["Password"];

            if (ModelState.IsValid)
            {
                var cust = (from m in db.Customers
                            where (m.UserName == usrName && m.Password == Pass)
                            select m).SingleOrDefault();

                if (cust != null)
                {
                    TempShpData.UserID = cust.CustomerID;
                    Session["username"] = cust.UserName;
                    return RedirectToAction("Index", "Home");
                }

            }
            return View();
        }

        //LOG OUT
        public ActionResult Logout()
        {
            Session["username"] = null;
            TempShpData.UserID = 0;
            TempShpData.items = null;
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public Customer GetUser(string _usrName)
        {
            var cust = (from c in db.Customers
                        where c.UserName == _usrName
                        select c).FirstOrDefault();
            return cust;
        }

        //UPDATE CUSTOMER DATA
        [HttpPost]
        public ActionResult Update(Customer cust)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cust).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Session["username"] = cust.UserName;
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // CREATE: Customer

        public ActionResult RegisterCustomer()
        {
            return View();
        }

        // REGISTER CUSTOMER (POST method for AJAX submission)
        [HttpPost]
        // Modified action to return JsonResult for AJAX
        public JsonResult RegisterCustomer(CustomerVM cvm)
        {
            // Check for server-side validation errors
            if (!ModelState.IsValid)
            {
                // Collect validation errors to return in JSON
                var errors = ModelState.Keys
                    .Where(key => ModelState[key].Errors.Any())
                    .ToDictionary(
                        key => key,
                        key => ModelState[key].Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                // Return JSON indicating failure and including errors
                return Json(new { success = false, errors = errors });
            }

            // Check if a picture was uploaded
            if (cvm.Picture == null)
            {
                // Return JSON indicating failure because picture is required
                return Json(new { success = false, message = "Profile picture is required." });
            }

            try
            {
                // Process the uploaded picture
                string filePath = Path.Combine("~/Images", Guid.NewGuid().ToString() + Path.GetExtension(cvm.Picture.FileName));
                string serverPath = Server.MapPath(filePath);

                // Ensure the directory exists
                var directory = Path.GetDirectoryName(serverPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                cvm.Picture.SaveAs(serverPath);

                // Map ViewModel to Model, handling potential null dates safely
                DateTime? dob = (cvm.DateofBirth.HasValue && cvm.DateofBirth.Value >= new DateTime(1753, 1, 1)) ? cvm.DateofBirth : null;
                DateTime? lastLogin = (cvm.LastLogin.HasValue && cvm.LastLogin.Value >= new DateTime(1753, 1, 1)) ? cvm.LastLogin : null;
                DateTime? created = (cvm.Created.HasValue && cvm.Created.Value >= new DateTime(1753, 1, 1)) ? cvm.Created : null;

                Customer c = new Customer
                {
                    CustomerID = cvm.CustomerID, // Note: CustomerID is usually auto-generated by the database
                    First_Name = cvm.First_Name,
                    Last_Name = cvm.Last_Name,
                    UserName = cvm.UserName,
                    Password = cvm.Password, // Consider hashing passwords!
                    Gender = cvm.Gender,
                    DateofBirth = dob,
                    Country = cvm.Country,
                    City = cvm.City,
                    PostalCode = cvm.PostalCode,
                    Email = cvm.Email,
                    Phone = cvm.Phone,
                    Address = cvm.Address,
                    PicturePath = filePath, // Save the relative path
                    status = cvm.status,
                    LastLogin = lastLogin,
                    Created = created,
                    Notes = cvm.Notes
                };

                // Add customer to database and save changes
                db.Customers.Add(c);
                db.SaveChanges();

                // Return JSON indicating success and the redirect URL
                return Json(new { success = true, message = "Registration successful!", redirectUrl = Url.Action("Login", "Account") });
            }
            catch (Exception ex)
            {
                // Log the exception (recommended)
                // For simplicity, returning a generic error message
                // In production, avoid exposing detailed error information
                return Json(new { success = false, message = "An error occurred during registration. Please try again." });
            }
        }


    }
}
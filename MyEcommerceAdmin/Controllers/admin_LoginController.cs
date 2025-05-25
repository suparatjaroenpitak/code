using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyEcommerceAdmin.Models;

namespace MyEcommerceAdmin.Controllers
{
    public class admin_LoginController : Controller
    {
        MyEcommerceDbContext db = new MyEcommerceDbContext();

        // GET: admin_Login
        public ActionResult Index()
        {
            // Returns the login view
            return View();
        }

        [HttpPost]
        public ActionResult Login(admin_Login login)
        {
            object responseData;

            // Check if the model state is valid based on data annotations
            if (!ModelState.IsValid)
            {
                // If validation fails, collect error messages
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                             .Select(e => e.ErrorMessage)
                                             .ToList();
                // Prepare JSON response for validation failure
                responseData = new { success = false, message = "Validation failed.", errors = errors };
            }
            else
            {
                // Attempt to find the user in the database based on username and password
                var loginInfo = db.admin_Login
                                 .Where(x => x.UserName == login.UserName && x.Password == login.Password)
                                 .FirstOrDefault();

                // Check if user was found
                if (loginInfo != null)
                {
                    // Set session variables upon successful login
                    Session["username"] = loginInfo.UserName;
                    // Assuming TemData is a static helper class for temporary data
                    TemData.EmpID = loginInfo.EmpID;

                    // Prepare JSON response for successful login, including redirect URL
                    responseData = new { success = true, message = "Login successful.", redirectUrl = Url.Action("Index", "Dashboard") };
                }
                else
                {
                    // Prepare JSON response for invalid credentials
                    responseData = new { success = false, message = "Invalid username or password." };
                }
            }

            // Return the response data as JSON
            return Json(responseData);
        }

        // Logout Server Code
        public ActionResult Logout()
        {
            // Clear the current session
            Session.Clear();

            // Prepare JSON response for successful logout, including redirect URL
            var responseData = new { success = true, message = "Logout successful.", redirectUrl = Url.Action("Index", "admin_Login") };

            // Return the response data as JSON
            return Json(responseData);
        }
    }
}

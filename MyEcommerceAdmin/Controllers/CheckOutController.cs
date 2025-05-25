using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyEcommerceAdmin.Models;
using System.Data;
using System.Data.Entity; // Required for database transactions

namespace MyEcommerceAdmin.Controllers
{
    public class CheckOutController : Controller
    {
        MyEcommerceDbContext db = new MyEcommerceDbContext();

        // GET: CheckOut
        public ActionResult Index()
        {
            // Populate ViewBag with payment methods for a dropdown list
            ViewBag.PayMethod = new SelectList(db.PaymentTypes, "PayTypeID", "TypeName");

            // Get default data for the checkout view (assuming GetDefaultData() exists elsewhere)
            var data = this.GetDefaultData();

            // Return the checkout view with the data
            return View(data);
        }

        //PLACE ORDER--LAST STEP
        [HttpPost] // It's good practice to make actions that modify data POST actions
        public ActionResult PlaceOrder(FormCollection getCheckoutDetails)
        {
            // Prepare a variable to hold the response data
            object responseData;

            // Use a try-catch block to handle potential errors during the process
            try
            {
                // --- Manual ID Generation (Consider using Identity Columns in DB instead) ---
                // This approach is prone to race conditions in a multi-user environment.
                int shpID = 1;
                if (db.ShippingDetails.Any()) // Use Any() for efficiency check if any records exist
                {
                    shpID = db.ShippingDetails.Max(x => x.ShippingID) + 1;
                }
                int payID = 1;
                if (db.Payments.Any())
                {
                    payID = db.Payments.Max(x => x.PaymentID) + 1;
                }
                int orderID = 1;
                if (db.Orders.Any())
                {
                    orderID = db.Orders.Max(x => x.OrderID) + 1;
                }
                // --- End Manual ID Generation ---

                // --- Start Database Operations (Consider using a single transaction) ---
                // Creating and saving Shipping Details
                ShippingDetail shpDetails = new ShippingDetail();
                shpDetails.ShippingID = shpID; // Assigned manually
                shpDetails.FirstName = getCheckoutDetails["FirstName"];
                shpDetails.LastName = getCheckoutDetails["LastName"];
                shpDetails.Email = getCheckoutDetails["Email"];
                shpDetails.Mobile = getCheckoutDetails["Mobile"];
                shpDetails.Address = getCheckoutDetails["Address"];
                shpDetails.City = getCheckoutDetails["City"];
                shpDetails.PostCode = getCheckoutDetails["PostCode"];
                db.ShippingDetails.Add(shpDetails);
                db.SaveChanges(); // First SaveChanges

                // Creating and saving Payment Details
                Payment pay = new Payment();
                pay.PaymentID = payID; // Assigned manually
                // Ensure conversion is safe, maybe use TryParse or check if value exists
                pay.Type = Convert.ToInt32(getCheckoutDetails["PayMethod"]);
                db.Payments.Add(pay);
                db.SaveChanges(); // Second SaveChanges

                // Creating and saving Order Details
                Order o = new Order();
                o.OrderID = orderID; // Assigned manually
                // Assuming TempShpData.UserID and TempShpData.items are populated elsewhere
                o.CustomerID = TempShpData.UserID;
                o.PaymentID = payID;
                o.ShippingID = shpID;
                // Ensure conversions are safe
                o.Discount = Convert.ToInt32(getCheckoutDetails["discount"]);
                o.TotalAmount = Convert.ToInt32(getCheckoutDetails["totalAmount"]);
                o.isCompleted = true;
                o.OrderDate = DateTime.Now;
                db.Orders.Add(o);
                db.SaveChanges(); // Third SaveChanges

                // Adding Order Items (OrderDetails)
                // Assuming TempShpData.items is a list of OrderDetail objects or similar
                if (TempShpData.items != null) // Check if items exist
                {
                    // Consider adding all OrderDetails and calling SaveChanges once after the loop
                    foreach (var OD in TempShpData.items)
                    {
                        OD.OrderID = orderID;
                        // Note: Setting navigation properties like OD.Order and OD.Product
                        // is often not necessary when adding related entities if relationships are configured correctly.
                        // The foreign key (OrderID, ProductID) is usually sufficient.
                        // Finding entities inside a loop and saving each time is inefficient.
                        // OD.Order = db.Orders.Find(orderID); // Less efficient inside loop
                        // OD.Product = db.Products.Find(OD.ProductID); // Less efficient inside loop

                        db.OrderDetails.Add(OD);
                        db.SaveChanges(); // SaveChanges inside loop - very inefficient and multiple transactions
                    }
                }
                // --- End Database Operations ---


                // Prepare JSON response for success, including the redirect URL
                responseData = new
                {
                    success = true,
                    message = "Order placed successfully.",
                    redirectUrl = Url.Action("Index", "ThankYou") // URL to redirect to on client-side
                };
            }
            catch (Exception ex)
            {
                // Log the exception (recommended in a real application)
                // System.Diagnostics.Trace.TraceError("Error placing order: " + ex.Message);
                // Return a JSON response indicating failure
                responseData = new
                {
                    success = false,
                    message = "An error occurred while placing your order. Please try again.",
                    errorDetails = ex.Message // Optionally include error details for debugging (be cautious in production)
                };
            }

            // Return the response data as JSON
            return Json(responseData);
        }

        // Logout Server Code (assuming this was part of the same controller context)
        public ActionResult Logout()
        {
            // Clear the current session
            Session.Clear();

            // Prepare JSON response for successful logout, including redirect URL
            var responseData = new { success = true, message = "Logout successful.", redirectUrl = Url.Action("Index", "admin_Login") };

            // Return the response data as JSON
            return Json(responseData);
        }

        // Assuming this method exists elsewhere in your controller or a base controller
        // private object GetDefaultData()
        // {
        //     // Implementation to get data for the Index view
        //     // return some data;
        // }
    }
}

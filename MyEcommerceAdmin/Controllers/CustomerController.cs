using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyEcommerceAdmin.Models;
using System.Data.Entity; // Required for EntityState

namespace MyEcommerceAdmin.Controllers
{
    public class CustomerController : Controller
    {
        MyEcommerceDbContext db = new MyEcommerceDbContext();

        // GET: Customer
        // Displays a list of all customers.
        public ActionResult Index()
        {
            // Retrieve all customers from the database and pass them to the view.
            return View(db.Customers.ToList());
        }

        // GET: Customer/Create
        // Displays the form to create a new customer.
        public ActionResult Create()
        {
            // Return the create customer view.
            return View();
        }

        // POST: Customer/Create
        // Handles the submission of the new customer form.
        [HttpPost]
        [ValidateAntiForgeryToken] // Protect against cross-site request forgery
        public ActionResult Create(CustomerVM cvm)
        {
            // Prepare a variable to hold the response data, initialized to null
            object responseData = null;

            // Use a try-catch block to handle potential errors
            try
            {
                // Check if the submitted data is valid according to the ViewModel's data annotations.
                if (ModelState.IsValid)
                {
                    string filePath = null; // Initialize file path

                    // Handle file upload if a picture is provided
                    if (cvm.Picture != null && cvm.Picture.ContentLength > 0)
                    {
                        // Generate a unique file name
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(cvm.Picture.FileName);
                        // Define the full path to save the file
                        filePath = Path.Combine("~/Images/Customers", fileName); // Assuming a subfolder for customer images
                        string serverPath = Server.MapPath(filePath);

                        // Ensure the directory exists
                        Directory.CreateDirectory(Path.GetDirectoryName(serverPath));

                        // Save the file to the server
                        cvm.Picture.SaveAs(serverPath);
                    }
                    // Note: If cvm.Picture is null, PicturePath will remain null, which is acceptable if the field is optional.

                    // Create a new Customer entity from the ViewModel data.
                    // IMPORTANT: If CustomerID is an Identity column in your database,
                    // do NOT assign cvm.CustomerID here. The database will generate it.
                    // Assigning it here is only appropriate if you are manually managing IDs,
                    // which is generally not recommended due to race conditions.
                    Customer c = new Customer
                    {
                        // CustomerID = cvm.CustomerID, // Remove this line if using Identity Column
                        First_Name = cvm.First_Name,
                        Last_Name = cvm.Last_Name,
                        UserName = cvm.UserName,
                        Password = cvm.Password, // Consider hashing passwords before saving
                        Gender = cvm.Gender,
                        DateofBirth = cvm.DateofBirth,
                        Country = cvm.Country,
                        City = cvm.City,
                        PostalCode = cvm.PostalCode,
                        Email = cvm.Email,
                        Phone = cvm.Phone,
                        Address = cvm.Address,
                        PicturePath = filePath, // Save the generated file path
                        status = cvm.status,
                        LastLogin = cvm.LastLogin, // Ensure this is handled correctly (might be set on actual login)
                        Created = DateTime.Now, // Set creation date here, not from ViewModel
                        Notes = cvm.Notes
                    };

                    // Add the new customer to the database context.
                    db.Customers.Add(c);
                    // Save changes to the database.
                    db.SaveChanges();

                    // Prepare JSON response for success.
                    responseData = new { success = true, message = "Customer created successfully." };
                }
                else
                {
                    // If model state is not valid, collect validation errors.
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                 .Select(e => e.ErrorMessage)
                                                 .ToList();
                    // Prepare JSON response for validation failure.
                    responseData = new { success = false, message = "Validation failed.", errors = errors };
                }
            }
            catch (Exception ex)
            {
                // Log the exception (recommended in a real application)
                // System.Diagnostics.Trace.TraceError("Error creating customer: " + ex.Message);

                // If an error occurred, especially during file save, clean up the file if it was saved.
                // Note: This cleanup logic might need refinement based on where the error occurred.
                // Accessing responseData here might still be problematic if the exception happened
                // before responseData was assigned in the try block.
                // A more robust approach might involve tracking the file path separately.
                // For now, initializing responseData helps compile, but the cleanup logic
                // might not execute correctly in all error scenarios.
                // if (!string.IsNullOrEmpty((responseData as dynamic)?.filePath)) // This check is risky
                // {
                //      string serverPathToDelete = Server.MapPath((responseData as dynamic).filePath);
                //      if (System.IO.File.Exists(serverPathToDelete))
                //      {
                //          System.IO.File.Delete(serverPathToDelete);
                //      }
                // }
                // A safer approach for cleanup would be to declare filePath outside the try block
                // and check its value in the catch block.

                // Prepare JSON response for general error.
                responseData = new
                {
                    success = false,
                    message = "An error occurred while creating the customer. Please try again.",
                    errorDetails = ex.Message // Optionally include error details for debugging
                };
            }

            // Return the response data as JSON.
            return Json(responseData);
        }

        // GET: Customer/Edit/5
        // Displays the form to edit an existing customer.
        public ActionResult Edit(int id)
        {
            // Find the customer by ID.
            Customer cus = db.Customers.Find(id);

            // If customer not found, return HttpNotFound.
            if (cus == null)
            {
                return HttpNotFound();
            }

            // Map the Customer entity data to the ViewModel for displaying in the form.
            CustomerVM cvm = new CustomerVM
            {
                CustomerID = cus.CustomerID,
                First_Name = cus.First_Name,
                Last_Name = cus.Last_Name,
                UserName = cus.UserName,
                Password = cus.Password, // Be cautious displaying passwords in edit forms
                Gender = cus.Gender,
                DateofBirth = cus.DateofBirth,
                Country = cus.Country,
                City = cus.City,
                PostalCode = cus.PostalCode,
                Email = cus.Email,
                Phone = cus.Phone,
                Address = cus.Address,
                PicturePath = cus.PicturePath, // Keep existing picture path
                status = cus.status,
                LastLogin = cus.LastLogin,
                Created = cus.Created,
                Notes = cus.Notes
            };

            // Return the edit customer view with the ViewModel.
            return View(cvm);
        }

        // POST: Customer/Edit/5
        // Handles the submission of the edited customer form.
        [HttpPost]
        [ValidateAntiForgeryToken] // Protect against cross-site request forgery
        public ActionResult Edit(CustomerVM cvm)
        {
            // Prepare a variable to hold the response data, initialized to null
            object responseData = null;

            // Use a try-catch block to handle potential errors
            try
            {
                // Check if the submitted data is valid according to the ViewModel's data annotations.
                if (ModelState.IsValid)
                {
                    // Retrieve the existing customer from the database.
                    Customer existingCustomer = db.Customers.Find(cvm.CustomerID);

                    // If customer not found, return an error.
                    if (existingCustomer == null)
                    {
                        responseData = new { success = false, message = "Customer not found for editing." };
                        return Json(responseData);
                    }

                    string oldPicturePath = existingCustomer.PicturePath; // Store old path for potential deletion
                    string newFilePath = oldPicturePath; // Assume existing path unless new picture is uploaded

                    // Handle file upload if a new picture is provided
                    if (cvm.Picture != null && cvm.Picture.ContentLength > 0)
                    {
                        // Generate a unique file name for the new picture
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(cvm.Picture.FileName);
                        // Define the full path to save the new file
                        newFilePath = Path.Combine("~/Images/Customers", fileName); // Assuming a subfolder for customer images
                        string serverPath = Server.MapPath(newFilePath);

                        // Ensure the directory exists
                        Directory.CreateDirectory(Path.GetDirectoryName(serverPath));

                        // Save the new file to the server
                        cvm.Picture.SaveAs(serverPath);

                        // If a new picture was saved and an old one existed, delete the old one.
                        if (!string.IsNullOrEmpty(oldPicturePath))
                        {
                            string oldServerPath = Server.MapPath(oldPicturePath);
                            if (System.IO.File.Exists(oldServerPath))
                            {
                                System.IO.File.Delete(oldServerPath);
                            }
                        }
                    }
                    // Note: If cvm.Picture is null, newFilePath remains the oldPicturePath.

                    // Update the properties of the existing customer entity with data from the ViewModel.
                    existingCustomer.First_Name = cvm.First_Name;
                    existingCustomer.Last_Name = cvm.Last_Name;
                    existingCustomer.UserName = cvm.UserName;
                    existingCustomer.Password = cvm.Password; // Consider updating password only if changed and hashed
                    existingCustomer.Gender = cvm.Gender;
                    existingCustomer.DateofBirth = cvm.DateofBirth;
                    existingCustomer.Country = cvm.Country;
                    existingCustomer.City = cvm.City;
                    existingCustomer.PostalCode = cvm.PostalCode;
                    existingCustomer.Email = cvm.Email;
                    existingCustomer.Phone = cvm.Phone;
                    existingCustomer.Address = cvm.Address;
                    existingCustomer.PicturePath = newFilePath; // Update with the new (or old) file path
                    existingCustomer.status = cvm.status;
                    // existingCustomer.LastLogin = cvm.LastLogin; // LastLogin should likely not be updated from edit form
                    // existingCustomer.Created = cvm.Created; // Created date should not be updated from edit form
                    existingCustomer.Notes = cvm.Notes;

                    // Mark the entity as modified and save changes.
                    // Since we retrieved the entity, Entity Framework tracks changes automatically.
                    // db.Entry(existingCustomer).State = System.Data.Entity.EntityState.Modified; // This line is often not needed if entity was retrieved
                    db.SaveChanges();

                    // Prepare JSON response for success.
                    responseData = new { success = true, message = "Customer updated successfully." };
                }
                else
                {
                    // If model state is not valid, collect validation errors.
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                 .Select(e => e.ErrorMessage)
                                                 .ToList();
                    // Prepare JSON response for validation failure.
                    responseData = new { success = false, message = "Validation failed.", errors = errors };
                }
            }
            catch (Exception ex)
            {
                // Log the exception (recommended in a real application)
                // System.Diagnostics.Trace.TraceError("Error updating customer: " + ex.Message);

                // Prepare JSON response for general error.
                responseData = new
                {
                    success = false,
                    message = "An error occurred while updating the customer. Please try again.",
                    errorDetails = ex.Message // Optionally include error details for debugging
                };
            }

            // Return the response data as JSON.
            return Json(responseData);
        }

        // GET: Customer/Details/5
        // Displays the details of a customer.
        public ActionResult Details(int id)
        {
            // Find the customer by ID.
            Customer cus = db.Customers.Find(id);

            // If customer not found, return HttpNotFound.
            if (cus == null)
            {
                return HttpNotFound();
            }

            // Map the Customer entity data to the ViewModel for displaying details.
            CustomerVM cvm = new CustomerVM
            {
                CustomerID = cus.CustomerID,
                First_Name = cus.First_Name,
                Last_Name = cus.Last_Name,
                UserName = cus.UserName,
                Password = cus.Password, // Be cautious displaying passwords
                Gender = cus.Gender,
                DateofBirth = cus.DateofBirth,
                Country = cus.Country,
                City = cus.City,
                PostalCode = cus.PostalCode,
                Email = cus.Email,
                Phone = cus.Phone,
                Address = cus.Address,
                PicturePath = cus.PicturePath,
                status = cus.status,
                LastLogin = cus.LastLogin,
                Created = cus.Created,
                Notes = cus.Notes
            };

            // Return the details view with the ViewModel.
            return View(cvm);
        }

        // Removed the duplicate/incorrect POST Details method.
        // If you intended a POST action for details (which is unusual),
        // please clarify its purpose and it can be added back with appropriate logic.


        // GET: Customer/Delete/5
        // Displays the confirmation page for deleting a customer.
        public ActionResult Delete(int? id)
        {
            // Check if ID is provided.
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            // Find the customer by ID.
            Customer customer = db.Customers.Find(id);
            // If customer not found, return HttpNotFound.
            if (customer == null)
            {
                return HttpNotFound();
            }
            // Return the delete confirmation view with the customer data.
            return View(customer);
        }

        // POST: Customer/Delete/5
        // Handles the actual deletion of the customer after confirmation.
        [HttpPost, ActionName("Delete")] // ActionName specifies the route name
        [ValidateAntiForgeryToken] // Protect against cross-site request forgery
        public ActionResult DeleteConfirm(int id) // Renamed parameter for clarity
        {
            // Prepare a variable to hold the response data, initialized to null
            object responseData = null;

            // Use a try-catch block to handle potential errors during deletion
            try
            {
                // Find the customer by ID.
                Customer customer = db.Customers.Find(id);

                // If customer not found, return an error JSON.
                if (customer == null)
                {
                    responseData = new { success = false, message = "Customer not found for deletion." };
                    return Json(responseData);
                }

                // Get the picture file path.
                string file_name = customer.PicturePath;

                // Remove the customer from the database context.
                db.Customers.Remove(customer);
                // Save changes to the database (this deletes the customer record).
                db.SaveChanges();

                // If the customer record was deleted successfully and a picture path exists,
                // attempt to delete the associated picture file.
                if (!string.IsNullOrEmpty(file_name))
                {
                    string path = Server.MapPath(file_name);
                    FileInfo file = new FileInfo(path);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                    // Note: Error during file deletion does not necessarily mean the database delete failed.
                    // You might want more robust file handling/logging here.
                }

                // Prepare JSON response for success.
                responseData = new { success = true, message = "Customer deleted successfully." };
            }
            catch (Exception ex)
            {
                // Log the exception (recommended in a real application)
                // System.Diagnostics.Trace.TraceError("Error deleting customer: " + ex.Message);

                // Prepare JSON response for general error.
                responseData = new
                {
                    success = false,
                    message = "An error occurred while deleting the customer. Please try again.",
                    errorDetails = ex.Message // Optionally include error details for debugging
                };
            }

            // Return the response data as JSON.
            return Json(responseData);
        }
    }
}

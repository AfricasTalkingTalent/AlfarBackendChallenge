using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SampleProject.Models;
using System.Net.Mail;
using System.Web.Configuration;
using System.Data.SqlClient;
using AfricasTalkingCS;
namespace SampleProject.Controllers
{
    public class CustomersController : Controller
    {
        private DataDb db = new DataDb();

        // GET: Customers
        public ActionResult Index()
        {
            return View(db.Customer.ToList());
        }

        // GET: Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customer.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: Customers/Create
        public ActionResult Create()
        {
            return PartialView("Create");
        }

        // POST: Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Customer customer)
        {
            try
            {
                int result = 0;
                if (ModelState.IsValid)
                {
                    using (var db = new DataDb())
                    {
                        var FirstName = new SqlParameter("@FirstName", customer.FirstName);
                        var LastName = new SqlParameter("@LastName", customer.LastName);
                        var Email = new SqlParameter("@Email", customer.Email);
                        var MobileNo = new SqlParameter("@MobileNo", customer.MobileNo);
                        result=db.Database.ExecuteSqlCommand("Customer_Insert @FirstName,@LastName,@Email,@MobileNo", FirstName, LastName, Email, MobileNo);
                    }
                    if (result > 0)
                    {
                        if (!SendEmail(customer.Email))
                        {
                            TempData["Message"] = "Email was not sent";
                        }
                        if (!sendSMS(customer.MobileNo))
                        {
                            TempData["Message"] = "SMS was not sent";
                        }
                    }
                    return PartialView("Create");
                }

                return PartialView("Create", customer);
            }
            catch (Exception)
            {
                return PartialView("Create", customer);
            }
           
        }

        // GET: Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customer.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return PartialView("Edit", customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                int result = 0;
                using (var db = new DataDb())
                {
                    var Id = new SqlParameter("@Id", customer.Id);
                    var FirstName = new SqlParameter("@FirstName", customer.FirstName);
                    var LastName = new SqlParameter("@LastName", customer.LastName);
                    var Email = new SqlParameter("@Email", customer.Email);
                    var MobileNo = new SqlParameter("@MobileNo", customer.MobileNo);
                    result= db.Database.ExecuteSqlCommand("Customer_Update @Id,@FirstName,@LastName,@Email,@MobileNo",Id,FirstName, LastName, Email, MobileNo);
                }
                if (result > 0)
                {
                    TempData["Message"] = "Customer updated successfully";
                    return PartialView("Edit");
                }
                
            }
            return PartialView("Edit", customer);
        }

        // GET: Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customer.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return PartialView("Delete", customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                int result = 0;
                Customer customer = new Customer();
                using (var db = new DataDb())
                {
                     customer = db.Customer.Find(id);
                    var Id = new SqlParameter("@Id", id);
                    result=db.Database.ExecuteSqlCommand("Customer_Delete @Id", Id);
                }
                if (result > 0)
                {
                    TempData["Message"] = "Customer deleted successfully";
                    return PartialView("Delete", customer);
                }
                return PartialView("Delete", customer);
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public bool sendSMS(string mobilenumbers)
        {
            try
            {
                const string username = "sandbox"; 
                const string apikey = "ac1a13f7e77039a451afa2801351a39ee2b64b97a610acede5027434e32c1e51"; 
                var gateway = new AfricasTalkingGateway(username, apikey);
                string recipients = mobilenumbers; string message = "Welcome to test message";
                try
                {
                    var sms = gateway.SendMessage(recipients, message);
                    foreach (var res in sms["SMSMessageData"]["Recipients"])
                    {
                        Console.WriteLine((string)res["number"] + ": ");
                        Console.WriteLine((string)res["status"] + ": ");
                        Console.WriteLine((string)res["messageId"] + ": ");
                        Console.WriteLine((string)res["cost"] + ": ");
                    }
                    return true;
                }
                catch (AfricasTalkingGatewayException exception)
                {
                    Console.WriteLine(exception);
                    return false;
                }
            }
            catch (Exception)
            {

                return false;
            }
        }
        public bool SendEmail(string email)
        {
            try
            {
                var fromEmail = new MailAddress(WebConfigurationManager.AppSettings["FromEmail"], WebConfigurationManager.AppSettings["config:Company"]);
                var toEmail = new MailAddress(email);
                var fromEmailPassword = WebConfigurationManager.AppSettings["EmailPassword"]; // Replace with actual password
                string subject = "Welcome To Francis Sample";
                string body = "Use password below to login." +
                    "<br/>Thank you for registering to Francis Sample Test." +
                    "<br/>Welcome on Board"+
                    "<br/><br/>";

                var smtp = new SmtpClient
                {
                    Host = WebConfigurationManager.AppSettings["Host"],
                    Port = Convert.ToInt32(WebConfigurationManager.AppSettings["Port"]),
                    EnableSsl = Convert.ToBoolean(WebConfigurationManager.AppSettings["Ssl"]),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
                };

                using (var message = new MailMessage(fromEmail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                    smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}

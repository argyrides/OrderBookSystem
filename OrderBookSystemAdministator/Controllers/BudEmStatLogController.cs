using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderBookSystemClient.Data;
using OrderBookSystemAdministator.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace OrderBookSystemAdministator.Controllers
{
    public class BudEmStatLogController : Controller
    {
        DBContext db = new DBContext();

        #region Budget
        public IActionResult Budget()
        {
            double budget = 0;
            var getCompletedOrders = db.Orders.Where(a => a.order_status == "APPROVE").ToList();
            var getUsername = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == getUsername).Select(a => a.user_id).FirstOrDefault();

            if (getUsername != null && getUserId != 0)
            {
                foreach (var item in getCompletedOrders)
                {
                    budget += item.order_total_price;
                }

                return View(budget);
            }
            else
            {
                return RedirectToAction("Login", "AdminAccount");

            }
        }
        #endregion

        #region EmailManagement
        public IActionResult ManageEmails()
        {
            var getUsername = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == getUsername).Select(a => a.user_id).FirstOrDefault();
            var getEmailsList = db.Emails.Where(a => a.email_id >= 0).ToList();

            if (getUsername != null && getUserId != 0)
            {

                foreach (var item in getEmailsList)
                {
                    item.Z_email_id = db.EmailTypes.Where(a => a.type_id == item.type_id).FirstOrDefault();
                }

                return View(getEmailsList);

            }
            else
            {
                return RedirectToAction("Login", "AdminAccount");
            }
        }

        public IActionResult EmailsHaveSend()
        {
            var getUsername = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == getUsername).Select(a => a.user_id).FirstOrDefault();
            var getEmailsHaveSendList = db.EmailManagements.Where(a => a.emailmanagement_id >= 0).ToList();


            if (getUsername != null && getUserId != 0)
            {
                foreach (var item in getEmailsHaveSendList)
                {
                    item.Z_email_id = db.Emails.Where(a => a.email_id == item.email_id).FirstOrDefault();
                    item.Z_user_id = db.Users.Where(a => a.user_id == item.user_id).FirstOrDefault();
                }

                return View(getEmailsHaveSendList);

            }
            else
            {
                return RedirectToAction("Login", "AdminAccount");

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddEmails(string AddEmailData)
        {
            KeyValuePair<string, string> GetResult;
            var sa = new JsonSerializerSettings();
            OrderBookSystemClient.Data.Entities.Email EmailItem = new OrderBookSystemClient.Data.Entities.Email();

            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(AddEmailData);
            var getExistsEmails = db.Emails.Where(a => a.type_id >= 0).ToList();

            bool isforaddbool = Array_With_JSON_Res.isforaddbool;
            string email_content = Array_With_JSON_Res.emailcontent;
            string email_type = Array_With_JSON_Res.emailtype;
            int type_id = db.EmailTypes.Where(a => a.type_title == email_type).Select(a=> a.type_id).FirstOrDefault();

            foreach (var item in getExistsEmails)
            {
                if(item.type_id == type_id && isforaddbool == true)
                {
                    GetResult = new KeyValuePair<string, string>("F", "Email with specific type already exist. You can't add another email for this type.");
                    return Json(GetResult, sa);
                }
            }

            if (email_content == "")
            {
                GetResult = new KeyValuePair<string, string>("F", "Email not Successfully Added");
                return Json(GetResult, sa);
            }
            else
            {
                if (isforaddbool == true)
                {
                    try
                    {
                        EmailItem.email_content = email_content;
                        EmailItem.type_id = type_id;
                        db.Emails.Add(EmailItem);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        GetResult = new KeyValuePair<string, string>("EX", "Exception Error Please contact Admin :" + ex.Message);
                        return Json(GetResult, sa);
                    }
                    GetResult = new KeyValuePair<string, string>("S", "Email Added Successfully!");
                }
                else
                {
                    try
                    {
                        int email_id = Array_With_JSON_Res.emailid;
                        var findSpecificEmail = db.Emails.Find(email_id);
                        findSpecificEmail.email_content = email_content;
                        findSpecificEmail.type_id = type_id;
                        db.Emails.Update(findSpecificEmail);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        GetResult = new KeyValuePair<string, string>("EX", "Exception Error Please contact Admin :" + ex.Message);
                        return Json(GetResult, sa);
                    }
                    GetResult = new KeyValuePair<string, string>("S", "Email Updated Successfully!");
                }
            }

            return Json(GetResult, sa);
        }

        [HttpPost]
        public JsonResult DeleteEmails(string DeleteEmailsData)
        {
            KeyValuePair<string, string> GetResult;
            var sa = new JsonSerializerSettings();
            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(DeleteEmailsData);

            int email_id = Array_With_JSON_Res.emailid;

            OrderBookSystemClient.Data.Entities.Email emailItem = db.Emails.Find(email_id);


            try
            {
                db.Emails.Remove(emailItem);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                GetResult = new KeyValuePair<string, string>("EX", "Exception Error Occures during Delete, Please contact Admin :" + ex.Message);
                return Json(GetResult, sa);
            }
            GetResult = new KeyValuePair<string, string>("S", "Email Deleted Successfully!");

            return Json(GetResult, sa);

        }


        #endregion

        #region Statistics
        public IActionResult Statistics()
        {
            var getApprovedOrders = db.Orders.Where(a => a.order_status == "APPROVE").ToList();
            var getUsername = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == getUsername).Select(a => a.user_id).FirstOrDefault();

            List<OrderBookSystemClient.Data.Entities.OrderDetail> getApprovedOrdersDetails = new List<OrderBookSystemClient.Data.Entities.OrderDetail>();
            OrderBookSystemClient.Data.Entities.OrderDetail orderDetailItem = new OrderBookSystemClient.Data.Entities.OrderDetail();

            List<string> countTypesForPurchasedBooks = new List<string>();
            List<string> countPurchasedBooks = new List<string>();

            List<StatisticsModel> statisticsModelList = new List<StatisticsModel>();
            List<StatisticsModel> statisticsModelList2 = new List<StatisticsModel>();
            if (getUsername != null && getUserId != 0)
            {
                foreach (var item in getApprovedOrders)
                {
                    var getApprovedOrdersDetailsSecondList = db.OrdersDetails.Where(a => a.order_id == item.order_id).ToList();

                    foreach (var item2 in getApprovedOrdersDetailsSecondList)
                    {
                        item2.Z_bookuser_id = db.BooksUsers.Where(a => a.booksusers_id == item2.booksusers_id).FirstOrDefault();
                        if (item2.Z_bookuser_id.quantity > 1)
                        {
                            for (int i = 0; i < item2.Z_bookuser_id.quantity; i++)
                            {
                                getApprovedOrdersDetails.Add(item2);
                            }
                        }
                        else
                        {
                            getApprovedOrdersDetails.Add(item2);
                        }
                    }
                }

                foreach (var item in getApprovedOrdersDetails)
                {
                    string getTypesString;
                    string getBookString;
                    var getBooksHaveOrderedItem = db.BooksUsers.Where(a => a.booksusers_id == item.booksusers_id).Where(a => a.order_is_send == true).SingleOrDefault();
                    getBooksHaveOrderedItem.Z_book_id = db.Books.Where(a => a.book_id == item.Z_bookuser_id.book_id).FirstOrDefault();
                    getBooksHaveOrderedItem.Z_book_id.Z_BookType = db.BookTypes.Where(a => a.BookTypes_id == item.Z_bookuser_id.Z_book_id.bookTypes_id).FirstOrDefault();

                    getTypesString = item.Z_bookuser_id.Z_book_id.Z_BookType.bookTypes_name;
                    getBookString = item.Z_bookuser_id.Z_book_id.book_title;


                    countPurchasedBooks.Add(getBookString);
                    countTypesForPurchasedBooks.Add(getTypesString);
                }

                statisticsModelList2 = (from x in countPurchasedBooks
                                        group x by x into g
                                        let count = g.Count()
                                        select new StatisticsModel { statistics_book_name = g.Key, statistics_value = count }).ToList();



                statisticsModelList = (from x in countTypesForPurchasedBooks
                                       group x by x into g
                                       let count = g.Count()
                                       select new StatisticsModel { statistics_type = g.Key, statistics_value = count }).ToList();

                ViewData["statisticsModelList2"] = statisticsModelList2;


                return View(statisticsModelList);
            }
            else
            {
                return RedirectToAction("Login", "AdminAccount");

            }
        }
        #endregion

        #region LogPage
        public IActionResult Log()
        {
            var getApprovedOrders= db.Orders.Where(a => a.order_status == "APPROVE").ToList();
            var getUsername = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == getUsername).Select(a => a.user_id).FirstOrDefault();
            List<OrderBookSystemClient.Data.Entities.OrderDetail> getApprovedOrdersDetails = new List<OrderBookSystemClient.Data.Entities.OrderDetail>();
            OrderBookSystemClient.Data.Entities.OrderDetail orderDetailItem = new OrderBookSystemClient.Data.Entities.OrderDetail();

            List<LogModel> logModelList = new List<LogModel>();
            if (getUsername != null && getUserId != 0)
            {
                foreach (var item in getApprovedOrders)
                {
                    var getApprovedOrdersDetailsSecondList = db.OrdersDetails.Where(a => a.order_id == item.order_id).ToList();

                    foreach (var item2 in getApprovedOrdersDetailsSecondList)
                    {
                        item2.Z_bookuser_id = db.BooksUsers.Where(a => a.booksusers_id == item2.booksusers_id).FirstOrDefault();
                        getApprovedOrdersDetails.Add(item2);
                    }
                }

                int i = 1;
                foreach (var item in getApprovedOrdersDetails)
                {
                    LogModel logModel = new LogModel();
                    var getBooksHaveOrderedItem = db.BooksUsers.Where(a => a.booksusers_id == item.booksusers_id).Where(a => a.order_is_send == true).SingleOrDefault();
                    getBooksHaveOrderedItem.Z_book_id = db.Books.Where(a => a.book_id == item.Z_bookuser_id.book_id).FirstOrDefault();
                    getBooksHaveOrderedItem.Z_book_id.Z_BookType = db.BookTypes.Where(a => a.BookTypes_id == item.Z_bookuser_id.Z_book_id.bookTypes_id).FirstOrDefault();

                    logModel.book_author = item.Z_bookuser_id.Z_book_id.book_author;
                    logModel.book_title = item.Z_bookuser_id.Z_book_id.book_title;
                    logModel.order_date = item.Z_order_id.order_date;
                    logModel.returning_date = item.Z_bookuser_id.loan_returning_date;
                    logModel.book_type = item.Z_bookuser_id.Z_book_id.Z_BookType.bookTypes_name;
                    logModel.no = i;

                    logModelList.Add(logModel);
                    i++;
                }

                return View(logModelList);
            }
            else
            {
                return RedirectToAction("Login", "AdminAccount");

            }
        }
        #endregion
    }
}

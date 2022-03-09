using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderBookSystemAdministator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OrderBookSystemClient.Data;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Mail;

namespace OrderBookSystemAdministator.Controllers
{
    public class HomeAdminController : Controller
    {
        DBContext db = new DBContext();

        private readonly ILogger<HomeAdminController> _logger;

        public HomeAdminController(ILogger<HomeAdminController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == username).Select(a => a.user_id).FirstOrDefault();

            if (username == null && getUserId != 0)
            {
                return RedirectToAction("Login","AdminAccount");
            }
            else
            {
                List<OrderBookSystemClient.Data.Entities.User> getActiveUsers = db.Users.Where(a => a.is_approve == true).Where(a=> a.is_deleted == false).ToList();
                return View(getActiveUsers);
            }
        }

        #region manageUsers
        public IActionResult ManageUsers()
        {
            var username = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == username).Select(a => a.user_id).FirstOrDefault();

            if (username != null && getUserId != 0)
            {
                var getNotApprovedUsers = db.Users.Where(a => a.user_id >= 0).Where(a => a.is_approve == false).ToList();

                return View(getNotApprovedUsers);
            }
            else
            {
                return RedirectToAction("Login", "AdminAccount");
            }
        }

        public IActionResult UserApproval(int userid)
        {
            var username = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == username).Select(a => a.user_id).FirstOrDefault();

            var findUserId = db.Users.Find(userid);
            findUserId.is_approve = true;

            if (username != null && getUserId != 0)
            {
                try
                {
                    db.SaveChanges();
                }
                catch
                {
                    throw;
                }
                return RedirectToAction("ManageUsers");
            }
            else
            {
                return RedirectToAction("Login", "AdminAccount");

            }
        }

        [HttpPost]
        public JsonResult DeleteUsers(string DeleteUsersData)
        {
            KeyValuePair<string, string> GetResult;
            var sa = new JsonSerializerSettings();
            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(DeleteUsersData);

            int userid = Array_With_JSON_Res.userid;

            OrderBookSystemClient.Data.Entities.User userItem = db.Users.Find(userid);

            try
            {
                userItem.is_deleted = true;
                userItem.is_locked = true;
                userItem.is_approve = true;
                db.Users.Update(userItem);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                GetResult = new KeyValuePair<string, string>("EX", "Exception Error Occures during Delete, Please contact Admin :" + ex.Message);
                return Json(GetResult, sa);
            }
            GetResult = new KeyValuePair<string, string>("S", "User Deleted Successfully!");

            return Json(GetResult, sa);
        }

        [HttpPost]
        public JsonResult BlockUsers(string BlockUsersData)
        {
            KeyValuePair<string, string> GetResult;
            var sa = new JsonSerializerSettings();
            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(BlockUsersData);

            int userid = Array_With_JSON_Res.userid;

            OrderBookSystemClient.Data.Entities.User userItem = db.Users.Find(userid);

            try
            {
                userItem.is_locked = true;
                db.Users.Update(userItem);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                GetResult = new KeyValuePair<string, string>("EX", "Exception Error Occures during Blocking, Please contact Admin :" + ex.Message);
                return Json(GetResult, sa);
            }
            GetResult = new KeyValuePair<string, string>("S", "User Blocked Successfully!");

            return Json(GetResult, sa);
        }

        [HttpPost]
        public JsonResult UnBlockUsers(string UnBlockUsersData)
        {
            KeyValuePair<string, string> GetResult;
            var sa = new JsonSerializerSettings();
            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(UnBlockUsersData);

            int userid = Array_With_JSON_Res.userid;

            OrderBookSystemClient.Data.Entities.User userItem = db.Users.Find(userid);

            try
            {
                userItem.is_locked = false;
                db.Users.Update(userItem);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                GetResult = new KeyValuePair<string, string>("EX", "Exception Error Occures during UnBlocking, Please contact Admin :" + ex.Message);
                return Json(GetResult, sa);
            }
            GetResult = new KeyValuePair<string, string>("S", "User Unblocked Successfully!");

            return Json(GetResult, sa);
        }

        #endregion


        #region manageOrdersPage

        public IActionResult ManageOrders()
        {
            var username = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == username).Select(a => a.user_id).FirstOrDefault();

            if (username != null && getUserId != 0)
            {
                var getPendingOrders = db.Orders.Where(a => a.order_status == "PENDING").ToList();

                var getPendingOrdersDetailsList = new List<OrderBookSystemClient.Data.Entities.OrderDetail>();
                var getPendingOrdersDetailsSecondList = new List<OrderBookSystemClient.Data.Entities.OrderDetail>();

                foreach (var item in getPendingOrders)
                {
                    getPendingOrdersDetailsSecondList = db.OrdersDetails.Where(a => a.order_id == item.order_id).ToList();
                    var getZ_BookUser = new OrderBookSystemClient.Data.Entities.BookUser();
                    var getZ_User = new OrderBookSystemClient.Data.Entities.User();
                    var getZ_Book = new OrderBookSystemClient.Data.Entities.Book();

                    foreach (var item2 in getPendingOrdersDetailsSecondList)
                    {
                        getZ_BookUser = db.BooksUsers.Where(a => a.booksusers_id == item2.booksusers_id).FirstOrDefault();
                        getZ_User = db.Users.Where(a => a.user_id == item2.user_id).FirstOrDefault();
                        item2.Z_user_id = getZ_User;
                        item2.Z_bookuser_id = getZ_BookUser;
                        item2.Z_bookuser_id.Z_book_id = db.Books.Where(a => a.book_id == item2.Z_bookuser_id.book_id).FirstOrDefault();
                        getPendingOrdersDetailsList.Add(item2);
                    }
                    GetMoreDetails(getPendingOrdersDetailsList);
                }

                var getBooksPerUserList = new List<OrderBookSystemClient.Data.Entities.BookUser>();
                var getBooksPerUserItem = new OrderBookSystemClient.Data.Entities.BookUser();

                var getZBook = new OrderBookSystemClient.Data.Entities.Book();

                foreach (var item in getPendingOrdersDetailsList)
                {
                    getBooksPerUserItem = db.BooksUsers.Where(a => a.booksusers_id == item.booksusers_id).FirstOrDefault();
                    getZBook = db.Books.Where(a => a.book_id == getBooksPerUserItem.book_id).SingleOrDefault();
                    getBooksPerUserItem.Z_book_id = getZBook;

                    getBooksPerUserList.Add(getBooksPerUserItem);

                }

                var getBooksList = new List<OrderBookSystemClient.Data.Entities.Book>();
                var getBooksItem = new OrderBookSystemClient.Data.Entities.Book();

                foreach (var item in getBooksPerUserList)
                {
                    getBooksItem = db.Books.Where(a => a.book_id == item.book_id).FirstOrDefault();
                    getBooksList.Add(getBooksItem);
                }

                int order_message_state = Convert.ToInt32(TempData["order_message_state"]);
                if (order_message_state == 1)
                {
                    ViewData["order_message"] = 1;
                }
                if (order_message_state == 2)
                {
                    ViewData["order_message"] = 2;
                }
                if (order_message_state == 3)
                {
                    ViewData["order_message"] = 3;
                }
                if (order_message_state == 4)
                {
                    ViewData["order_message"] = 4;
                }
                if (order_message_state == 5)
                {
                    ViewData["order_message"] = 5;
                }
                return View(getPendingOrders);
            }
            else
            {
                return RedirectToAction("Login", "AdminAccount");
            }
        }


        public IActionResult OrderApproval(int orderid)
        {
            OrderBookSystemClient.Data.Entities.Order findSpecificOrder = db.Orders.Find(orderid);
            List<OrderBookSystemClient.Data.Entities.Book> bookList = new List<OrderBookSystemClient.Data.Entities.Book>();

            var username = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == username).Select(a => a.user_id).FirstOrDefault();

            var getBooksHaveOrdered = db.OrdersDetails.Where(a => a.order_id == orderid).ToList();
            var getuserhaveordered = db.OrdersDetails.Where(a => a.order_id == orderid).Select(a => a.user_id).FirstOrDefault();
            TempData["user_id_have_ordered"] = getuserhaveordered;
            bool has_quantity = true;
            if (username != null && getUserId != 0)
            {
                foreach (var item in getBooksHaveOrdered)
                {
                    item.Z_bookuser_id = db.BooksUsers.Where(a => a.booksusers_id == item.booksusers_id).FirstOrDefault();
                    var getBook = item.Z_bookuser_id.Z_book_id = db.Books.Where(a => a.book_id == item.Z_bookuser_id.book_id).FirstOrDefault();

                    if (getBook.book_quantity == 0)
                    {
                        has_quantity = false;
                    }

                    if (item.Z_bookuser_id.quantity > 1)
                    {
                        var getQuantityList = item.Z_bookuser_id.quantity;
                        for (int i = 0; i < getQuantityList; i++)
                        {
                            bookList.Add(getBook);
                        }
                    }
                    else
                    {
                        bookList.Add(getBook);
                    }
                }

                if (has_quantity == true)
                {

                    try
                    {

                        int message_state = 0;
                        foreach (var item in bookList)
                        {
                            var findBook = db.Books.Find(item.book_id);

                            db.Books.Update(findBook);
                            if (findBook.book_quantity <= 0)
                            {
                                message_state = 5; //out of stock int = 5
                                TempData["order_message_state"] = message_state;
                                return RedirectToAction("ManageOrders");
                            }
                            else
                            {
                                findBook.book_quantity -= 1;
                            }
                        }
                        findSpecificOrder.order_status = "APPROVE";
                        message_state = 1; //success approval int = 1
                        TempData["order_message_state"] = message_state;
                        db.SaveChanges();

                    }
                    catch (Exception)
                    {
                        int message_state = 2; //warning approval int = 2
                        TempData["order_message_state"] = message_state;
                        throw;
                    }

                    SendEmail("SUCCESS");
                }
                else
                {
                    int message_state = 5; //out of stock int = 5
                    TempData["order_message_state"] = message_state;
                }

                return RedirectToAction("ManageOrders");
            }
            else
            {
                return RedirectToAction("Login", "AdminAccount");

            }
        }

        public IActionResult OrderDecline(int orderid)
        {
            OrderBookSystemClient.Data.Entities.Order findSpecificOrder = db.Orders.Find(orderid);
            var getuserhaveordered = db.OrdersDetails.Where(a => a.order_id == orderid).Select(a => a.user_id).FirstOrDefault();
            TempData["user_id_have_ordered"] = getuserhaveordered;
            var username = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == username).Select(a => a.user_id).FirstOrDefault();

            if (username != null && getUserId != 0)
            {
                try
                {
                    findSpecificOrder.order_status = "DECLINE";
                    int message_state = 3; //success decline int = 3
                    TempData["order_message_state"] = message_state;
                   
                }
                catch (Exception)
                {
                    int message_state = 4; //warning decline int = 4
                    TempData["order_message_state"] = message_state;
                    throw;
                }
                SendEmail("FAILURE");
                db.SaveChanges();
                return RedirectToAction("ManageOrders");
            }
            else
            {
                return RedirectToAction("Login", "AdminAccount");

            }
        }

        public void SendEmail(string type_title)
        {
            OrderBookSystemClient.Data.Entities.EmailManagement emailManagement = new OrderBookSystemClient.Data.Entities.EmailManagement();
            var emailtypeId = db.EmailTypes.Where(a => a.type_title == type_title).Select(a => a.type_id).FirstOrDefault();
            var emailid = db.Emails.Where(a=> a.type_id == emailtypeId).Select(a => a.email_id).FirstOrDefault();
            var userid = Convert.ToInt32(TempData["user_id_have_ordered"]);
            var getUser = db.Users.Where(a => a.user_id == userid).FirstOrDefault();
            
            if (emailid == 0)
            {
                OrderBookSystemClient.Data.Entities.Email email = new OrderBookSystemClient.Data.Entities.Email();
                if(type_title == "SUCCESS")
                {
                    try
                    {
                        email.email_content = "Your Order successfully approved!";
                        email.type_id = emailtypeId;
                        db.Emails.Add(email);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    db.SaveChanges();
                }
                else
                {
                    try
                    {
                        email.email_content = "Sorry. Your Order have declined! Please contact with us for more informations or answer on this email.";
                        email.type_id = emailtypeId;
                        db.Emails.Add(email);
                    }
                    catch(Exception ex)
                    {
                        throw ex;
                    }
                    db.SaveChanges();
                }
                emailid = db.Emails.Where(a => a.type_id == emailtypeId).Select(a => a.email_id).FirstOrDefault();
            }

            var getEmailContent = db.Emails.Where(a => a.email_id == emailid).Select(a => a.email_content).FirstOrDefault();
            try
            {
                //TO USE THIS YOU MUST CHANGE SOME EMAIL CHANGES. 1)GO TO EMAIL --> MANAGE ACCOUNT --> SECURITY --> PROSVASI SE LEIGOTERO SECURE APPS --> ENABLE 
                // 2) GOTO SETTINGS --> ALL SETTINGS --> POP/IMAP --> ENABLE IMAP
           
                MailMessage mail = new MailMessage();
                mail.To.Add(getUser.email);
                mail.From = new MailAddress("mariosviolarisbooksystem@gmail.com");
                mail.Subject = "BOOKS ORDER";
                mail.Body = $"<p>Dear {getUser.username}, </p> <p> {getEmailContent} </p>" +
                        $" <p> You can see more info about your order in your account in our book system.</p><p>Thank you for choosing us.</p><p>Kind Regards</p> ";
                mail.IsBodyHtml = true;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587);
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("mariosviolarisbooksystem@gmail.com", "mariosviolaris1!");
                smtp.Send(mail);
           
            
                try
                {
                    emailManagement.email_id = emailid;
                    emailManagement.user_id = userid;
                    db.EmailManagements.Add(emailManagement);
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    throw;
                }
           
                
            }
            catch (Exception)
            {
                throw;
            }
        }
       
        

        public void GetMoreDetails(List<OrderBookSystemClient.Data.Entities.OrderDetail> getPendingOrdersDetailsList)
        {

            var MoreDetailsList = new List<OrderModel>();
            MoreDetailsList.Clear();
            foreach (var item in getPendingOrdersDetailsList)
            {
                
                OrderModel om = new OrderModel();
                om.order_id = item.order_id;
                om.username = item.Z_user_id.username;
                om.for_buy = item.Z_bookuser_id.for_buy;
                om.for_loan = item.Z_bookuser_id.for_loan;
                om.book_title = item.Z_bookuser_id.Z_book_id.book_title;
                om.quantity = item.Z_bookuser_id.quantity;
                if (item.Z_bookuser_id.for_buy == true)
                {
                    om.price = item.Z_bookuser_id.Z_book_id.book_buy_price;
                }
                else
                {
                    om.price = item.Z_bookuser_id.Z_book_id.book_loan_price;
                }
                MoreDetailsList.Add(om);
               
            }
            ViewData["moredetailslist"] = MoreDetailsList;
          
        }


        #endregion


        #region manageBookRegion
        public IActionResult ManageBooks()
        {
            var username = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == username).Select(a => a.user_id).FirstOrDefault();
            if (username != null && getUserId != 0)
            {
                var getBooks = db.Books.Where(a => a.book_id >= 0).Where(b => b.book_has_deleted == false).ToList();
                foreach (var item in getBooks)
                {
                    var getBookType = db.BookTypes.Where(a => a.BookTypes_id == item.bookTypes_id).FirstOrDefault();
                    item.Z_BookType = getBookType;
                }

                return View(getBooks);
            }
            else
            {
                return RedirectToAction("Login", "AdminAccount");
            }
        }

   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddBooks(string AddBookData)
        {

            KeyValuePair<string, string> GetResult;
            var sa = new JsonSerializerSettings();
            OrderBookSystemClient.Data.Entities.Book BookItem = new OrderBookSystemClient.Data.Entities.Book();

            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(AddBookData);

            string bookTitle = Array_With_JSON_Res.bookname;
            string bookAuthor = Array_With_JSON_Res.bookauthor;
            double buyprice = Array_With_JSON_Res.buyprice;
            double loanprice = Array_With_JSON_Res.loanprice;
            int bookquantity = Array_With_JSON_Res.bookquantity;
            bool isAvailableForLoan = Array_With_JSON_Res.availableforloancheck;
            bool isAvailableForBuy = Array_With_JSON_Res.availableforbuycheck;
            string booktype = Array_With_JSON_Res.booktype;

            int getBookTypeId = db.BookTypes.Where(a => a.bookTypes_name == booktype).Select(a => a.BookTypes_id).FirstOrDefault();

            if (bookTitle == "" || bookAuthor == "" || booktype == "")
            {
                GetResult = new KeyValuePair<string, string>("F", "Book not Successfully Added");
                return Json(GetResult, sa);
            }
            else
            {
                try
                {
                    BookItem.book_title = bookTitle;
                    BookItem.book_author = bookAuthor;
                    BookItem.book_buy_price = buyprice;
                    BookItem.book_loan_price = loanprice;
                    BookItem.book_quantity = bookquantity;
                    BookItem.book_buy_availability = isAvailableForBuy;
                    BookItem.book_loan_availability = isAvailableForLoan;
                    BookItem.bookTypes_id = getBookTypeId;
                    BookItem.book_has_deleted = false;
                    BookItem.insert_date = DateTime.Now;

                    db.Books.Add(BookItem);
                    db.SaveChanges();

                }
                catch (Exception ex)
                {
                    GetResult = new KeyValuePair<string, string>("EX", "Exception Error Please contact Admin :" + ex.Message);
                    return Json(GetResult, sa);
                }
            }
            GetResult = new KeyValuePair<string, string>("S", "Book Added Successfully!");

            return Json(GetResult, sa);
        }

        [HttpPost]
        public JsonResult DeleteBooks(string DeleteBooksData)
        {
            KeyValuePair<string, string> GetResult;
            var sa = new JsonSerializerSettings();
            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(DeleteBooksData);

            int bookid = Array_With_JSON_Res.bookid;

            OrderBookSystemClient.Data.Entities.Book bookItem = db.Books.Find(bookid);

        
            try
            {
                bookItem.book_has_deleted = true;

                db.Books.Update(bookItem);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
               GetResult = new KeyValuePair<string, string>("EX", "Exception Error Occures during Delete, Please contact Admin :" + ex.Message);
               return Json(GetResult, sa);
            }
            GetResult = new KeyValuePair<string, string>("S", "Book Deleted Successfully!");

            return Json(GetResult, sa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditBooks(string EditBookData)
        {
            KeyValuePair<string, string> GetResult;
            var sa = new JsonSerializerSettings();

            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(EditBookData);

            int bookid = Array_With_JSON_Res.bookid;
            string bookTitle = Array_With_JSON_Res.bookname;
            string bookAuthor = Array_With_JSON_Res.bookauthor;
            double buyprice = Array_With_JSON_Res.buyprice;
            double loanprice = Array_With_JSON_Res.loanprice;
            int bookquantity = Array_With_JSON_Res.bookquantity;
            bool isAvailableForLoan = Array_With_JSON_Res.availableforloancheck;
            bool isAvailableForBuy = Array_With_JSON_Res.availableforbuycheck;
            string booktype = Array_With_JSON_Res.booktype;

            int getBookTypeId = db.BookTypes.Where(a => a.bookTypes_name == booktype).Select(a => a.BookTypes_id).FirstOrDefault();

            var getExactBookItem = db.Books.Find(bookid);

            if (bookTitle == "" || bookAuthor == "" || booktype == "")
            {
                GetResult = new KeyValuePair<string, string>("F", "Book not Successfully Edited");
                return Json(GetResult, sa);
            }
            else
            {
                try
                {
                    getExactBookItem.book_title = bookTitle;
                    getExactBookItem.book_author = bookAuthor;
                    getExactBookItem.book_buy_price = buyprice;
                    getExactBookItem.book_loan_price = loanprice;
                    getExactBookItem.book_quantity = bookquantity;
                    getExactBookItem.book_buy_availability = isAvailableForBuy;
                    getExactBookItem.book_loan_availability = isAvailableForLoan;
                    getExactBookItem.bookTypes_id = getBookTypeId;
                    getExactBookItem.book_has_deleted = false;

                    db.Books.Update(getExactBookItem);
                    db.SaveChanges();

                }
                catch (Exception ex)
                {
                    GetResult = new KeyValuePair<string, string>("EX", "Exception Error Please contact Admin :" + ex.Message);
                    return Json(GetResult, sa);
                }
            }
            GetResult = new KeyValuePair<string, string>("S", "Book Edit Successfully!");

            return Json(GetResult, sa);
        }



        #endregion



        #region manageBookTypes
        public IActionResult ManageTypes()
        {
            var username = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == username).Select(a => a.user_id).FirstOrDefault();

            if (username != null && getUserId != 0)
            {


                var getTypes = db.BookTypes.Where(a => a.BookTypes_id >= 0).ToList();

                return View(getTypes);
            }
            else
            {
                return RedirectToAction("Login", "AdminAccount");

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddTypes(string AddTypeData)
        {
            KeyValuePair<string, string> GetResult;
            var sa = new JsonSerializerSettings();
            OrderBookSystemClient.Data.Entities.BookType bookTypeItem = new OrderBookSystemClient.Data.Entities.BookType();

            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(AddTypeData);
            string typename = Array_With_JSON_Res.typename;

            if (typename == "")
            {
                GetResult = new KeyValuePair<string, string>("F", "Type Name is Required!");
                return Json(GetResult, sa);
            }
            else
            {
                try
                {
                    bookTypeItem.bookTypes_name = typename;
                    db.BookTypes.Add(bookTypeItem);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    GetResult = new KeyValuePair<string, string>("EX", "Exception Error Please contact Admin :" + ex.Message);
                    return Json(GetResult, sa);
                }
            }
            GetResult = new KeyValuePair<string, string>("S", "Type Added Successfully!");
            return Json(GetResult, sa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditTypes(string EditTypeData)
        {
            KeyValuePair<string, string> GetResult;
            var sa = new JsonSerializerSettings();

            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(EditTypeData);

            int typeId = Array_With_JSON_Res.typeid;
            string typeName = Array_With_JSON_Res.typename;
          
            var getExactType = db.BookTypes.Find(typeId);

            if (typeName == "")
            {
                GetResult = new KeyValuePair<string, string>("F", "Type not Successfully Edited");
                return Json(GetResult, sa);
            }
            else
            {
                try
                {
                    getExactType.BookTypes_id = typeId;
                    getExactType.bookTypes_name = typeName;
                    db.BookTypes.Update(getExactType);
                    db.SaveChanges();

                }
                catch (Exception ex)
                {
                    GetResult = new KeyValuePair<string, string>("EX", "Exception Error Please contact Admin :" + ex.Message);
                    return Json(GetResult, sa);
                }
            }
            GetResult = new KeyValuePair<string, string>("S", "Type Edited Successfully!");

            return Json(GetResult, sa);
        }

        [HttpPost]
        public JsonResult DeleteTypes(string DeleteTypesData)
        {
            KeyValuePair<string, string> GetResult;
            var sa = new JsonSerializerSettings();
            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(DeleteTypesData);

            int typeid = Array_With_JSON_Res.typeid;

            OrderBookSystemClient.Data.Entities.BookType typeItem = db.BookTypes.Find(typeid);

            var getBooks = db.Books.Where(a => a.book_id >= 0).ToList();

            foreach (var item in getBooks)
            {
                if(item.bookTypes_id == typeItem.BookTypes_id && item.book_has_deleted == false)
                {
                    GetResult = new KeyValuePair<string, string>("F", "Sorry, this type cannot be deleted." + item.book_title + " has this type on it.");
                    return Json(GetResult, sa);
                }
                if(item.bookTypes_id == typeItem.BookTypes_id && item.book_has_deleted == true)
                {
                    try
                    {
                        var findBook = db.Books.Find(item.book_id);
                        db.Books.Remove(findBook);
                        db.BookTypes.Remove(typeItem);
                        db.SaveChanges();
                    }
                    catch(Exception ex)
                    {
                        GetResult = new KeyValuePair<string, string>("EX", "Exception Error Occures during Delete, Please contact Admin :" + ex.Message);
                        return Json(GetResult, sa);
                    }
                    GetResult = new KeyValuePair<string, string>("S", "Type Deleted Successfully!");
                    return Json(GetResult, sa);
                }
            }

            try
            {
                db.BookTypes.Remove(typeItem);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                GetResult = new KeyValuePair<string, string>("EX", "Exception Error Occures during Delete, Please contact Admin :" + ex.Message);
                return Json(GetResult, sa);
            }
            GetResult = new KeyValuePair<string, string>("S", "Type Deleted Successfully!");

            return Json(GetResult, sa);
        }


        #endregion



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrderBookSystemClient.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemClient.Controllers
{
    
    public class HomeController : Controller
    {
        OrderBookSystemClient.Data.DBContext db = new Data.DBContext();
       
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var getUsername = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == getUsername).Select(a => a.user_id).FirstOrDefault();

            if (getUsername != null && getUserId != 0)
            {

                var getOrderDetails = db.OrdersDetails.Where(a => a.user_id == getUserId).ToList();
                var getOrdersItem = new Data.Entities.Order();
                var getOrders = new List<Data.Entities.Order>();

                foreach (var item in getOrderDetails)
                {
                    item.Z_order_id = db.Orders.Where(a => a.order_id == item.order_id).FirstOrDefault();
                    item.Z_bookuser_id = db.BooksUsers.Where(a => a.booksusers_id == item.booksusers_id).FirstOrDefault();
                    item.Z_bookuser_id.Z_book_id = db.Books.Where(a => a.book_id == item.Z_bookuser_id.book_id).FirstOrDefault();
                }
                GetMoreOrderDetails(getOrderDetails);

                var getOrdersDist = getOrderDetails.Select(a => a.order_id).Distinct().ToList();
                foreach (var item2 in getOrdersDist)
                {
                    getOrdersItem = db.Orders.Where(a => a.order_id == item2).FirstOrDefault();
                    getOrders.Add(getOrdersItem);
                }
                return View(getOrders);
            }
            else
            {
                return RedirectToAction("ClientLogin", "Account");
            }
        }

        #region cart
        public IActionResult AddToCart(int book_id, bool for_loan, bool for_buy)
        {
            OrderBookSystemClient.Data.Entities.BookUser booksPersUser = new Data.Entities.BookUser();

            var getUsername = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == getUsername).Select(a => a.user_id).FirstOrDefault();
            var check_if_a_book_already_isin_cart = new List<OrderBookSystemClient.Data.Entities.BookUser>();
            if (for_buy == true)
            {
                 check_if_a_book_already_isin_cart = db.BooksUsers.Where(a => a.user_id == getUserId).Where(b => b.book_id == book_id).Where(c => c.order_is_send == false).Where(d => d.for_buy == true).ToList();
            }
            if(for_loan == true)
            {
                check_if_a_book_already_isin_cart = db.BooksUsers.Where(a => a.user_id == getUserId).Where(b => b.book_id == book_id).Where(c => c.order_is_send == false).Where(d => d.for_loan == true).ToList();

            }

            if (getUsername != null && getUserId != 0)
            {
                var getBookType = db.Books.Where(a => a.book_id == book_id).Select(a => a.Z_BookType.BookTypes_id).FirstOrDefault();

                if (check_if_a_book_already_isin_cart.Count < 1)
                {
                    DateTime defaultDateTime = new DateTime(2000, 01, 01);

                    try
                    {
                        booksPersUser.book_id = book_id;
                        booksPersUser.user_id = getUserId;
                        booksPersUser.for_buy = for_buy;
                        booksPersUser.for_loan = for_loan;
                        booksPersUser.quantity = 1;

                        if (for_loan == true)
                        {
                            booksPersUser.loan_returning_date = DateTime.Now.AddMonths(1);
                        }
                        else
                        {
                            booksPersUser.loan_returning_date = defaultDateTime;
                        }
                        db.BooksUsers.Add(booksPersUser);
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    return RedirectToAction("ShowBooksDependOnCategory", new { booktypes_id = getBookType });
                }
                else
                {
                    return RedirectToAction("ShowBooksDependOnCategory", new { booktypes_id = getBookType });
                }
            }
            else
            {
                return RedirectToAction("ClientLogin", "Account");
            }
        }

        public IActionResult RemoveFromCart(int book_id)
        {
            OrderBookSystemClient.Data.Entities.BookUser booksPersUser = new Data.Entities.BookUser();

            var getUsername = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == getUsername).Select(a => a.user_id).FirstOrDefault();

            if (getUsername != null && getUserId != 0)
            {
                var getRemovedBook = db.BooksUsers.Where(a => a.user_id == getUserId).Where(b => b.book_id == book_id).Where(c => c.order_is_send == false).ToList();

                foreach (var item in getRemovedBook)
                {
                    try
                    {
                        db.BooksUsers.Remove(item);
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                return RedirectToAction("SeeCart");
            }
            else
            {
                return RedirectToAction("ClientLogin", "Account");
            }
        }

        public IActionResult SeeCart()
        {

            var getUsername = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == getUsername).Select(a => a.user_id).FirstOrDefault();


            if (getUsername != null && getUserId != 0)
            {

                double price = 0;
                var orderBooksList = db.BooksUsers.Where(a => a.user_id == getUserId).Where(a => a.order_is_send == false).ToList();
                var ModelList = new List<CartModel>();
                if (orderBooksList.Count != 0)
                {
                    foreach (var item in orderBooksList)
                    {
                        CartModel cartModel = new CartModel();

                        item.Z_book_id = db.Books.Where(a => a.book_id == item.book_id).FirstOrDefault();
                        cartModel.user_id = item.user_id;
                        item.Z_user_id = db.Users.Where(a => a.user_id == item.user_id).FirstOrDefault();
                        cartModel.book_id = item.book_id;
                        cartModel.for_buy = item.for_buy;
                        cartModel.for_loan = item.for_loan;
                        cartModel.book_name = item.Z_book_id.book_title;
                        cartModel.book_author = item.Z_book_id.book_author;
                        item.Z_book_id.Z_BookType = db.BookTypes.Where(a => a.BookTypes_id == item.Z_book_id.bookTypes_id).FirstOrDefault();
                        cartModel.book_type = item.Z_book_id.Z_BookType.bookTypes_name;
                        if (item.for_buy == true)
                        {
                            price = cartModel.price = item.Z_book_id.book_buy_price;
                        }
                        else
                        {
                            price = cartModel.price = item.Z_book_id.book_loan_price;
                        }

                        cartModel.total_price += price;

                        ModelList.Add(cartModel);
                    }

                    return View(ModelList);
                }
                else
                {
                    int message_state = Convert.ToInt32(TempData["order_completed"]);
                    if (message_state == 1)
                    {
                        ViewData["message_order_completed"] = 1;
                    }
                    if (message_state == 2)
                    {
                        ViewData["message_order_completed"] = 2;
                    }
                    if (message_state == 3)
                    {
                        ViewData["message_order_completed"] = 3;
                    }
                    return View(ModelList);
                }
            }
            else
            {
                return RedirectToAction("ClientLogin", "Account");

            }
        }

        public void CompleteOrder(string CompleteOrder_Data)
        {

            
            var sa = new JsonSerializerSettings();

            var getUsername = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == getUsername).Select(a => a.user_id).FirstOrDefault();

            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(CompleteOrder_Data);
            double totalorder = Array_With_JSON_Res.totalorder;

            JArray quantity_and_bookid = Array_With_JSON_Res.quantity_bookid;

            List<Data.Entities.BookUser> bookUsers = db.BooksUsers.Where(a => a.user_id == getUserId).Where(a => a.order_is_send == false).ToList();


            foreach (JObject element in quantity_and_bookid)
            {
                var quantity = element["quantity"].ToString();
                var bookid = element["bookid"].ToString();
                var bookid_int = Convert.ToInt32(bookid);
                var quantity_int = Convert.ToInt32(quantity); 
                if (quantity_int > 1)
                {
                    foreach (var item in bookUsers)
                    {
                        if(item.book_id == bookid_int)
                        {
                            item.quantity = quantity_int;
                        }
                    }
                }
            }
            if (bookUsers.Count != 0) 
            {
                Data.Entities.Order orderItem = new Data.Entities.Order();
                try
                {
                    orderItem.order_date = DateTime.Now;
                    orderItem.order_status = "PENDING";
                    orderItem.order_total_price = totalorder;
                    orderItem.order_returning_date = DateTime.Now.AddMonths(1);
                    db.Orders.Add(orderItem);
                    db.SaveChanges();
                    int message_state = 1; //order completed
                    TempData["order_completed"] = message_state;
                }
                catch (Exception)
                {
                    int message_state = 2; //oops something going wrong
                    TempData["order_completed"] = message_state;
                    throw;
                }

                Data.Entities.OrderDetail orderDetailsItem = new Data.Entities.OrderDetail();

                int orderLastId = db.Orders.Max(item => item.order_id);
                int userid = getUserId;

                foreach (var item in bookUsers)
                {
                    int booksusersid = item.booksusers_id;

                    try
                    {
                        orderDetailsItem.user_id = userid;
                        orderDetailsItem.order_id = orderLastId;
                        orderDetailsItem.booksusers_id = booksusersid;
                        db.OrdersDetails.Add(orderDetailsItem);
                        db.SaveChanges();
                        int message_state = 1; //order completed
                        TempData["order_completed"] = message_state;
                    }
                    catch (Exception)
                    {
                        int message_state = 2; //oops something going wrong
                        TempData["order_completed"] = message_state;
                        throw;
                    }
                }

                foreach (var item in bookUsers)
                {
                    Data.Entities.BookUser bookUserItem = db.BooksUsers.Find(item.booksusers_id);

                    try
                    {
                        bookUserItem.order_is_send = true;
                        db.SaveChanges();
                        int message_state = 1; //order completed
                        TempData["order_completed"] = message_state;
                    }
                    catch (Exception)
                    {
                        int message_state = 2; //oops something going wrong
                        TempData["order_completed"] = message_state;
                        throw;
                    }
                }
            }
            else
            {
                int message_state = 3; //cannot send empty order
                TempData["order_completed"] = message_state;
            }
            
        }

        #endregion

        public IActionResult OrdersLog()
        {
            var getUsername = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == getUsername).Select(a => a.user_id).FirstOrDefault();
            if (getUsername != null && getUserId != 0)
            {

                var getOrderDetails = db.OrdersDetails.Where(a => a.user_id == getUserId).ToList();
                var getOrdersItem = new Data.Entities.Order();
                var getOrders = new List<Data.Entities.Order>();

                foreach (var item in getOrderDetails)
                {
                    item.Z_order_id = db.Orders.Where(a => a.order_id == item.order_id).FirstOrDefault();
                    item.Z_bookuser_id = db.BooksUsers.Where(a => a.booksusers_id == item.booksusers_id).FirstOrDefault();
                    item.Z_bookuser_id.Z_book_id = db.Books.Where(a => a.book_id == item.Z_bookuser_id.book_id).FirstOrDefault();  
                }
                GetMoreOrderDetails(getOrderDetails);

                var getOrdersDist = getOrderDetails.Select(a => a.order_id).Distinct().ToList();
                foreach (var item2 in getOrdersDist)
                {
                    getOrdersItem = db.Orders.Where(a => a.order_id == item2).FirstOrDefault();
                    getOrders.Add(getOrdersItem);
                }
                return View(getOrders);
            }
            else
            {
                return RedirectToAction("ClientLogin", "Account");
            }
        }

        public void GetMoreOrderDetails(List<OrderBookSystemClient.Data.Entities.OrderDetail> getClientPendingOrdersDetailsList)
        {

            var MoreDetailsList = new List<ClientOrderModel>();
            MoreDetailsList.Clear();
            foreach (var item in getClientPendingOrdersDetailsList)
            {

                ClientOrderModel om = new ClientOrderModel();
                om.order_id = item.order_id;
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
            ViewData["moredetailsclientlist"] = MoreDetailsList;

        }

        public IActionResult ShowBooksDependOnCategory(int booktypes_id)
        {
            var getUsername = HttpContext.Session.GetString("username");
            var getUserId = db.Users.Where(a => a.username == getUsername).Select(a => a.user_id).FirstOrDefault();

            if (getUsername != null && getUserId != 0)
            {

                var getBooksPerType = db.Books.Where(a => a.bookTypes_id == booktypes_id).Where(a => a.book_has_deleted == false).ToList();
                return View(getBooksPerType);
            }
            else
            {
                return RedirectToAction("ClientLogin", "Account");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

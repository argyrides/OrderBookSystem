using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderBookSystemClient.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace OrderBookSystemAdministator.Controllers
{
    public class AdminAccountController : Controller
    {
        DBContext db = new DBContext();
        OrderBookSystemClient.Classes.HelperClass helperClass = new OrderBookSystemClient.Classes.HelperClass();

        public IActionResult FirstLogin()
        {
            int register_message_state = Convert.ToInt32(TempData["register_message_state"]);

            if (register_message_state == 1)
            {
                ViewData["register_message"] = 1;
            }
            return View();
        }

        [HttpPost]
        public JsonResult CheckRegisterCredentials(string RegisterData)
        {
            KeyValuePair<string, string> GetResult = new KeyValuePair<string, string>();
            var sa = new JsonSerializerSettings();
            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(RegisterData);

            string firstName = Array_With_JSON_Res.firstname;
            string lastName = Array_With_JSON_Res.lastname;
            string userName = Array_With_JSON_Res.username;
            string password = Array_With_JSON_Res.password;
            string email = Array_With_JSON_Res.email;

            DateTime defaultDateTime = new DateTime(2000, 01, 01);
            var checkIfUsernameAlreadyExist = db.Users.Where(a => a.username == userName).ToList();

            if (checkIfUsernameAlreadyExist.Count > 0)
            {
                GetResult = new KeyValuePair<string, string>("F", "This username is already used! Please choose another!");
                return Json(GetResult, sa);
            }
            else
            {
                if (helperClass.CheckPasswordIfIsValid(password) == true)
                {
                    OrderBookSystemClient.Data.Entities.User user = new OrderBookSystemClient.Data.Entities.User();
                    try
                    {
                        user.last_login = defaultDateTime;
                        user.role = "ADMIN";
                        user.password = helperClass.Encrypt(password);
                        user.email = email;
                        user.username = userName;
                        user.firstname = firstName;
                        user.lastname = lastName;
                        user.is_approve = true;
                        db.Users.Add(user);
                        db.SaveChanges();

                    }
                    catch (Exception ex)
                    {
                        GetResult = new KeyValuePair<string, string>("EX", "Please contact Administrator" + ex);
                        return Json(GetResult, sa);

                    }
                }
                else
                {
                    GetResult = new KeyValuePair<string, string>("F", "Password must contain 8 characters, 1 uppercase letter, 1 alpharethmitical and 1 special character at least!");
                    return Json(GetResult, sa);
                }


            }

            GetResult = new KeyValuePair<string, string>("S", "Register Completed Successfully!");
            return Json(GetResult, sa);
        }

        public IActionResult Login()
        {
            var getAdmins = db.Users.Where(a => a.role == "ADMIN").ToList();
            if (getAdmins.Count == 0)
            {
                return RedirectToAction("FirstLogin");
            }
            else
            {

                var username = HttpContext.Session.GetString("username");
                var userid = db.Users.Where(a => a.username == username).Select(a => a.user_id).FirstOrDefault();

                int message_state = Convert.ToInt32(TempData["message_state"]);


                if (userid == 0 && HttpContext.Session.GetString("username") == null && message_state <= 0)
                {
                    return View();
                }
                if (userid == 0 && HttpContext.Session.GetString("username") != null)
                {
                    return RedirectToAction("Index", "HomeAdmin");
                }
                if (message_state == 0)
                {
                    return View();
                }
                if (message_state == 1)
                {
                    if (HttpContext.Session.GetString("username") != null)
                    {
                        ViewData["message"] = 1;
                        return RedirectToAction("Index", "HomeAdmin");
                    }
                }
                if (message_state == 2)
                {
                    ViewData["message"] = 2;
                }
                if (message_state == 3)
                {
                    ViewData["message"] = 3;
                }
                if (message_state == 4)
                {
                    ViewData["message"] = 4;
                }
                if (message_state == 5)
                {
                    ViewData["message"] = 5;
                }
                if (message_state == 6)
                {
                    ViewData["message"] = 6;
                }
              

                return View();
            }
        }

        [Route("logout")]
        public IActionResult Logout()
        {
            var username = HttpContext.Session.GetString("username");
            var userid = db.Users.Where(a => a.username == username).Select(a => a.user_id).FirstOrDefault();

            if (username == null)
            {
                return RedirectToAction("Login");
            }
            else
            {

                var getUser = db.Users.Find(userid);
                try
                {
                    getUser.is_login = false;
                    db.Users.Update(getUser);
                    HttpContext.Session.Remove("username");

                    db.SaveChanges();
                }
                catch (Exception)
                {
                    throw;
                }
                int message_state = 0; //logout
                TempData["message_state"] = message_state;

                return View();
            }
            
        }

        public IActionResult CheckUserCredentials(OrderBookSystemClient.Data.Entities.User user)
        {
            var getAllUsers = db.Users.Where(a => a.user_id >= 0).ToList();

            foreach (var item in getAllUsers)
            {
                if (user.username == item.username)
                {
                    if (item.role == "ADMIN")
                    {
                        if (user.password == helperClass.Decrypt(item.password))
                        {
                            if (item.login_tries <= 3)
                            {
                                if (item.is_locked == false)
                                {
                                    if (item.is_approve == true)
                                    {
                                        int message_state = 1; //login!!
                                        TempData["message_state"] = message_state;
                                        var findUserId = db.Users.Where(a => a.username == item.username).Select(a => a.user_id).FirstOrDefault();
                                        var getUser = db.Users.Find(findUserId);
                                        try
                                        {
                                            getUser.is_login = true;
                                            getUser.last_login = DateTime.Now;
                                            getUser.is_approve = true;
                                            getUser.login_tries = 0;
                                            HttpContext.Session.SetString("username", item.username);
                                            db.Users.Update(getUser);
                                            db.SaveChanges();
                                        }
                                        catch (Exception)
                                        {
                                            throw;
                                        }
                                        return RedirectToAction("Login");
                                    }
                                    else
                                    {
                                        int message_state = 4; //your account is not approved yet
                                        TempData["message_state"] = message_state;
                                        return RedirectToAction("Login");
                                    }
                                }
                                else
                                {
                                    int message_state = 3; //your account is locked
                                    TempData["message_state"] = message_state;
                                    return RedirectToAction("Login");
                                }
                            }
                            else
                            {
                                var findUserId = db.Users.Where(a => a.username == item.username).Select(a => a.user_id).FirstOrDefault();
                                var getUser = db.Users.Find(findUserId);
                                try
                                {
                                    int message_state = 3; //your account is locked
                                    TempData["message_state"] = message_state;
                                    getUser.is_locked = true;
                                    db.Users.Update(getUser);
                                    db.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                                return RedirectToAction("Login");
                            }
                        }
                        else //wrong password
                        {
                            var findUserId = db.Users.Where(a => a.username == item.username).Select(a => a.user_id).FirstOrDefault();
                            var getUser = db.Users.Find(findUserId);
                            try
                            {
                                int message_state = 2; //wrong username or password
                                TempData["message_state"] = message_state;
                                getUser.login_tries += 1;
                                db.Users.Update(getUser);
                                db.SaveChanges();
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            return RedirectToAction("Login");

                        }
                    }
                    else
                    {
                        int message_state = 5; //no access to that pages
                        TempData["message_state"] = message_state;
                        return RedirectToAction("Login");
                    }
                }
                else //wrong username
                {
                    int message_state = 2; //wrong username or password
                    TempData["message_state"] = message_state;
                }
            }
            return RedirectToAction("Login");
        }

    }
}

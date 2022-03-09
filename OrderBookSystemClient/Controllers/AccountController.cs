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

using Newtonsoft.Json;

namespace OrderBookSystemClient.Controllers
{
    public class AccountController : Controller
    {
        DBContext db = new DBContext();

        public IActionResult ClientLogin()
        {
            var username = HttpContext.Session.GetString("username");
            var userid = db.Users.Where(a => a.username == username).Select(a => a.user_id).FirstOrDefault();
            //var getUserHasLogin = db.Users.Where(a => a.user_id == userid).Select(a => a.user_id).FirstOrDefault();

            //TempData["userHasLogin"] = getUserHasLogin;
           

            int message_state = Convert.ToInt32(TempData["message_state"]);

            if (userid == 0 && username == null && message_state == 0)
            {
                return View();
            }
            if (userid == 0 && username != null)
            {
                
                return RedirectToAction("Index", "Home");

            }
            if (message_state == 0)
            {
                return View();
            }
            if (message_state == 1)
            {
                if (username != null)
                {
                    ViewData["message"] = 1;
                    return RedirectToAction("Index", "Home");
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

            return View();
        }

        public IActionResult ClientRegister()
        {
            int register_message_state = Convert.ToInt32(TempData["register_message_state"]);
           
            if (register_message_state == 1)
            {
                ViewData["register_message"] = 1;
            }
            if (register_message_state == 2)
            {
                ViewData["register_message"] = 2;
            }
            if (register_message_state == 3)
            {
                ViewData["register_message"] = 3;
            }

            return View();
        }

        [Route("userlogout")]
        public IActionResult ClientLogout()
        {
            //int user_id = Convert.ToInt32(TempData["userHasLogin"]);
            var username = HttpContext.Session.GetString("username");
            var user_id = db.Users.Where(a => a.username == username).Select(a => a.user_id).FirstOrDefault();

            if (username == null)
            {
                return RedirectToAction("ClientLogin");
            }
            else
            {
                var getUser = db.Users.Find(user_id);
                try
                {
                    getUser.is_login = false;
                    db.Users.Update(getUser);
                    HttpContext.Session.Remove("username");
                    Console.WriteLine(username + "has logged out");

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

        public IActionResult CheckClientUserCredentials(OrderBookSystemClient.Data.Entities.User user)
        {
            var getAllUsers = db.Users.Where(a => a.user_id >= 0).ToList();
            Classes.HelperClass helperClass = new Classes.HelperClass();

            foreach (var item in getAllUsers)
            {
                if (user.username == item.username)
                {
                    if (item.role == "ADMIN" || item.role == "USER")
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
                                            getUser.login_tries = 0;
                                            HttpContext.Session.SetString("username", item.username);
                                            db.Users.Update(getUser);
                                            db.SaveChanges();
                                        }
                                        catch (Exception)
                                        {
                                            throw;
                                        }
                                        return RedirectToAction("ClientLogin");
                                    }
                                    else
                                    {
                                        int message_state = 4; //your account is not approved yet
                                        TempData["message_state"] = message_state;
                                        return RedirectToAction("ClientLogin");
                                    }
                                }
                                else
                                {
                                    int message_state = 3; //your account is locked
                                    TempData["message_state"] = message_state;
                                    return RedirectToAction("ClientLogin");
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
                                return RedirectToAction("ClientLogin");
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
                            return RedirectToAction("ClientLogin");

                        }
                    }
                    else
                    {
                        int message_state = 5; //no access to that pages
                        TempData["message_state"] = message_state;
                        return RedirectToAction("ClientLogin");
                    }
                }
                else //wrong username
                {
                    int message_state = 2; //wrong username or password
                    TempData["message_state"] = message_state;
                }
            }
            return RedirectToAction("ClientLogin");
        }
    
        [HttpPost]
        public JsonResult CheckRegisterCredentials(string RegisterData)
        {
            Classes.HelperClass helperClass = new Classes.HelperClass();
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
                if(helperClass.CheckPasswordIfIsValid(password) == true)
                {
                    Data.Entities.User user = new Data.Entities.User();
                    try
                    {
                        user.last_login = defaultDateTime;
                        user.role = "USER";
                        user.password = helperClass.Encrypt(password);
                        user.email = email;
                        user.username = userName;
                        user.firstname = firstName;
                        user.lastname = lastName;
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


    }

}

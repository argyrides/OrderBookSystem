using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderBookSystemClient.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemClient.Controllers
{
    public class SettingsController : Controller
    {
        OrderBookSystemClient.Data.DBContext db = new Data.DBContext();
        public IActionResult SettingsIndex()
        {
            var getUsername = HttpContext.Session.GetString("username");
            var getUser = db.Users.Where(a => a.username == getUsername).FirstOrDefault();
            if (getUsername != null && getUser.user_id != 0)
            {
                return View(getUser);
            }
            else
            {
                return RedirectToAction("ClientLogin", "Account");
            }
        }

        public JsonResult SaveChangesForSettings(string SettingsData)
        {
            Classes.HelperClass helperClass = new Classes.HelperClass();
            var getUsername = HttpContext.Session.GetString("username");
            var getUser = db.Users.Where(a => a.username == getUsername).Select(a=> a.user_id).FirstOrDefault();

            KeyValuePair<string, string> GetResult;
            var sa = new JsonSerializerSettings();
            var Array_With_JSON_Res = JsonConvert.DeserializeObject<dynamic>(SettingsData);

            string firstname = Array_With_JSON_Res.firstName;
            string lastname = Array_With_JSON_Res.lastName;
            string oldpass = Array_With_JSON_Res.oldPass;
            string newpass = Array_With_JSON_Res.newPass;
            string email = Array_With_JSON_Res.email;


            OrderBookSystemClient.Data.Entities.User userItem = db.Users.Find(getUser);

            try
            {
                string encryptPass = helperClass.Decrypt(userItem.password);

                if (email != "")
                {
                    userItem.email = email;
                }
                else
                {
                    GetResult = new KeyValuePair<string, string>("F", "Error. Email Must Not be empty!");
                    return Json(GetResult, sa);
                }
                if (firstname != "")
                {
                    userItem.firstname = firstname;
                }
                else
                {
                    GetResult = new KeyValuePair<string, string>("F", "Error. First Name Must Not be empty!");
                    return Json(GetResult, sa);
                }
                if (lastname != "")
                {
                    userItem.lastname = lastname;
                }
                else
                {
                    GetResult = new KeyValuePair<string, string>("F", "Error. Last Name Must Not be empty!");
                    return Json(GetResult, sa);
                }
                if (oldpass == "" && newpass == "")
                {
                    userItem.password = userItem.password;
                }
                else if(oldpass == "" && newpass != "")
                {
                    GetResult = new KeyValuePair<string, string>("F", "Error. Please fill your old password to change your password!");
                    return Json(GetResult, sa);
                }
                else if(oldpass != "" && newpass == "")
                {
                    GetResult = new KeyValuePair<string, string>("F", "Error. New password must not be empty!");
                    return Json(GetResult, sa);
                }
                else if(oldpass != "" && newpass != "" && encryptPass == newpass)
                {
                    GetResult = new KeyValuePair<string, string>("F", "Error. New password must be different from old password!");
                    return Json(GetResult, sa);
                }
                else
                {
                    if (helperClass.Encrypt(oldpass) == userItem.password)
                    {
                        if (helperClass.CheckPasswordIfIsValid(newpass) == true)
                        {
                            userItem.password = helperClass.Encrypt(newpass);
                        }
                        else
                        {
                            GetResult = new KeyValuePair<string, string>("F", "Password must contain 8 characters, 1 uppercase letter, 1 alpharethmitical and 1 special character at least!");
                            return Json(GetResult, sa);
                        }
                    }
                    else
                    {
                        GetResult = new KeyValuePair<string, string>("F", "Error. The old password is wrong!!");
                        return Json(GetResult, sa);
                    }
                }

                db.Users.Update(userItem);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                GetResult = new KeyValuePair<string, string>("EX", "Exception Error Occures during Delete, Please contact Admin :" + ex.Message);
                return Json(GetResult, sa);
            }
            GetResult = new KeyValuePair<string, string>("S", "Changes Saved Successfully!");

            return Json(GetResult, sa);
        }
    }
   
}

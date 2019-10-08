using BOS.Auth.Client;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models;
using BOS.StarterCode.Models.BOSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace BOS.StarterCode.Controllers
{
    [Authorize(Policy = "IsAuthenticated")]
    public class ProfileController : Controller
    {
        private readonly IAuthClient _bosAuthClient;

        public ProfileController(IAuthClient authClient)
        {
            _bosAuthClient = authClient;
        }

        public async Task<IActionResult> Index()
        {
            var model = await GetPageData();
            return View(model);
        }

        private async Task<dynamic> GetPageData()
        {
            var modules = HttpContext.Session.GetObject<List<Module>>("Modules");
            dynamic model = new ExpandoObject();
            model.Modules = modules;
            if (User.FindFirst(c => c.Type == "Username") != null || User.FindFirst(c => c.Type == "Role") != null)
            {
                model.Username = User.FindFirst(c => c.Type == "Username").Value.ToString();
                model.Roles = User.FindFirst(c => c.Type == "Role").Value.ToString();

                string userId = User.FindFirst(c => c.Type == "UserId").Value.ToString();
                var userInfo = await _bosAuthClient.GetUserByIdWithRolesAsync<User>(Guid.Parse(userId));
                if (userInfo.IsSuccessStatusCode)
                {
                    model.UserInfo = userInfo.User;
                }

                var availableRoles = await _bosAuthClient.GetRolesAsync<Role>();
                if (availableRoles.IsSuccessStatusCode)
                {
                    model.AvailableRoles = availableRoles.Roles;
                }
            }
            return model;
        }

        public IActionResult NavigateToModule(Guid id, string code, bool isDefault)
        {
            if (isDefault)
            {
                switch (code)
                {
                    case "MYPFL":
                        return RedirectToAction("Index", "Profile");
                    case "USERS":
                        return RedirectToAction("Index", "Users");
                    case "ROLES":
                        return RedirectToAction("Index", "Roles");
                    case "PRMNS":
                        return RedirectToAction("Index", "Permissions");
                    default:

                        return View("Index", GetPageData());
                }
            }
            return null;
        }

        [HttpPost]
        public async Task<string> UpdateUsername([FromBody]string username)
        {
            string userId = User.FindFirst(c => c.Type == "UserId").Value.ToString();
            var updatedUsernameResponse = await _bosAuthClient.UpdateUsernameAsync(Guid.Parse(userId), username);
            if (updatedUsernameResponse.IsSuccessStatusCode)
            {
                return "Username updated successfully";
            }
            else
            {
                return "Unable to update username at this time. Please try again later.";
            }
        }

        public ActionResult ChangePassword()
        {
            return View("ChangePassword");
        }

        public async Task<ActionResult> UpdatePassword(ChangePassword passwordObj)
        {
            string userId = User.FindFirst(c => c.Type == "UserId").Value.ToString();
            var response = await _bosAuthClient.ChangePasswordAsync(Guid.Parse(userId), passwordObj.CurrentPassword, passwordObj.NewPassword);
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "Password updated successfully";
                var model = await GetPageData();
                return View("Index", model);
                // return Redirect(Request.Headers["Referer"].ToString());
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
        }
    }
}
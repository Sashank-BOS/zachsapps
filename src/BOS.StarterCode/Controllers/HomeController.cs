using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models;
using BOS.StarterCode.Models.BOSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;

namespace BOS.StarterCode.Controllers
{
    [Authorize(Policy = "IsAuthenticated")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // dynamic model = GetPageData();
            return View(GetPageData());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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

        private dynamic GetPageData()
        {
            var modules = HttpContext.Session.GetObject<List<Module>>("Modules");
            dynamic model = new ExpandoObject();
            model.Modules = modules;
            model.Username = User.FindFirst(c => c.Type == "Username").Value.ToString();
            model.Roles = User.FindFirst(c => c.Type == "Role").Value.ToString();

            return model;
        }


        private Dictionary<string, string> GetOperationsForModule(Guid moduleId)
        {
            List<Operation> permOperationList = HttpContext.Session.GetObject<List<Operation>>("Operations");
            List<Operation> moduleOperationList = permOperationList.Where(a => a.ModuleId == moduleId).ToList();
            Dictionary<string, string> operationsDict = new Dictionary<string, string>();

            foreach (Operation operation in moduleOperationList)
            {
                operationsDict.Add(operation.Code, operation.Name);
            }
            return operationsDict;
        }
    }
}

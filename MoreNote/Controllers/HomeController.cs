﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoreNote.Logic.Business;

using MoreNote.Logic.Entity;
using MoreNote.Models;
using MoreNote.Value;
using System.Collections.Generic;
using System.Diagnostics;

namespace MoreNote.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            //return Content("An API listing authors of docs.asp.net.");
            ViewBag.title = "leanote";
            ViewBag.msg = LanguageResource.GetMsg();

            return View();
        }
     
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult ErrorPage()
        {
            
            return View();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MoreNote.Controllers
{
    public class APPController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "后台管理员界面";
            return View();
        }

    }
}
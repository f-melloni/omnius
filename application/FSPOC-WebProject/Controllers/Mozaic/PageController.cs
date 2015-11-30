﻿using FSS.Omnius.Modules.CORE;
using FSS.Omnius.Modules.Entitron.Entity.Mozaic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FSPOC_WebProject.Controllers.Mozaic
{
    public class PageController : Controller
    {
        public ActionResult Index(int id)
        {
            var tables = new CORE().Entitron.GetStaticTables();
            return View(tables.Pages.Where(p => p.ApplicationId == id));
        }
        public ActionResult Create(int id, int templateId)
        {
            string js = $"<script></script><div style='position:absolute;top:0;left:0;width:50px;height:50px;background-color:white;border:1px solid black;'><a href='/Mozaic/Page/Index/{id}'>Back</a></div>";

            Page page = new Page { ApplicationId = id, MasterTemplateId = templateId };
            page.MasterTemplate = new CORE().Entitron.GetStaticTables().Templates.SingleOrDefault(t => t.Id == templateId);

            string Html = page.MasterTemplate.Html;
            int indexOfEnd = Html.IndexOf("<body");
            ViewBag.Html = Html.Insert(indexOfEnd, js);
            return View();
        }
    }
}
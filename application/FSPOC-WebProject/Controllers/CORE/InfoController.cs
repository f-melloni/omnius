﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FSS.Omnius.Controllers.CORE
{
    public class InfoController : Controller
    {
        [PersonaAuthorize]
        // GET: Info
        public void Index()
        {
            Response.Write("App code using: ");
            Response.Write(System.Security.Principal.WindowsIdentity.GetCurrent().Name + "<br/>");
            Response.Write("Is user auth: ");
            Response.Write(User.Identity.IsAuthenticated.ToString() + "<br/>");
            Response.Write("Auth type: ");
            Response.Write(User.Identity.AuthenticationType + "<br/>");
            Response.Write("User name: ");
            Response.Write(User.Identity.Name + "<br/>");
        }

        public bool Leave()
        {
            Modules.CORE.CORE core = new Modules.CORE.CORE();

            core.Persona.getUser(User.Identity.Name).LastLogout = null;
            core.Entitron.GetStaticTables().SaveChanges();

            return true;
        }

        public bool LogOut()
        {
            Modules.CORE.CORE core = new Modules.CORE.CORE();

            core.Persona.getUser(User.Identity.Name).LastLogout = DateTime.UtcNow;
            core.Entitron.GetStaticTables().SaveChanges();

            return true;
        }
    }
}
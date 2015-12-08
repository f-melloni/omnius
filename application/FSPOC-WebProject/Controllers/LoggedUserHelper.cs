﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Security.Principal;
using FSS.Omnius.Modules.Entitron.Entity.Persona;
using FSS.Omnius.Modules.CORE;

namespace System
{
    public static partial class ExtendMethods
    {
        public static User GetLogged(this IPrincipal user, CORE core = null)
        {
            core = core ?? new CORE();
            return core.Persona.getUser(user.Identity.Name);
        }
    }
}
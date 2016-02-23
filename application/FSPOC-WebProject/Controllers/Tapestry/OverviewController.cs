﻿using System.Linq;
using System.Web.Mvc;
using FSS.Omnius.Modules.Entitron.Entity;
using FSS.Omnius.Modules.Entitron.Entity.Tapestry;
using FSS.Omnius.Modules.Entitron.Entity.Master;

namespace FSPOC_WebProject.Controllers.Tapestry
{
    [PersonaAuthorize(Roles = "Admin", Module = "Tapestry")]
    public class OverviewController : Controller
    {
        public ActionResult Index(FormCollection formParams)
        {
            using (var context = new DBEntities())
            {
                if (Request.HttpMethod == "POST")
                {

                    int metablockId = 0;
                    TapestryDesignerMetablock parentMetablock = null;
                    if (formParams["appId"] != null)
                    {
                        int appId = int.Parse(formParams["appId"]);
                        var rootMetablock = context.Applications.Include("TapestryDesignerRootMetablock")
                            .Where(c => c.Id == appId).First().TapestryDesignerRootMetablock;
                        if(rootMetablock == null)
                        {
                            var newRootMetablock = new TapestryDesignerMetablock
                            {
                                Name = "Root metablock"
                            };
                            context.TapestryDesignerMetablocks.Add(newRootMetablock);
                            context.Applications.Find(appId).TapestryDesignerRootMetablock = newRootMetablock;
                            newRootMetablock.ParentApp = context.Applications.Find(appId);
                            context.SaveChanges();
                            metablockId = newRootMetablock.Id;
                        }
                        else
                            metablockId = context.Applications.Include("TapestryDesignerRootMetablock")
                            .Where(c => c.Id == appId).First().TapestryDesignerRootMetablock.Id;
                        ViewData["appName"] = context.Applications.Find(appId).DisplayName;
                        ViewData["currentAppId"] = appId;
                    }
                    else
                    {
                        metablockId = int.Parse(formParams["metablockId"]);
                        parentMetablock = context.TapestryDesignerMetablocks.Include("ParentMetablock")
                            .Where(c => c.Id == metablockId).First().ParentMetablock;

                        Application app = GetApplication(parentMetablock, context); 

                        ViewData["appName"] = app.Name;
                        ViewData["currentAppId"] = app.Id;
                    }
                    ViewData["metablockId"] = metablockId;
                    if (parentMetablock == null)
                        ViewData["parentMetablockId"] = 0;
                    else
                        ViewData["parentMetablockId"] = parentMetablock.Id;
                }
                else
                {
                    var firstApp = context.Applications.Include("TapestryDesignerRootMetablock").First();
                    ViewData["metablockId"] = firstApp.TapestryDesignerRootMetablock.Id;
                    ViewData["parentMetablockId"] = 0;
                    ViewData["appName"] = firstApp.DisplayName;
                    ViewData["currentAppId"] = firstApp.Id;
                }
            }
            return View();
        }

        private Application GetApplication(TapestryDesignerMetablock parentMetablock, DBEntities context)
        {
            while(parentMetablock.ParentMetablock != null) {
                parentMetablock = context.TapestryDesignerMetablocks.Include("ParentMetablock")
                                                                    .Where(b => b.Id == parentMetablock.ParentMetablock.Id).First();
            }

            return context.Applications.Include("TapestryDesignerRootMetablock")
                                       .Where(a => a.TapestryDesignerRootMetablock.Id == parentMetablock.Id).First();
        }

    }
}

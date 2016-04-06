﻿using FSS.Omnius.Modules.Entitron.Entity.Persona;
using FSS.Omnius.Modules.Entitron.Entity.Tapestry;
using FSS.Omnius.Modules.Tapestry.Service;
using System;
using System.Web.Mvc;
using System.Linq;
using FSS.Omnius.Modules.Entitron;
using FSS.Omnius.Modules.Entitron.Entity;
using T = FSS.Omnius.Modules.Tapestry;
using C = FSS.Omnius.Modules.CORE;
using E = FSS.Omnius.Modules.Entitron;
using System.Collections.Generic;
using System.IO;
using System.Data;

namespace FSS.Omnius.Controllers.Tapestry
{
    [PersonaAuthorize]
    public class RunController : Controller
    {
        [HttpGet]
        public ActionResult Index(string appName, string blockIdentify = null, int modelId = -1, string message = null, string messageType = null)
        {
            using (var context = new DBEntities())
            {
                // init
                C.CORE core = HttpContext.GetCORE();
                if(!string.IsNullOrEmpty(appName))
                    core.Entitron.AppName = appName;
                core.User = User.GetLogged(core);

                // get block
                Block block = null;
                try
                {
                    int blockId = Convert.ToInt32(blockIdentify);
                    block = context.Blocks.SingleOrDefault(b => b.Id == blockId);
                }
                catch(FormatException)
                {
                    block = context.Blocks.FirstOrDefault(b => b.Name == blockIdentify);
                }

                try
                {
                    block = block ?? context.WorkFlows.FirstOrDefault(w => w.ApplicationId == core.Entitron.AppId && w.InitBlockId != null).InitBlock;
                }
                catch (NullReferenceException)
                {
                    return new HttpStatusCodeResult(404);
                }

                // fill data
                ViewData["appName"] = core.Entitron.Application.DisplayName;
                ViewData["appIcon"] = core.Entitron.Application.Icon;
                ViewData["pageName"] = block.DisplayName;
                ViewData["blockName"] = block.Name;

                DBItem modelRow = null;
                if(modelId != -1 && !string.IsNullOrEmpty(block.ModelName))
                {
                    modelRow = core.Entitron.GetDynamicTable(block.ModelName).GetById(modelId);
                }
                foreach (var resourceMappingPair in block.ResourceMappingPairs)
                {
                    DataTable dataSource = new DataTable();
                    var columnDisplayNameDictionary = new Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(resourceMappingPair.Sources[0].TableName)
                        && string.IsNullOrEmpty(resourceMappingPair.Sources[0].ColumnName))
                    {
                        string tableName = resourceMappingPair.Sources[0].TableName;
                        var entitronTable = core.Entitron.GetDynamicTable(tableName);
                        List<string> columnFilter = null;
                        bool getAllColumns = true;
                        if (!string.IsNullOrEmpty(resourceMappingPair.SourceColumnFilter))
                        {
                            columnFilter = resourceMappingPair.SourceColumnFilter.Split(',').ToList();
                            if (columnFilter.Count > 0)
                                getAllColumns = false;
                        }
                        var entitronColumnList = entitronTable.columns.OrderBy(c => c.ColumnId).ToList();
                        dataSource.Columns.Add("hiddenId", typeof(int));
                        foreach (var entitronColumn in entitronColumnList)
                        {
                            if (getAllColumns || columnFilter.Contains(entitronColumn.Name))
                            {
                                var columnMetadata = core.Entitron.Application.ColumnMetadata.FirstOrDefault(c => c.TableName == entitronTable.tableName
                                    && c.ColumnName == entitronColumn.Name);
                                if (columnMetadata != null && columnMetadata.ColumnDisplayName != null)
                                {
                                    dataSource.Columns.Add(columnMetadata.ColumnDisplayName);
                                    columnDisplayNameDictionary.Add(entitronColumn.Name, columnMetadata.ColumnDisplayName);
                                }
                                else
                                {
                                    dataSource.Columns.Add(entitronColumn.Name);
                                    columnDisplayNameDictionary.Add(entitronColumn.Name, entitronColumn.Name);
                                }
                            }
                        }
                        var entitronRowList = entitronTable.Select().ToList();
                        foreach (var entitronRow in entitronRowList)
                        {
                            if (resourceMappingPair.Sources[0].ConditionSets.Count == 0
                                || core.Entitron.filteringService.MatchConditionSets(resourceMappingPair.Sources[0].ConditionSets, entitronRow))
                            {
                                var newRow = dataSource.NewRow();
                                newRow["hiddenId"] = (int)entitronRow["id"];
                                foreach (var entitronColumn in entitronColumnList)
                                {
                                    if (getAllColumns || columnFilter.Contains(entitronColumn.Name))
                                    {
                                        if (entitronColumn.type == "bit")
                                        {
                                            if ((bool)entitronRow[entitronColumn.Name] == true)
                                                newRow[columnDisplayNameDictionary[entitronColumn.Name]] = "Ano";
                                            else
                                                newRow[columnDisplayNameDictionary[entitronColumn.Name]] = "Ne";
                                        }
                                        else
                                            newRow[columnDisplayNameDictionary[entitronColumn.Name]] = entitronRow[entitronColumn.Name];
                                    }
                                }
                                dataSource.Rows.Add(newRow);
                            }
                        }
                    }
                    if (resourceMappingPair.TargetType == "data-table-read-only" || resourceMappingPair.TargetType == "data-table-with-actions"
                        || resourceMappingPair.TargetType == "name-value-list")
                    {
                        ViewData["tableData_" + resourceMappingPair.TargetName] = dataSource;
                    }
                    else if (resourceMappingPair.TargetType == "dropdown-select" && string.IsNullOrEmpty(resourceMappingPair.Sources[0].ColumnName))
                    {
                        var dropdownDictionary = new Dictionary<int, string>();
                        foreach (DataRow datarow in dataSource.Rows)
                        {
                            dropdownDictionary.Add((int)datarow["hiddenId"], (string)datarow[columnDisplayNameDictionary["name"]]);
                        }
                        ViewData["dropdownData_" + resourceMappingPair.TargetName] = dropdownDictionary;
                    }
                    if (modelRow != null && !string.IsNullOrEmpty(resourceMappingPair.Sources[0].ColumnName)
                        && resourceMappingPair.TargetType == "checkbox")
                    {
                        ViewData["checkboxData_" + resourceMappingPair.TargetName] = modelRow[resourceMappingPair.Sources[0].ColumnName];
                    }
                    else if (modelRow != null && !string.IsNullOrEmpty(resourceMappingPair.Sources[0].ColumnName)
                        && (resourceMappingPair.TargetType == "input-single-line" || resourceMappingPair.TargetType == "input-multiline"))
                    {
                        ViewData["inputData_" + resourceMappingPair.TargetName] = modelRow[resourceMappingPair.Sources[0].ColumnName];
                    }
                    else if (modelRow != null && !string.IsNullOrEmpty(resourceMappingPair.Sources[0].ColumnName)
                        && resourceMappingPair.TargetType == "dropdown-select")
                    {
                        ViewData["dropdownSelection_" + resourceMappingPair.TargetName] = modelRow[resourceMappingPair.Sources[0].ColumnName];
                    }
                    if (!string.IsNullOrEmpty(resourceMappingPair.Sources[0].ColumnName) && resourceMappingPair.DataSourceParams == "currentUser"
                        && (resourceMappingPair.TargetType == "input-single-line" || resourceMappingPair.TargetType == "input-multiline"))
                    {
                        if (resourceMappingPair.Sources[0].TableName == "Omnius::Users")
                            switch (resourceMappingPair.Sources[0].ColumnName)
                            {
                                case "DisplayName":
                                    ViewData["inputData_" + resourceMappingPair.TargetName] = core.User.DisplayName;
                                    break;
                                case "Company":
                                    ViewData["inputData_" + resourceMappingPair.TargetName] = core.User.Company;
                                    break;
                                case "Job":
                                    ViewData["inputData_" + resourceMappingPair.TargetName] = core.User.Job;
                                    break;
                                case "Email":
                                    ViewData["inputData_" + resourceMappingPair.TargetName] = core.User.Email;
                                    break;
                                case "Address":
                                    ViewData["inputData_" + resourceMappingPair.TargetName] = core.User.Address;
                                    break;
                            }
                        else if (resourceMappingPair.Sources[0].TableName == "Users")
                        {
                            var epkUserRowList = core.Entitron.GetDynamicTable("Users").Select()
                                        .where(c => c.column("ad_email").Equal(core.User.Email)).ToList();
                            if (epkUserRowList.Count > 0)
                                ViewData["inputData_" + resourceMappingPair.TargetName] = epkUserRowList[0][resourceMappingPair.Sources[0].ColumnName];
                        }
                    }
                    else if (!string.IsNullOrEmpty(resourceMappingPair.Sources[0].ColumnName) && resourceMappingPair.DataSourceParams == "superior"
                        && (resourceMappingPair.TargetType == "input-single-line" || resourceMappingPair.TargetType == "input-multiline"))
                    {
                        var tableUsers = core.Entitron.GetDynamicTable("Users");
                        if (resourceMappingPair.Sources[0].TableName == "Users")
                        {
                            var epkUserRowList = tableUsers.Select().where(c => c.column("ad_email").Equal(core.User.Email)).ToList();
                            if (epkUserRowList.Count > 0)
                            {
                                int superiorId = (int)epkUserRowList[0]["h_pernr"];
                                var epkSuperiorRowList = tableUsers.Select()
                                        .where(c => c.column("pernr").Equal(superiorId)).ToList();
                                if (epkSuperiorRowList.Count > 0)
                                    ViewData["inputData_" + resourceMappingPair.TargetName] = epkSuperiorRowList[0][resourceMappingPair.Sources[0].ColumnName];
                            }
                        }
                    }
                }
                // show
                return View(block.MozaicPage.ViewPath);
            }
        }
        [HttpPost]
        public ActionResult Index(string appName, string button, FormCollection fc, string blockIdentify = null, int modelId = -1)
        {
            C.CORE core = HttpContext.GetCORE();
            core.Entitron.Application = core.Entitron.GetStaticTables().Applications.SingleOrDefault(a => a.Name == appName && a.IsEnabled && a.IsPublished && !a.IsSystem);

            using (DBEntities context = new DBEntities())
            {
                // get block
                Block block = null;
                try
                {
                    int blockId = Convert.ToInt32(blockIdentify);
                    block = context.Blocks.SingleOrDefault(b => b.Id == blockId);
                }
                catch (FormatException)
                {
                    block = context.Blocks.FirstOrDefault(b => b.Name == blockIdentify);
                }

                try
                {
                    block = block ?? context.WorkFlows.FirstOrDefault(w => w.ApplicationId == core.Entitron.AppId && w.InitBlockId != null).InitBlock;
                }
                catch (NullReferenceException)
                {
                    return new HttpStatusCodeResult(404);
                }

                // run
                var result = core.Tapestry.run(HttpContext.GetLoggedUser(), block, button, modelId, fc);

                // redirect
                return RedirectToRoute("Run", new { appName = appName, blockIdentify = result.Item2.Name, modelId = modelId, message = result.Item1.ToUser(), messageType = result.Item1.Type.ToString() });
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using FSPOC.Models;
using FSPOC.DAL;
using Logger;

namespace FSPOC.Controllers
{
    public class DatabaseApiController : Controller
    {
        [Route("api/database/commits")]
        [HttpGet]
        public ActionResult GetCommitList()
        {
            try
            {
                List<AjaxTransferCommitHeader> returnCommitList = new List<AjaxTransferCommitHeader>();
                using (var context = new WorkflowDbContext())
                {
                    try
                    {
                        var sourceCommitList = from c in context.DbSchemeCommits orderby c.Timestamp descending select c;
                        foreach (DbSchemeCommit c in sourceCommitList)
                            returnCommitList.Add(new AjaxTransferCommitHeader
                            {
                                Id = c.Id,
                                CommitMessage = c.CommitMessage,
                                TimeString = c.Timestamp.ToString("d. M. yyyy H:mm:ss")
                            });
                    }
                    catch (InvalidOperationException) // History is empty
                    {
                        Log.Info(String.Format("DatabaseDesigner: commit list requested but there are no commits yet. Returning an empty commit array."));
                        return Json(new List<AjaxTransferCommitHeader>(), JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(returnCommitList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("DatabaseDesigner: error when loading the commit history (GET api/database/commits). Exception message: {0}", ex.Message));
                return new HttpStatusCodeResult(500);
            }
        }
        private ActionResult getCommit(int commitId = -1)
        {
            AjaxTransferDbScheme result = new AjaxTransferDbScheme();
            using (var context = new WorkflowDbContext())
            {
                DbSchemeCommit requestedCommit = new DbSchemeCommit();
                try
                {
                    if (commitId == -1) // No commitId specified, get the lates commit by default
                        requestedCommit = (from c in context.DbSchemeCommits orderby c.Timestamp descending select c).First();
                    else
                        requestedCommit = (from c in context.DbSchemeCommits where c.Id.Equals(commitId) select c).First();
                }
                catch (InvalidOperationException ex) // History is empty
                {
                    if (commitId == -1)
                    {
                        Log.Info(String.Format("DatabaseDesigner: latest commit was requested, but there are no commits yet. Returning an empty commit."));
                        return Json(new AjaxTransferDbScheme(), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Log.Error(String.Format("DatabaseDesigner: the requested commit with id={1} doesn't exist. "
                            + "Exception message: {0}", ex.Message, commitId));
                        return new HttpStatusCodeResult(404);
                    }
                }
                foreach (var table in requestedCommit.Tables)
                {
                    AjaxTransferDbTable ajaxTable = new AjaxTransferDbTable
                    {
                        Id = table.Id,
                        Name = table.Name,
                        PositionX = table.PositionX,
                        PositionY = table.PositionY
                    };
                    foreach (var column in table.Columns)
                    {
                        ajaxTable.Columns.Add(new AjaxTransferDbColumn
                        {
                            Id = column.Id,
                            Name = column.Name,
                            Type = column.Type,
                            PrimaryKey = column.PrimaryKey,
                            AllowNull = column.AllowNull,
                            DefaultValue = column.DefaultValue,
                            ColumnLength = column.ColumnLength,
                            ColumnLengthIsMax = column.ColumnLengthIsMax
                        });
                    }
                    foreach (var index in table.Indices)
                    {
                        ajaxTable.Indices.Add(new AjaxTransferDbIndex
                        {
                            Id = index.Id,
                            Name = index.Name,
                            Unique = index.Unique,
                            FirstColumnName = index.FirstColumnName,
                            SecondColumnName = index.SecondColumnName
                        });
                    }
                    result.Tables.Add(ajaxTable);
                }
                foreach (var relation in requestedCommit.Relations)
                {
                    result.Relations.Add(new AjaxTransferDbRelation
                    {
                        LeftTable = relation.LeftTable,
                        RightTable = relation.RightTable,
                        LeftColumn = relation.LeftColumn,
                        RightColumn = relation.RightColumn,
                        Type = relation.Type
                    });
                }
                foreach (var view in requestedCommit.Views)
                {
                    result.Views.Add(new AjaxTransferDbView
                    {
                        Id = view.Id,
                        Name = view.Name,
                        Query = view.Query,
                        PositionX = view.PositionX,
                        PositionY = view.PositionY
                    });
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [Route("api/database/commits/{commitId:int}")]
        [HttpGet]
        public ActionResult GetCommitById(int commitId)
        {
            try
            {
                return getCommit(commitId);
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("DatabaseDesigner: error when loading the commit with id={1} (GET api/database/commits/{1}). "
                    + "Exception message: {0}", ex.Message, commitId));
                return new HttpStatusCodeResult(500);
            }
        }
        [Route("api/database/commits/latest")]
        [HttpGet]
        public ActionResult LoadLatest()
        {
            try
            {
                return getCommit();
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("DatabaseDesigner: error when loading the latest commit (GET api/database/commits/latest). "
                    + "Exception message: {0}", ex.Message));
                return new HttpStatusCodeResult(500);
            }
        }
        [Route("api/database/commits")]
        [HttpPost]
        public ActionResult SaveScheme(AjaxTransferDbScheme postData)
        {
            try
            {
                DbSchemeCommit commit = new DbSchemeCommit();
                using (var context = new WorkflowDbContext())
                {
                    commit.Timestamp = DateTime.Now;
                    commit.CommitMessage = postData.CommitMessage;
                    context.DbSchemeCommits.Add(commit);
                    Dictionary<int, int> tableIdMapping = new Dictionary<int, int>();
                    Dictionary<int, int> columnIdMapping = new Dictionary<int, int>();

                    foreach (var ajaxTable in postData.Tables)
                    {
                        int ajaxTableId = ajaxTable.Id;
                        DbTable newTable = new DbTable { Name = ajaxTable.Name, PositionX = ajaxTable.PositionX, PositionY = ajaxTable.PositionY };
                        commit.Tables.Add(newTable);
                        context.SaveChanges();
                        tableIdMapping.Add(ajaxTableId, newTable.Id);
                        foreach (var column in ajaxTable.Columns)
                        {
                            int ajaxColumnId = column.Id;
                            DbColumn newColumn = new DbColumn { Name = column.Name, Type = column.Type,
                                PrimaryKey = column.PrimaryKey, AllowNull = column.AllowNull, DefaultValue = column.DefaultValue,
                                ColumnLength = column.ColumnLength, ColumnLengthIsMax = column.ColumnLengthIsMax
                            };
                            newTable.Columns.Add(newColumn);
                            context.SaveChanges();
                            columnIdMapping.Add(ajaxColumnId, newColumn.Id);
                        }
                        foreach (var index in ajaxTable.Indices)
                        {
                            DbIndex newIndex = new DbIndex
                            {
                                Name = index.Name,
                                Unique = index.Unique,
                                FirstColumnName = index.FirstColumnName,
                                SecondColumnName = index.SecondColumnName
                            };
                            newTable.Indices.Add(newIndex);
                        }
                    }
                    foreach (var ajaxRelation in postData.Relations)
                    {
                        int leftTable = tableIdMapping[ajaxRelation.LeftTable];
                        int rightTable = tableIdMapping[ajaxRelation.RightTable];
                        int leftColumn = columnIdMapping[ajaxRelation.LeftColumn];
                        int rightColumn = columnIdMapping[ajaxRelation.RightColumn];
                        commit.Relations.Add(new DbRelation
                        {
                            LeftTable = leftTable,
                            RightTable = rightTable,
                            LeftColumn = leftColumn,
                            RightColumn = rightColumn,
                            Type = ajaxRelation.Type
                        });
                    }
                    foreach (var ajaxView in postData.Views)
                    {
                        commit.Views.Add(new DbView
                        {
                             Name = ajaxView.Name,
                             Query = ajaxView.Query,
                             PositionX = ajaxView.PositionX,
                             PositionY = ajaxView.PositionY
                        });
                    }
                    context.SaveChanges();
                    return new HttpStatusCodeResult(200);
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("DatabaseDesigner: an error occurred when saving the database scheme (POST api/database/commits). "
                    + "Exception message: {0}", ex.Message));
                return new HttpStatusCodeResult(500);
            }
        }
    }
}
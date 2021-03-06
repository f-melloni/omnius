﻿using FSS.Omnius.Modules.CORE;
using FSS.Omnius.Modules.Entitron.Entity;
using FSS.Omnius.Modules.Entitron.Entity.Master;
using FSS.Omnius.Modules.Entitron.Entity.Mozaic;
using FSS.Omnius.Modules.Entitron.Entity.Tapestry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSS.Omnius.Modules.Tapestry.Service
{
    public class TapestryGeneratorService
    {
        private string[] _splitGateways = new string[] { "gateway-x" };

        private COREobject _core;
        private DBEntities _context;
        private DBEntities _masterContext;
        private Application _app;
        private bool _rebuildInAction;
        private Random _random;
        private ModalProgressHandler<EModule> _progressHandler;

        private Dictionary<TapestryDesignerBlock, Block> _blockMapping;
        private HashSet<TapestryDesignerBlock> _blocksToBuild;

        public delegate void SendWS(string str);

        public TapestryGeneratorService(DBEntities masterContext, DBEntities context, bool rebuildInAction)
        {
            _blockMapping = new Dictionary<TapestryDesignerBlock, Block>();
            _blocksToBuild = new HashSet<TapestryDesignerBlock>();
            _masterContext = masterContext;
            _context = context;
            _rebuildInAction = rebuildInAction;
            _random = new Random();
        }

        public Dictionary<TapestryDesignerBlock, Block> GenerateTapestry(COREobject core, ModalProgressHandler<EModule> progressHandler)
        {
            _core = core;
            _progressHandler = progressHandler;
            _app = core.Application;

            // block & metablock
            try
            {
                // generate new
                progressHandler.SetMessage("block", "Generating metablocks and blocks", MessageType.InProgress, _app.TapestryDesignerMetablocks.Count);
                saveMetablock(_app.TapestryDesignerRootMetablock, true);
                progressHandler.SetMessage("block", "Generating metablocks and blocks completed", MessageType.Success);
            }
            catch (Exception)
            {
                _context.DiscardChanges();
                
                throw;
            }

            // block content
            try
            {
                progressHandler.SetMessage("blockContent", "Generating block content", MessageType.InProgress, _app.TapestryDesignerMetablocks.Sum(mb => mb.Blocks.Count));
                saveBlocks();
                _context.SaveChanges();
                progressHandler.SetMessage("blockContent", "Generating block content completed", MessageType.Success);
            }
            catch (Exception)
            {
                _context.DiscardChanges();

                throw;
            }

            return _blockMapping;
        }

        private WorkFlow saveMetablock(TapestryDesignerMetablock metaBlock, bool init = false)
        {
            try
            {
                string metaBlockName = metaBlock.Name.RemoveDiacritics();
                WorkFlow resultWF = _app.WorkFlows.SingleOrDefault(w => w.Name == metaBlockName);
                if (resultWF == null)
                {
                    resultWF = new WorkFlow
                    {
                        ApplicationId = _app.Id,
                        Name = metaBlock.Name.RemoveDiacritics(),
                        IsTemp = false
                    };
                    _context.WorkFlows.Add(resultWF);
                }
                resultWF.IsInMenu = metaBlock.IsInMenu;
                resultWF.Type = init ? _context.WorkFlowTypes.Single(t => t.Name == "Init") : _context.WorkFlowTypes.Single(t => t.Name == "Partial");
                _context.SaveChanges();

                // child meta block
                foreach (TapestryDesignerMetablock childMetablock in metaBlock.Metablocks.Where(mb => !mb.IsDeleted))
                {
                    WorkFlow wf = saveMetablock(childMetablock);
                    wf.Parent = resultWF;
                }
                _context.SaveChanges();

                // child block
                List<TapestryDesignerBlock> designerToBuild = _rebuildInAction
                    ? metaBlock.Blocks.Where(b => !b.IsDeleted).ToList()
                    : metaBlock.Blocks.Where(b => !b.IsDeleted && b.IsChanged).ToList();
                foreach (TapestryDesignerBlock childBlock in designerToBuild)
                {
                    try
                    {
                        TapestryDesignerBlockCommit commit = childBlock.BlockCommits.OrderByDescending(c => c.Timestamp).FirstOrDefault();

                        string modelName = null;
                        if (commit != null)
                        {
                            if (!string.IsNullOrEmpty(commit.ModelTableName))
                                modelName = commit.ModelTableName;
                            else if (!string.IsNullOrEmpty(commit.AssociatedTableName))
                                modelName = commit.AssociatedTableName.Split(',').First();
                        }

                        // find already builded
                        string blockName = childBlock.Name.RemoveDiacritics();
                        Block resultBlock = resultWF.Blocks.SingleOrDefault(b => b.Name == blockName);
                        // create block
                        if (resultBlock == null)
                        {
                            resultBlock = new Block();
                            resultWF.Blocks.Add(resultBlock);
                        }
                        else
                        {
                            IQueryable<int?> cgsToRemove = null;
                            if (_context != _masterContext)
                                cgsToRemove = _context.ActionRules.Where(ar => (ar.SourceBlock.IsVirtualForBlockId == resultBlock.Id || ar.SourceBlockId == resultBlock.Id) && ar.ConditionGroup != null).Select(ar => ar.ConditionGroupId);
                            
                            foreach(var ar in _context.ActionRules.Where(ar => ar.SourceBlockId == resultBlock.Id)) {
                                ar.ConditionGroup = null;
                                _context.ActionRules.Remove(ar);
                            }
                            foreach(var ar in _context.ActionRules.Where(ar => ar.TargetBlockId == resultBlock.Id || ar.TargetBlock.IsVirtualForBlockId == resultBlock.Id)) {
                                ar.ConditionGroup = null;
                                _context.ActionRules.Remove(ar);
                            }
                            _context.ResourceMappingPairs.RemoveRange(_context.ResourceMappingPairs.Where(mp => mp.BlockId == resultBlock.Id));
                            _context.SaveChanges();

                            _context.Blocks.RemoveRange(_context.Blocks.Where(b => b.IsVirtualForBlockId == resultBlock.Id));
                            resultBlock.InitForWorkFlow.Clear();

                            if (_context != _masterContext)
                                _context.TapestryDesignerConditionGroups.RemoveRange(_context.TapestryDesignerConditionGroups.Where(cg => cgsToRemove.Contains(cg.Id)));
                        }
                        // update
                        resultBlock.Name = childBlock.Name.RemoveDiacritics();
                        resultBlock.DisplayName = childBlock.Name;
                        resultBlock.ModelName = modelName;

                        // add init
                        if (childBlock.IsInitial)
                            resultBlock.InitForWorkFlow.Add(resultWF);

                        _blockMapping.Add(childBlock, resultBlock);
                        _blocksToBuild.Add(childBlock);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Block [{childBlock.Name}]: {e.Message}", e);
                    }
                }
                _context.SaveChanges();
                // remove deleted
                List<string> currentDesignerBlockNames = _masterContext.TapestryDesignerBlocks.Where(db => db.ParentMetablock_Id == metaBlock.Id && !db.IsDeleted).Select(b => b.Name).ToList();
                var deletedBlocks = _context.Blocks.Where(b => b.WorkFlowId == resultWF.Id && b.IsVirtualForBlock == null
                    && !currentDesignerBlockNames.Contains(b.DisplayName));
                _context.Blocks.RemoveRange(deletedBlocks);
                _context.SaveChanges();

                // map rest
                if (!_rebuildInAction)
                    foreach (TapestryDesignerBlock childBlock in metaBlock.Blocks.Where(b => !b.IsDeleted && !b.IsChanged))
                    {
                        string blockName = childBlock.Name.RemoveDiacritics();
                        _blockMapping[childBlock] = _context.Blocks.SingleOrDefault(b => b.WorkFlowId == resultWF.Id && b.Name == blockName);
                    }

                // DONE :)
                _progressHandler.IncrementProgress("block");
                return resultWF;
            }
            catch (Exception e)
            {
                throw new Exception($"Metablock [{metaBlock.Name}]: {e.Message}", e);
            }
        }

        private void saveBlocks()
        {
            bool abort = false;
            foreach(TapestryDesignerBlock block in _blocksToBuild)
            {
                try
                {
                    saveBlockContent(block);
                    _progressHandler.IncrementProgress("blockContent");
                }
                catch (Exception e)
                {
                    while (e.Message.Contains("An error occurred while updating the entries. See the inner exception for details."))
                        e = e.InnerException;

                    _progressHandler.Error($"{block.Name} - {e.Message}");
                    abort = true;
                }
            }
            _progressHandler.SetMessage("blockContent", "Saving generated block content", MessageType.InProgress);
            _context.SaveChanges();

            if (abort)
                throw new Exception("během aktualizace workflow došlo k chybám");
        }

        private void saveBlockContent(TapestryDesignerBlock block)
        {
            // block
            Block resultBlock = _blockMapping[block];
            var stateColumnMapping = new Dictionary<int, string>();

            TapestryDesignerBlockCommit commit = block.BlockCommits.OrderBy(bc => bc.Timestamp).LastOrDefault();
            if (commit == null) // no commit
                return;

            resultBlock.RoleWhitelist = commit.RoleWhitelist;

            // Resources
            foreach (TapestryDesignerResourceRule resourceRule in commit.ResourceRules)
            {
                var pair = saveResourceRule(resourceRule, _app, stateColumnMapping, resultBlock);
                //resultBlock.ResourceMappingPairs.Add(pair);
            }

            // ActionRule
            foreach (TapestryDesignerWorkflowRule workflowRule in commit.WorkflowRules)
            {
                saveWFRule(workflowRule, resultBlock, resultBlock.WorkFlow, stateColumnMapping);
            }

            if (!string.IsNullOrEmpty(commit.AssociatedPageIds))
            {
                var pageIdList = commit.AssociatedPageIds.Split(',').Select(int.Parse).ToList();
                Page mainPage = null;
                foreach (int pageId in pageIdList)
                {
                    var currentPage = _masterContext.MozaicEditorPages.Find(pageId);
                    if (!currentPage.IsModal)
                    {
                        mainPage = _context.Pages.Find(currentPage.CompiledPageId);
                        break;
                    }
                }
                resultBlock.EditorPageId = pageIdList[0];
                resultBlock.MozaicPage = mainPage;
            }
            if (!string.IsNullOrEmpty(commit.AssociatedBootstrapPageIds))
            {
                var pageIdList = commit.AssociatedBootstrapPageIds.Split(',').Select(int.Parse).ToList();
                Page mainPage = null;
                foreach (int pageId in pageIdList)
                {
                    var currentPage = _masterContext.MozaicBootstrapPages.Find(pageId);
                    mainPage = _context.Pages.Find(currentPage.CompiledPageId);
                    break;
                }
                resultBlock.BootstrapPageId = pageIdList[0];
                resultBlock.MozaicPage = mainPage;
            }
            //block.IsChanged = false;
        }

        private ResourceMappingPair saveResourceRule(TapestryDesignerResourceRule resourceRule, Application app, Dictionary<int, string> stateColumnMapping, Block resultBlock)
        {
            foreach (TapestryDesignerResourceConnection connection in resourceRule.Connections)
            {
                TapestryDesignerResourceItem source = connection.Source;
                TapestryDesignerResourceItem target = connection.Target;

                string targetName = "", targetType = "", dataSourceParams = "";

                if (source.ActionId == 1023 || source.ActionId == 1024)
                    continue;

                if (!string.IsNullOrEmpty(target.ComponentName))
                {
                    if (target.IsBootstrap == null || target.IsBootstrap == false)
                    {
                        var targetPage = target.Page;
                        var component = targetPage.Components.FirstOrDefault(c => c.Name == target.ComponentName);
                        if (component == null)
                        {
                            foreach (var parentComponent in targetPage.Components)
                            {
                                if (parentComponent.ChildComponents.Count > 0)
                                    component = parentComponent.ChildComponents.FirstOrDefault(c => c.Name == target.ComponentName);
                                if (component != null)
                                    break;
                            }
                        }
                        if (component != null)
                        {
                            targetName = component.Name;
                            targetType = component.Type;
                        }
                    }
                    else
                    {
                        var targetPage = target.BootstrapPage;
                        var component = targetPage.Components.FirstOrDefault(c => c.ElmId == target.ComponentName);
                        if (component == null)
                        {
                            foreach (var parentComponent in targetPage.Components)
                            {
                                if (parentComponent.ChildComponents.Count > 0)
                                    component = parentComponent.ChildComponents.FirstOrDefault(c => c.ElmId == target.ComponentName);
                                if (component != null)
                                    break;
                            }
                        }
                        if (component != null)
                        {
                            targetName = component.ElmId;
                            targetType = component.UIC;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(source.ColumnName))
                {
                    foreach (TapestryDesignerResourceConnection relatedConnection in resourceRule.Connections)
                    {
                        if (relatedConnection.TargetId == source.Id)
                        {
                            if (relatedConnection.Source.ActionId == 1023)
                                dataSourceParams = "currentUser";
                            else if (relatedConnection.Source.ActionId == 1024)
                                dataSourceParams = "superior";
                        }
                    }
                }
                if (source.StateId != null && !string.IsNullOrEmpty(target.ColumnName))
                {
                    stateColumnMapping.Add(source.StateId.Value, target.ColumnName);
                    continue;
                }
                ResourceMappingPair pair = new ResourceMappingPair
                {
                    relationType = $"{(source.Label.StartsWith("View:") ? "V:" : "")}{source.TypeClass}__{target.TypeClass}",

                    SourceComponentName = source.ComponentName,
                    SourceTableName = source.TableName,
                    SourceIsShared = source.IsShared,
                    SourceColumnName = source.ColumnName,
                    SourceColumnFilter = source.ColumnFilter,

                    TargetType = targetType,
                    TargetName = targetName,
                    TargetTableName = target.TableName,
                    TargetIsShared = target.IsShared,
                    TargetColumnName = target.ColumnName,

                    DataSourceParams = dataSourceParams,
                    Block = _blockMapping[connection.ResourceRule.ParentBlockCommit.ParentBlock],

                    BlockName = _blockMapping[connection.ResourceRule.ParentBlockCommit.ParentBlock].Name,
                    ApplicationName = _app.Name
                };

                resultBlock.ResourceMappingPairs.Add(pair);

                _context.SaveChanges();
                
                foreach (TapestryDesignerConditionGroup cg in source.ConditionGroups)
                {
                    // copy
                    if (_context != _masterContext)
                    {
                        // new conditionGroup
                        TapestryDesignerConditionGroup conditionGroup = new TapestryDesignerConditionGroup
                        {
                            Application = _app,
                            ResourceMappingPairId = pair.Id
                        };

                        // new conditionSet
                        foreach (TapestryDesignerConditionSet originConditionSet in cg.ConditionSets)
                        {
                            TapestryDesignerConditionSet conditionSet = new TapestryDesignerConditionSet
                            {
                                SetIndex = originConditionSet.SetIndex,
                                SetRelation = originConditionSet.SetRelation
                            };
                            conditionGroup.ConditionSets.Add(conditionSet);

                            // new condition
                            foreach (TapestryDesignerCondition originCondition in originConditionSet.Conditions)
                            {
                                TapestryDesignerCondition condition = new TapestryDesignerCondition
                                {
                                    Index = originCondition.Index,
                                    Operator = originCondition.Operator,
                                    Relation = originCondition.Relation,
                                    Value = originCondition.Value,
                                    Variable = originCondition.Variable,
                                };
                                conditionSet.Conditions.Add(condition);
                            }
                        }

                        _context.TapestryDesignerConditionGroups.Add(conditionGroup);
                        _context.SaveChanges();
                    }
                    else
                        cg.ResourceMappingPairId = pair.Id;
                }

                return pair;
            }
            return null;
        }

        private void saveWFRule(TapestryDesignerWorkflowRule workflowRule, Block block, WorkFlow wf, Dictionary<int, string> stateColumnMapping)
        {
            HashSet<TapestryDesignerWorkflowConnection> todoConnections = new HashSet<TapestryDesignerWorkflowConnection>();
            Dictionary<TapestryDesignerWorkflowItem, Block> BlockMapping = new Dictionary<TapestryDesignerWorkflowItem, Block>();
            Dictionary<Block, TapestryDesignerConditionGroup> conditionMapping = new Dictionary<Block, TapestryDesignerConditionGroup>();
            HashSet<Block> blockHasRights = new HashSet<Block> { block };

            // create virtual starting items
            TapestryDesignerWorkflowItem virtualBeginItem = new TapestryDesignerWorkflowItem();
            BlockMapping.Add(virtualBeginItem, block);
            foreach (TapestryDesignerWorkflowItem item in _masterContext.TapestryDesignerWorkflowItems.Where(i => i.ParentSwimlane.ParentWorkflowRule.Id == workflowRule.Id && (i.TypeClass == "uiItem" || i.SymbolType == "circle-single" || i.SymbolType == "envelope-start" || i.SymbolType == "circle-event")))
            {
                TapestryDesignerWorkflowConnection conn = new TapestryDesignerWorkflowConnection
                {
                    Source = virtualBeginItem,
                    Target = item
                };

                todoConnections.Add(conn);
            }

            //
            var splitItems = _masterContext.TapestryDesignerWorkflowItems.Where(i => i.ParentSwimlane.ParentWorkflowRule_Id == workflowRule.Id && i.TypeClass == "symbol" && _splitGateways.Contains(i.SymbolType) && (i.ParentForeach == null || i.SymbolType == "foreach")).ToList();
            var joinItems = _masterContext.TapestryDesignerWorkflowItems.Where(i => i.ParentSwimlane.ParentWorkflowRule_Id == workflowRule.Id && i.TargetToConnection.Count() > 1 && (i.ParentForeach == null || i.SymbolType == "foreach")).ToList();

            foreach (var splitItem in splitItems)
            {
                // block mapping
                int random = _random.Next() % 1000000;
                Block newBlock = new Block
                {
                    Name = $"split_{block.Name}_{random}",
                    DisplayName = $"split[{block.Name}_{random}]",
                    ModelName = block.ModelName,
                    IsVirtualForBlockId = block.Id
                };
                wf.Blocks.Add(newBlock);
                BlockMapping.Add(splitItem, newBlock);

                // conditions
                if (splitItem.SymbolType == "gateway-x")
                {
                    try
                    {
                        conditionMapping.Add(newBlock, splitItem.ConditionGroups.Single());
                    }
                    catch(InvalidOperationException ex)
                    {
                        throw new Exception("Gateway has no condition!", ex);
                    }
                }

                // rights
                if (checkBlockHasRights(splitItem))
                    blockHasRights.Add(newBlock);

                // todo connection
                int connectionCount = splitItem.SourceToConnection.Count;
                if (connectionCount == 0) // wrong WF
                    throw new Exception($"Workflow ends with gateway - {block.Name}: {workflowRule.Name}");
                if (connectionCount == 1) // missing way - return to real Block
                {
                    TapestryDesignerWorkflowConnection connection = splitItem.SourceToConnection.FirstOrDefault();
                    todoConnections.Add(connection);
                    todoConnections.Add(new TapestryDesignerWorkflowConnection { SourceSlot = connection.SourceSlot == 0 ? 1 : 0, Source = splitItem });
                }
                else // OK
                    foreach (TapestryDesignerWorkflowConnection connection in splitItem.SourceToConnection)
                        todoConnections.Add(connection);
            }

            foreach (var joinItem in joinItems)
            {
                if (BlockMapping.ContainsKey(joinItem)) // split item & join item are same
                    continue;

                // block mapping
                int random = _random.Next() % 1000000;
                Block newBlock = new Block
                {
                    Name = $"join_{block.Name}_{random}",
                    DisplayName = $"join[{block.Name}_{random}]",
                    ModelName = block.ModelName,
                    IsVirtualForBlockId = block.Id
                };
                wf.Blocks.Add(newBlock);
                BlockMapping.Add(joinItem, newBlock);

                // todo connection
                todoConnections.Add(new TapestryDesignerWorkflowConnection { Source = joinItem, Target = joinItem });
            }

            //// ACTIONS ////
            foreach (TapestryDesignerWorkflowConnection connection in todoConnections)
                createActionRule(workflowRule, block, BlockMapping[connection.Source], connection, ref BlockMapping, conditionMapping, stateColumnMapping, blockHasRights, ref wf);
        }

        private ActionRule createActionRule(TapestryDesignerWorkflowRule workflowRule, Block nonVirtualBlock, Block startBlock, TapestryDesignerWorkflowConnection connection,
            ref Dictionary<TapestryDesignerWorkflowItem, Block> blockMapping, Dictionary<Block, TapestryDesignerConditionGroup> conditionMapping, Dictionary<int, string> stateColumnMapping, HashSet<Block> blockHasRights,
            ref WorkFlow wf)
        {
            TapestryDesignerWorkflowItem item = connection.Target;

            // is there a button name?
            string init = item?.ComponentName;
            if (workflowRule.Name == "INIT" && nonVirtualBlock == startBlock) { // initial ActionRule
                init = "INIT";
            }
            if(item?.TypeClass == "symbol" && (item?.SymbolType == "envelope-start" || item?.SymbolType == "circle-event")) { // emailové workflow nebo socket listener
                init = item?.Label;
            }
            if(item?.TypeClass == "symbol" && item?.SymbolType == "circle-single" && workflowRule.Name != "INIT") { // REST ENDPOINT
                init = workflowRule.Name;
            }

            string ActorName = (init != null ? "Manual" : "Auto");
            ActionRule rule = new ActionRule
            {
                Actor = _context.Actors.Single(a => a.Name == ActorName),
                Name = $"{workflowRule.Name}: {startBlock.DisplayName} - {(_random.Next() % 1000000).ToString()}",
                ExecutedBy = init
            };

            // condition
            if (conditionMapping.ContainsKey(startBlock))
            {
                // branch true
                if (connection.SourceSlot == 0)
                {
                    // copy condition to new db
                    if (_masterContext != _context)
                    {
                        TapestryDesignerConditionGroup originConditionGroup = conditionMapping[startBlock];
                        // new conditionGroup
                        TapestryDesignerConditionGroup conditionGroup = new TapestryDesignerConditionGroup
                        {
                            Application = _app,
                            ResourceMappingPairId = originConditionGroup.ResourceMappingPairId
                        };
                        rule.ConditionGroup = conditionGroup;

                        // new conditionSet
                        foreach (TapestryDesignerConditionSet originConditionSet in originConditionGroup.ConditionSets)
                        {
                            TapestryDesignerConditionSet conditionSet = new TapestryDesignerConditionSet
                            {
                                SetIndex = originConditionSet.SetIndex,
                                SetRelation = originConditionSet.SetRelation
                            };
                            conditionGroup.ConditionSets.Add(conditionSet);

                            // new condition
                            foreach (TapestryDesignerCondition originCondition in originConditionSet.Conditions)
                            {
                                TapestryDesignerCondition condition = new TapestryDesignerCondition
                                {
                                    Index = originCondition.Index,
                                    Operator = originCondition.Operator,
                                    Relation = originCondition.Relation,
                                    Value = originCondition.Value,
                                    Variable = originCondition.Variable,
                                };
                                conditionSet.Conditions.Add(condition);
                            }
                        }

                        _context.TapestryDesignerConditionGroups.Add(conditionGroup);
                        _context.SaveChanges();
                    }
                    // same DB
                    else
                        rule.ConditionGroup = conditionMapping[startBlock];
                }

                // branch false
                else
                    rule.isDefault = true;
            }
            startBlock.SourceTo_ActionRules.Add(rule);
            // rights
            if (blockHasRights.Contains(startBlock) && item != null)
                AddActionRuleRights(rule, item.ParentSwimlane);

            TapestryDesignerWorkflowItem prevItem = null;
            while (item != null // end WF
                && (!blockMapping.ContainsKey(item) // split || join
                    || (prevItem == null && connection.Source == connection.Target))) // split || join but starting action item
            {
                switch (item.TypeClass)
                {
                    case "actionItem":
                        //if (item.ActionId == 181) // noAction
                        //    break;

                        string generatedInputVariables = "";
                        if (item.ActionId == 2005 || item.ActionId == 193) // Send mail, Send mail for each
                        {
                            foreach (var relatedConnections in workflowRule.Connections.Where(c => c.TargetId == item.Id))
                            {
                                if (relatedConnections.Source.TypeClass == "templateItem")
                                    generatedInputVariables = ";Template=s$" + relatedConnections.Source.Label;
                            }
                        }
                        if (item.ActionId == 3001 || item.ActionId == 3002 || item.ActionId == 3012) // Call SOAP, Call REST
                        {
                            foreach (var relatedConnections in workflowRule.Connections.Where(c => c.TargetId == item.Id))
                            {
                                if (relatedConnections.Source.Label?.StartsWith("WS: ") ?? false)
                                    generatedInputVariables = ";WSName=s$" + relatedConnections.Source.Label.Substring(4);
                            }
                        }
                        if (item.ActionId == 3004 || item.ActionId == 3005 || item.ActionId == 3006 || item.ActionId == 3007) // ExtDB Select / Insert / Update / Delete
                        {
                            foreach (var relatedConnections in workflowRule.Connections.Where(c => c.TargetId == item.Id))
                            {
                                if (relatedConnections.Source.Label?.StartsWith("ExtDB: ") ?? false)
                                    generatedInputVariables = ";dbName=s$" + relatedConnections.Source.Label.Substring(7);
                            }
                        }
                        if (item.ActionId == 3008) // Send to RabbitMQ
                        {
                            foreach (var relatedConnections in workflowRule.Connections.Where(c => c.TargetId == item.Id))
                            {
                                if (relatedConnections.Source.Label?.StartsWith("RabbitMQ: ") ?? false)
                                    generatedInputVariables = ";rabbitMQ=s$" + relatedConnections.Source.Label.Substring(10);
                            }
                        }
                        if (item.ActionId == 2007) // DataTable Response action
                        {
                            generatedInputVariables = ";BootstrapPageId=i$" + startBlock.BootstrapPageId;
                        }

                        ActionRule_Action result = new ActionRule_Action
                        {
                            ActionId = item.ActionId.Value,
                            Order = rule.ActionRule_Actions.Any() ? rule.ActionRule_Actions.Max(aar => aar.Order) + 1 : 1,
                            InputVariablesMapping = item.InputVariables + generatedInputVariables,
                            OutputVariablesMapping = item.OutputVariables,
                            VirtualParentId = item.ParentForeachId.HasValue ? item.ParentForeachId : null,
                            IsForeachStart = item.IsForeachStart,
                            IsForeachEnd = item.IsForeachEnd
                        };
                        rule.ActionRule_Actions.Add(result);
                        break;

                    case "targetItem":
                        if (_blockMapping.ContainsKey(item.Target))
                            rule.TargetBlock = _blockMapping[item.Target];
                        else
                            _progressHandler.Error($"Block [{item.Target.Name}] not found!");
                        break;

                    case "symbol":
                        // No needed now
                        //switch (item.SymbolType)
                        //{
                        //    case "gateway-x":
                        //        Block splitBlock = blockMapping[item];
                        //        // if not already in conditionMapping
                        //        if (!conditionMapping.ContainsKey(splitBlock))
                        //            conditionMapping.Add(splitBlock, item.Id);
                        //        break;
                        //}
                        break;
                    case "circle-thick":
                        break;
                    case "attributeItem":
                        break;
                    case "templateItem":
                        break;
                    case "stateItem":
                        if (stateColumnMapping.ContainsKey(item.StateId.Value))
                        {
                            string stateColumn = stateColumnMapping[item.StateId.Value];
                            ActionRule_Action setStateAction = new ActionRule_Action
                            {
                                ActionId = 1029,
                                Order = rule.ActionRule_Actions.Any() ? rule.ActionRule_Actions.Max(aar => aar.Order) + 1 : 1,
                                InputVariablesMapping = $"ColumnName=s${stateColumn};StateId=i${item.StateId.Value}",
                                OutputVariablesMapping = ""
                            };
                            rule.ActionRule_Actions.Add(setStateAction);
                        }
                        break;
                    case "circle-single":
                        break;
                    case "circle-single-dashed":
                        break;
                    case "uiItem":
                        // DONE
                        break;
                    case "database":
                        break;
                    case "comment":
                        break;
                    case "virtualAction":
                        switch(item.SymbolType) {
                            case "foreach":
                                HashSet<TapestryDesignerWorkflowConnection> todoConnections = new HashSet<TapestryDesignerWorkflowConnection>();

                                ActionRule_Action feResult = new ActionRule_Action
                                {
                                    ActionId = -1,
                                    Order = rule.ActionRule_Actions.Any() ? rule.ActionRule_Actions.Max(aar => aar.Order) + 1 : 1,
                                    InputVariablesMapping = item.InputVariables,
                                    OutputVariablesMapping = item.OutputVariables,
                                    VirtualAction = "foreach",
                                    VirtualItemId = item.ParentForeachId
                                };
                                rule.ActionRule_Actions.Add(feResult);

                                // Uložíme akce uvnitř cyklu
                                var splitItems = _masterContext.TapestryDesignerWorkflowItems.Where(i => i.ParentSwimlane.ParentWorkflowRule_Id == workflowRule.Id && i.TypeClass == "symbol" && _splitGateways.Contains(i.SymbolType) && i.ParentForeachId == item.ParentForeachId && i.Id != item.Id).ToList();
                                var joinItems = _masterContext.TapestryDesignerWorkflowItems.Where(i => i.ParentSwimlane.ParentWorkflowRule_Id == workflowRule.Id && (i.TargetToConnection.Count() > 1 || i.IsForeachStart == true) && i.ParentForeachId == item.ParentForeachId && i.Id != item.Id).ToList();

                                foreach (var splitItem in splitItems) {
                                    // block mapping
                                    int random = _random.Next() % 1000000;
                                    Block newBlock = new Block
                                    {
                                        Name = $"split_{nonVirtualBlock.Name}_{random}",
                                        DisplayName = $"split[{nonVirtualBlock.Name}_{random}]",
                                        ModelName = nonVirtualBlock.ModelName,
                                        IsVirtualForBlockId = nonVirtualBlock.Id
                                    };
                                    wf.Blocks.Add(newBlock);
                                    blockMapping.Add(splitItem, newBlock);

                                    // conditions
                                    if (splitItem.SymbolType == "gateway-x") {
                                        try {
                                            conditionMapping.Add(newBlock, splitItem.ConditionGroups.Single());
                                        }
                                        catch (InvalidOperationException ex) {
                                            throw new Exception("Gateway has no condition!", ex);
                                        }
                                    }

                                    // rights
                                    if (checkBlockHasRights(splitItem))
                                        blockHasRights.Add(newBlock);

                                    // todo connection
                                    int connectionCount = splitItem.SourceToConnection.Count;
                                    if (connectionCount == 0) // wrong WF
                                        throw new Exception($"Workflow ends with gateway - {nonVirtualBlock.Name}: {workflowRule.Name}");
                                    if (connectionCount == 1) // missing way - return to real Block
                                    {
                                        TapestryDesignerWorkflowConnection conn = splitItem.SourceToConnection.FirstOrDefault();
                                        todoConnections.Add(conn);
                                        todoConnections.Add(new TapestryDesignerWorkflowConnection { SourceSlot = conn.SourceSlot == 0 ? 1 : 0, Source = splitItem });
                                    }
                                    else // OK
                                        foreach (TapestryDesignerWorkflowConnection conn in splitItem.SourceToConnection)
                                            todoConnections.Add(conn);
                                }

                                foreach (var joinItem in joinItems) {
                                    if (blockMapping.ContainsKey(joinItem)) // split item & join item are same
                                        continue;

                                    // block mapping
                                    int random = _random.Next() % 1000000;
                                    Block newBlock = new Block
                                    {
                                        Name = $"join_{nonVirtualBlock.Name}_{random}",
                                        DisplayName = $"join[{nonVirtualBlock.Name}_{random}]",
                                        ModelName = nonVirtualBlock.ModelName,
                                        IsVirtualForBlockId = nonVirtualBlock.Id
                                    };
                                    wf.Blocks.Add(newBlock);
                                    blockMapping.Add(joinItem, newBlock);

                                    // todo connection
                                    todoConnections.Add(new TapestryDesignerWorkflowConnection { Source = joinItem, Target = joinItem });
                                }

                                foreach (TapestryDesignerWorkflowConnection conn in todoConnections)
                                    createActionRule(workflowRule, nonVirtualBlock, blockMapping[conn.Source], conn, ref blockMapping, conditionMapping, stateColumnMapping, blockHasRights, ref wf);

                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }

                // next item
                prevItem = item;
                item = workflowRule.Connections.FirstOrDefault(c => c.SourceId == item.Id)?.Target;
            }

            // real block not set (TargetItem)
            if (rule.TargetBlock == null)
            {
                // continue to next virtual Block
                if (item != null && blockMapping.ContainsKey(item))
                    rule.TargetBlock = blockMapping[item];
                // return to origin real Block
                else
                    rule.TargetBlock = nonVirtualBlock;
            }

            return rule;
        }

        private bool checkBlockHasRights(TapestryDesignerWorkflowItem item)
        {
            List<TapestryDesignerWorkflowConnection> connections = item.SourceToConnection.ToList();
            if (connections.Count < 2)
                return false;

            TapestryDesignerSwimlane originalSwimlane = item.ParentSwimlane;
            foreach (var connection in connections)
                if (connection.Target.ParentSwimlane != originalSwimlane)
                    return true;

            return false;
        }

        private void AddActionRuleRights(ActionRule rule, TapestryDesignerSwimlane swimlane)
        {
            if (string.IsNullOrWhiteSpace(swimlane.Roles))
                return;

            foreach (string roleName in swimlane.Roles.Split(','))
            {
                rule.ActionRuleRights.Add(new Entitron.Entity.Persona.ActionRuleRight
                {
                    ApplicationId = _app.Id,
                    AppRoleName = roleName,
                    Executable = true
                });
            }
        }
    }
}

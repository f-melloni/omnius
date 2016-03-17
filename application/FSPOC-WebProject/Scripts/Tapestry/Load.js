﻿function LoadBlock(commitId) {
    appId = $("#currentAppId").val();
    blockId = $("#currentBlockId").val();
    if (commitId)
        url = "/api/tapestry/apps/" + appId + "/blocks/" + blockId + "/commits/" + commitId;
    else
        url = "/api/tapestry/apps/" + appId + "/blocks/" + blockId;
    $.ajax({
        type: "GET",
        url: url,
        dataType: "json",
        error: function (request, status, error) {
            alert(request.responseText);
        },
        success: function (data) {
            ChangedSinceLastSave = false;
            $("#resourceRulesPanel .resourceRule").remove();
            $("#workflowRulesPanel .workflowRule").remove();
            $("#blockHeaderBlockName").text(data.Name);
            for (i = 0; i < data.ResourceRules.length; i++) {
                currentRuleData = data.ResourceRules[i];
                newRule = $('<div class="rule resourceRule" style="width: '+currentRuleData.Width+'px; height: '+currentRuleData.Height+'px; left: '
                    + currentRuleData.PositionX + 'px; top: ' + currentRuleData.PositionY + 'px;"></div>');
                newRule.attr("id", AssingID());
                $("#resourceRulesPanel .scrollArea").append(newRule);
                newRule.draggable({
                    containment: "parent",
                    revert: function (event, ui) {
                        return ($(this).collision("#resourceRulesPanel .resourceRule").length > 1);
                    },
                    stop: function (event, ui) {
                        ChangedSinceLastSave = true;
                    }
                });
                newRule.resizable({
                    start: function (event, ui) {
                        rule = $(this);
                        contentsWidth = 120;
                        contentsHeight = 40;
                        rule.find(".item").each(function (index, element) {
                            rightEdge = $(element).position().left + $(element).width();
                            if (rightEdge > contentsWidth)
                                contentsWidth = rightEdge;
                            bottomEdge = $(element).position().top + $(element).height();
                            if (bottomEdge > contentsHeight)
                                contentsHeight = bottomEdge;
                        });
                        rule.css("min-width", contentsWidth + 40);
                        rule.css("min-height", contentsHeight + 20);

                        limits = CheckRuleResizeLimits(rule, true);
                        rule.css("max-width", limits.horizontal - 10);
                        rule.css("max-height", limits.vertical - 10);
                    },
                    resize: function (event, ui) {
                        rule = $(this);
                        limits = CheckRuleResizeLimits(rule, true);
                        rule.css("max-width", limits.horizontal - 10);
                        rule.css("max-height", limits.vertical - 10);
                    },
                    stop: function (event, ui) {
                        instance = $(this).data("jsPlumbInstance");
                        instance.recalculateOffsets();
                        instance.repaintEverything();
                        ChangedSinceLastSave = true;
                    }
                });
                CreateJsPlumbInstanceForRule(newRule);
                newRule.droppable({
                    containment: ".resourceRule",
                    tolerance: "touch",
                    accept: ".toolboxItem",
                    greedy: true,
                    drop: function (e, ui) {
                        if (dragModeActive) {
                            dragModeActive = false;
                            droppedElement = ui.helper.clone();
                            droppedElement.removeClass("toolboxItem");
                            droppedElement.addClass("item");
                            $(this).append(droppedElement);
                            ruleContent = $(this);
                            leftOffset = $("#tapestryWorkspace").offset().left - ruleContent.offset().left + 20;
                            topOffset = $("#tapestryWorkspace").offset().top - ruleContent.offset().top;
                            droppedElement.offset({ left: droppedElement.offset().left + leftOffset, top: droppedElement.offset().top + topOffset });
                            ui.helper.remove();
                            AddToJsPlumb(droppedElement);
                            if (droppedElement.position().left + droppedElement.width() + 35 > ruleContent.width()) {
                                droppedElement.css("left", ruleContent.width() - droppedElement.width() - 40);
                                instance = ruleContent.data("jsPlumbInstance");
                                instance.repaintEverything();
                            }
                            if (droppedElement.position().top + droppedElement.height() + 5 > ruleContent.height()) {
                                console.log("matched");
                                droppedElement.css("top", ruleContent.height() - droppedElement.height() - 15);
                                instance = ruleContent.data("jsPlumbInstance");
                                instance.repaintEverything();
                            }
                            ChangedSinceLastSave = true;
                        }
                    }
                });
                for (j = 0; j < currentRuleData.ResourceItems.length; j++) {
                    currentItemData = currentRuleData.ResourceItems[j];
                    newItem = $('<div id="resItem' + currentItemData.Id + '" class="item" style="left: ' + currentItemData.PositionX + 'px; top: '
                        + currentItemData.PositionY + 'px;">'
                        + currentItemData.Label + '</div>');
                    if (currentItemData.PageId != null)
                        newItem.attr("pageId", currentItemData.PageId);
                    if (currentItemData.ComponentName != null)
                        newItem.attr("componentName", currentItemData.ComponentName);
                    if (currentItemData.TableName != null) {
                        newItem.data("columnFilter", currentItemData.ColumnFilter);
                        newItem.attr("tableName", currentItemData.TableName);
                        newItem.addClass("tableAttribute");
                    }
                    if (currentItemData.ColumnName != null) {
                        newItem.attr("columnName", currentItemData.ColumnName);
                    }
                    newItem.addClass(currentItemData.TypeClass);
                    newRule.append(newItem);
                    AddToJsPlumb(newItem);
                }
                currentInstance = newRule.data("jsPlumbInstance");
                for (j = 0; j < currentRuleData.Connections.length; j++) {
                    currentConnectionData = currentRuleData.Connections[j];
                    currentInstance.connect({
                        uuids: ["resItem" + currentConnectionData.Source + "RightMiddle"], target: "resItem" + currentConnectionData.Target
                    });
                }
            }
            for (i = 0; i < data.WorkflowRules.length; i++) {
                currentRuleData = data.WorkflowRules[i];
                newRule = $('<div class="rule workflowRule" style="width: ' + currentRuleData.Width + 'px; height: ' + currentRuleData.Height
                    + 'px; left: ' + currentRuleData.PositionX + 'px; top: ' + currentRuleData.PositionY + 'px;"><div class="workflowRuleHeader">'
                    + '<div class="verticalLabel" style="margin-top: 0px;">' + currentRuleData.Name + '</div></div><div class="swimlaneArea"></div></div>');
                newRule.attr("id", AssingID());
                $("#workflowRulesPanel .scrollArea").append(newRule);
                newRule.draggable({
                    containment: "parent",
                    handle: ".workflowRuleHeader",
                    revert: function (event, ui) {
                        return ($(this).collision("#workflowRulesPanel .workflowRule").length > 1);
                    },
                    stop: function (event, ui) {
                        ChangedSinceLastSave = true;
                    }
                });
                newRule.resizable({
                    start: function (event, ui) {
                        rule = $(this);
                        contentsWidth = 120;
                        contentsHeight = 40;
                        rule.find(".item").each(function (index, element) {
                            rightEdge = $(element).position().left + $(element).width();
                            if (rightEdge > contentsWidth)
                                contentsWidth = rightEdge;
                            bottomEdge = $(element).position().top + $(element).height();
                            if (bottomEdge > contentsHeight)
                                contentsHeight = bottomEdge;
                        });
                        rule.css("min-width", contentsWidth + 40);
                        rule.css("min-height", contentsHeight + 20);

                        limits = CheckRuleResizeLimits(rule, false);
                        rule.css("max-width", limits.horizontal - 10);
                        rule.css("max-height", limits.vertical - 10);
                    },
                    resize: function (event, ui) {
                        rule = $(this);
                        instance = rule.data("jsPlumbInstance");
                        instance.recalculateOffsets();
                        instance.repaintEverything();
                        limits = CheckRuleResizeLimits(rule, false);
                        rule.css("max-width", limits.horizontal - 10);
                        rule.css("max-height", limits.vertical - 10);
                    },
                    stop: function (event, ui) {
                        ChangedSinceLastSave = true;
                    }
                });
                CreateJsPlumbInstanceForRule(newRule);
                for (j = 0; j < currentRuleData.Swimlanes.length; j++) {
                    currentSwimlaneData = currentRuleData.Swimlanes[j];
                    newSwimlane = $('<div class="swimlane" style="height: ' + (100 / currentRuleData.Swimlanes.length) + '%;"><div class="swimlaneRolesArea"><div class="roleItemContainer"></div><div class="rolePlaceholder"><div class="rolePlaceholderLabel">Pokud chcete specifikovat roli<br />'
                        + 'přetáhněte ji do této oblasti</div></div></div><div class="swimlaneContentArea"></div></div>');
                    newRule.find(".swimlaneArea").append(newSwimlane);
                    if (currentSwimlaneData.Roles.length > 0)
                        newSwimlane.find(".swimlaneRolesArea .rolePlaceholder").remove();
                    for (k = 0; k < currentSwimlaneData.Roles.length; k++) {
                        newSwimlane.find(".swimlaneRolesArea .roleItemContainer").append($('<div class="roleItem">' + currentSwimlaneData.Roles[k] + '</div>'));
                    }
                    for (k = 0; k < currentSwimlaneData.WorkflowItems.length; k++) {
                        currentItemData = currentSwimlaneData.WorkflowItems[k];
                        newItem = $('<div id="wfItem' + currentItemData.Id + '" class="item" style="left: ' + currentItemData.PositionX + 'px; top: '
                            + currentItemData.PositionY + 'px;"><span class="itemLabel">' + currentItemData.Label + '</span></div>');
                        newItem.addClass(currentItemData.TypeClass);
                        if (currentItemData.ActionId != null)
                            newItem.attr('actionId', currentItemData.ActionId);
                        if (currentItemData.InputVariables != null)
                            newItem.data('inputVariables', currentItemData.InputVariables);
                        if (currentItemData.OutputVariables != null)
                            newItem.data('outputVariables', currentItemData.OutputVariables);
                        if (currentItemData.ComponentName != null)
                            newItem.attr('componentName', currentItemData.ComponentName);
                        if (currentItemData.TargetId != null)
                            newItem.attr('targetId', currentItemData.TargetId);
                        targetSwimlane = newRule.find(".swimlane").eq(currentSwimlaneData.SwimlaneIndex).find(".swimlaneContentArea");
                        targetSwimlane.append(newItem);
                        AddToJsPlumb(newItem);
                    }
                    for (k = 0; k < currentSwimlaneData.WorkflowSymbols.length; k++) {
                        currentSymbolData = currentSwimlaneData.WorkflowSymbols[k];
                        newSymbol = $('<img id="wfSymbol' + currentSymbolData.Id + '" class="symbol" symbolType="' + currentSymbolData.Type +
                            '" src="/Content/images/TapestryIcons/' + currentSymbolData.Type + '.png" style="left: ' + currentSymbolData.PositionX + 'px; top: '
                            + currentSymbolData.PositionY + 'px;" />');
                        if (currentSymbolData.Type == "circle-thick")
                            newSymbol.attr("endpoints", "final");
                        else if (currentSymbolData.Type.substr(0, 8) == "gateway-")
                            newSymbol.attr("endpoints", "gateway");
                        if (currentSymbolData.Condition != null)
                            newSymbol.data("condition", currentSymbolData.Condition);
                        targetSwimlane = newRule.find(".swimlane").eq(currentSwimlaneData.SwimlaneIndex).find(".swimlaneContentArea");
                        targetSwimlane.append(newSymbol);
                        AddToJsPlumb(newSymbol);
                    }
                }
                newRule.find(".swimlaneRolesArea").droppable({
                    tolerance: "touch",
                    accept: ".toolboxItem.roleItem",
                    greedy: true,
                    drop: function (e, ui) {
                        if (dragModeActive) {
                            dragModeActive = false;
                            roleExists = false;
                            $(this).find(".roleItem").each(function (index, element) {
                                if ($(element).text() == ui.helper.text())
                                    roleExists = true;
                            });
                            if (!roleExists) {
                                droppedElement = ui.helper.clone();
                                $(this).find(".rolePlaceholder").remove();
                                $(this).find(".roleItemContainer").append($('<div class="roleItem">' + droppedElement.text() + '</div>'));
                                ui.helper.remove();
                                ChangedSinceLastSave = true;
                            }
                        }
                    }
                });
                newRule.find(".swimlaneContentArea").droppable({
                    containment: ".swimlaneContentArea",
                    tolerance: "touch",
                    accept: ".toolboxSymbol, .toolboxItem",
                    greedy: false,
                    drop: function (e, ui) {
                        if (dragModeActive) {
                            dragModeActive = false;
                            droppedElement = ui.helper.clone();
                            if (droppedElement.hasClass("roleItem")) {
                                ui.draggable.draggable("option", "revert", true);
                                return false;
                            }
                            ruleContent = $(this);
                            ruleContent.append(droppedElement);
                            if (droppedElement.hasClass("toolboxSymbol")) {
                                droppedElement.removeClass("toolboxSymbol ui-draggable ui-draggable-dragging");
                                droppedElement.addClass("symbol");
                                leftOffset = $("#tapestryWorkspace").offset().left - ruleContent.offset().left;
                                topOffset = $("#tapestryWorkspace").offset().top - ruleContent.offset().top;
                            }
                            else {
                                droppedElement.removeClass("toolboxItem");
                                droppedElement.addClass("item");
                                leftOffset = $("#tapestryWorkspace").offset().left - ruleContent.offset().left + 38;
                                topOffset = $("#tapestryWorkspace").offset().top - ruleContent.offset().top - 18;
                            }
                            droppedElement.offset({ left: droppedElement.offset().left + leftOffset, top: droppedElement.offset().top + topOffset });
                            ui.helper.remove();
                            AddToJsPlumb(droppedElement);
                            if (droppedElement.position().top + droppedElement.height() + 10 > ruleContent.height()) {
                                console.log("matched");
                                droppedElement.css("top", ruleContent.height() - droppedElement.height() - 20);
                                instance = ruleContent.parents(".workflowRule").data("jsPlumbInstance");
                                instance.repaintEverything();
                            }
                            ChangedSinceLastSave = true;
                        }
                    }
                });
                currentInstance = newRule.data("jsPlumbInstance");
                for (j = 0; j < currentRuleData.Connections.length; j++) {
                    currentConnectionData = currentRuleData.Connections[j];
                    sourceId = (currentConnectionData.SourceType == 1 ? "wfSymbol" : "wfItem") + currentConnectionData.Source;
                    targetId = (currentConnectionData.TargetType == 1 ? "wfSymbol" : "wfItem") + currentConnectionData.Target;
                    if (currentConnectionData.SourceSlot == 1)
                        sourceEndpointUuid = "BottomCenter";
                    else
                        sourceEndpointUuid = "RightMiddle";
                    currentInstance.connect({ uuids: [sourceId + sourceEndpointUuid], target: targetId });
                }
            }
            appId = $("#currentAppId").val();
            url = "/api/database/apps/" + appId + "/commits/latest",
            $.ajax({
                type: "GET",
                url: url,
                dataType: "json",
                success: function (tableData) {
                    $("#libraryCategory-Attributes .libraryItem").remove();
                    for (i = 0; i < tableData.Tables.length; i++) {
                        $("#libraryCategory-Attributes").append($('<div libId="' + ++lastLibId + '" libType="table-attribute" class="libraryItem tableAttribute" tableName="'
                            + tableData.Tables[i].Name + '">Table: ' + tableData.Tables[i].Name + '</div>'));
                    }
                    AssociatedTableIds = data.AssociatedTableIds;
                    AssociatedTableName = data.AssociatedTableName;
                    $("#blockHeaderDbResCount").text(data.AssociatedTableIds.length);
                    somethingWasAdded = false;
                    for (i = 0; i < data.AssociatedTableIds.length; i++) {
                        currentTable = tableData.Tables.filter(function (value) {
                            return value.Id == data.AssociatedTableIds[i];
                        })[0];
                        if (currentTable != undefined) {
                            for (j = 0; j < currentTable.Columns.length; j++) {
                                $("#libraryCategory-Attributes").append($('<div libId="' + ++lastLibId + '" libType="column-attribute" class="libraryItem columnAttribute" tableName="'
                                    + currentTable.Name + '" columnName="' + currentTable.Columns[j].Name + '">' + currentTable.Name + '.' + currentTable.Columns[j].Name + '</div>'));
                            }
                        }
                    };
                }
            });
            $('#libraryCategory-Actions .libraryItem').remove();
            url = "/api/tapestry/actions";
            $.ajax({
                type: "GET",
                url: url,
                dataType: "json",
                success: function (data) {
                    for(i = 0; i < data.Items.length; i++)
                    {
                        var action = data.Items[i];
                        $('#libraryCategory-Actions').append('<div libId="' + ++lastLibId + '" libType="action" class="libraryItem" actionId="' + action.Id + '">' + action.Name + '</div>');
                    }
                }
            });

            $('#libraryCategory-Roles .libraryItem').remove();
            url = "/api/Persona/app-roles/" + appId;
            $.ajax({
                type: "GET",
                url: url,
                dataType: "json",
                success: function (data) {
                    for(i = 0; i < data.Roles.length; i++) {
                        role = data.Roles[i];
                        $('#libraryCategory-Roles').append('<div libId="' + role.Id + '" libType="role" class="libraryItem">' + role.Name + '</div>');
                    }
                }
            });

            $('#libraryCategory-States .libraryItem').remove();
            url = "/api/Persona/app-states/" + appId;
            $.ajax({
                type: "GET",
                url: url,
                dataType: "json",
                success: function (data) {
                    for (i = 0; i < data.States.length; i++) {
                        state = data.States[i];
                        $('#libraryCategory-States').append('<div libId="' + ++lastLibId + '" libType="state" class="libraryItem" stateId="' + state.Id + '">' + state.Name + '</div>');
                    }
                }
            });


            $('#libraryCategory-Targets .libraryItem').remove();
            url = "/api/tapestry/apps/" + appId + "/blocks";
            $.ajax({
                type: "GET",
                url: url,
                dataType: "json",
                success: function(data) {
                    for(i = 0; i < data.ListItems.length; i++) {
                        var item = data.ListItems[i];
                        $('#libraryCategory-Targets').append('<div libId="' + ++lastLibId + '" libType="target" class="libraryItem" targetId="' + item.Id + '">' + item.Name + '</div>');
                    }
                }
            });

            $('#libraryCategory-Templates .libraryItem').remove();
            url = "/api/hermes/" + appId + "/templates";
            $.ajax({
                type: "GET",
                url: url,
                dataType: "json",
                success: function (data) {
                    for (i = 0; i < data.length; i++) {
                        $('#libraryCategory-Templates').append('<div libId="' + data[i].Id + '" libType="template" class="libraryItem">' + data[i].Name + '</div>');
                    }
                }
            });

            $('#libraryCategory-Integration .libraryItem').remove();
            url = "/api/nexus/" + appId + "/gateways";
            $.ajax({
                type: "GET",
                url: url,
                dataType: "json",
                success: function (data) {
                    for (i = 0; i < data.Ldap.length; i++) {
                        $('#libraryCategory-Integration').append('<div libId="' + ++lastLibId + '" libType="ldap" class="libraryItem">LDAP: ' + data.Ldap[i].Name + '</div>');
                    }
                    for (i = 0; i < data.WS.length; i++) {
                        $('#libraryCategory-Integration').append('<div libId="' + ++lastLibId + '" libType="ws" libSubType="' + data.WS[i].Type + '" class="libraryItem">WS: ' + data.WS[i].Name + '</div>');
                    }
                    for (i = 0; i < data.SMTP.length; i++) {
                        $('#libraryCategory-Integration').append('<div libId="' + ++lastLibId + '" libType="smtp" class="libraryItem">SMTP: ' + data.SMTP[i].Name + '</div>');
                    }
                    for (i = 0; i < data.WebDAV.length; i++) {
                        $('#libraryCategory-Integration').append('<div libId="' + ++lastLibId + '" libType="webdav" class="libraryItem">WebDAV: ' + data.WebDAV[i].Name + '</div>');
                    }
                }
            });
            
            AssociatedPageIds = data.AssociatedPageIds;
            $("#blockHeaderScreenCount").text(data.AssociatedPageIds.length);
            $("#libraryCategory-UI .libraryItem").remove();
            for (i = 0; i < data.AssociatedPageIds.length; i++) {
                pageId = data.AssociatedPageIds[i];
                url = "/api/mozaic-editor/apps/" + appId + "/pages/" + pageId;
                $.ajax({
                    type: "GET",
                    url: url,
                    dataType: "json",
                    success: function (data) {
                        for (i = 0; i < data.Components.length; i++) {
                            if (i == 0) {
                                $("#libraryCategory-UI").append('<div libId="' + ++lastLibId + '" pageId="' + data.Id + '" libType="ui" class="libraryItem">Screen: '
                                    + data.Name + '</div>');
                            }
                            cData = data.Components[i];
                            $("#libraryCategory-UI").append('<div libId="' + ++lastLibId + '" pageId="' + data.Id + '" componentName="' + cData.Name + '" libType="ui" class="libraryItem">'
                            + cData.Name + '</div>');
                            if (cData.Type == "data-table-with-actions") {
                                $("#libraryCategory-UI").append('<div libId="' + ++lastLibId + '" pageId="' + data.Id + '" componentName="datatable_edit" libType="ui" class="libraryItem">'
                                    + cData.Name + '_EditAction</div>');
                                $("#libraryCategory-UI").append('<div libId="' + ++lastLibId + '" pageId="' + data.Id + '" componentName="datatable_detail" libType="ui" class="libraryItem">'
                                    + cData.Name + '_DetailsAction</div>');
                                $("#libraryCategory-UI").append('<div libId="' + ++lastLibId + '" pageId="' + data.Id + '" componentName="datatable_delete" libType="ui" class="libraryItem">'
                                    + cData.Name + '_DeleteAction</div>');
                            }
                            if (cData.ChildComponents) {
                                for (j = 0; j < cData.ChildComponents.length; j++) {
                                    $("#libraryCategory-UI").append('<div libId="' + ++lastLibId + '" pageId="' + cData.ChildComponents[j].Id + '" componentName="' + cData.ChildComponents[j].Name + '" libType="ui" class="libraryItem">'
                                    + cData.ChildComponents[j].Name + '</div>');
                                }
                            }
                        }
                    }
                });
            }
        }
    });
};

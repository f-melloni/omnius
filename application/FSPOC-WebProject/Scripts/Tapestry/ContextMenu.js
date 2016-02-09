﻿$(function () {
    if (CurrentModuleIs("tapestryModule")) {
        $.contextMenu({
            selector: '.item, .symbol',
            trigger: 'right',
            zIndex: 300,
            callback: function (key, options) {
                item = options.$trigger;
                if (key == "delete") {
                    currentInstance = item.parents(".rule").data("jsPlumbInstance");
                    currentInstance.removeAllEndpoints(item, true);
                    item.remove();
                    ChangedSinceLastSave = true;
                }
            },
            items: {
                "delete": { name: "Delete", icon: "delete" }
            }
        });
        $.contextMenu({
            selector: '.resourceRule',
            trigger: 'right',
            zIndex: 300,
            callback: function (key, options) {
                item = options.$trigger;
                if (key == "delete") {
                    item.remove();
                    ChangedSinceLastSave = true;
                }
            },
            items: {
                "delete": { name: "Delete rule", icon: "delete" }
            }
        });
        $.contextMenu({
            selector: '.swimlaneRolesArea .roleItem',
            trigger: 'right',
            zIndex: 300,
            callback: function (key, options) {
                item = options.$trigger;
                if (key == "delete") {
                    item.parents(".swimlaneRolesArea").append($('<div class="rolePlaceholder"><div class="rolePlaceholderLabel">'
                        + 'Pokud chcete specifikovat roli<br />přetáhněte ji do této oblasti</div></div>'));
                    item.remove();
                    ChangedSinceLastSave = true;
                }
            },
            items: {
                "delete": { name: "Remove role", icon: "delete" }
            }
        });
        $.contextMenu({
            selector: '.workflowRule',
            trigger: 'right',
            zIndex: 300,
            callback: function (key, options) {
                if (key == "rename") {
                    currentRule = options.$trigger;
                    renameRuleDialog.dialog("open");
                }
                else if (key == "delete") {
                    options.$trigger.remove();
                    ChangedSinceLastSave = true;
                }
                else if (key == "add-swimlane") {
                    rule = options.$trigger;
                    newSwimlane = $('<div class="swimlane"><div class="swimlaneRolesArea"><div class="rolePlaceholder"><div class="rolePlaceholderLabel">Pokud chcete specifikovat roli<br />'
                        + 'přetáhněte ji do této oblasti</div></div></div><div class="swimlaneContentArea"></div></div>');
                    newSwimlane.find(".swimlaneRolesArea").droppable({
                        containment: ".swimlaneContentArea",
                        tolerance: "touch",
                        accept: ".toolboxItem.roleItem",
                        greedy: true,
                        drop: function (e, ui) {
                            droppedElement = ui.helper.clone();
                            $(this).find(".rolePlaceholder, .roleItem").remove();
                            $(this).append($('<div class="roleItem">' + droppedElement.text() + '</div>'));
                            ui.helper.remove();
                            ChangedSinceLastSave = true;
                        }
                    });
                    newSwimlane.find(".swimlaneContentArea").droppable({
                        containment: ".swimlaneContentArea",
                        tolerance: "touch",
                        accept: ".toolboxSymbol, .toolboxItem",
                        greedy: false,
                        drop: function (e, ui) {
                            droppedElement = ui.helper.clone();
                            if (droppedElement.hasClass("roleItem")) {
                                ui.draggable.draggable("option", "revert", true);
                                return false;
                            }
                            $(this).append(droppedElement);
                            ruleContent = $(this);
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
                            ChangedSinceLastSave = true;
                        }
                    });
                    rule.find(".swimlaneArea").append(newSwimlane);
                    newHeight = (100 / rule.find(".swimlane").length);
                    rule.find(".swimlane").each(function (swimlaneIndex, swimlaneDiv) {
                        $(swimlaneDiv).css("height", newHeight + "%");
                    });
                    instance = options.$trigger.data("jsPlumbInstance");
                    instance.recalculateOffsets();
                    instance.repaintEverything();
                    ChangedSinceLastSave = true;
                }
            },
            items: {
                "add-swimlane": { name: "Add swimlane", icon: "add" },
                "rename": { name: "Rename rule", icon: "edit" },
                "delete": { name: "Delete rule", icon: "delete" }
            }
        });
        $.contextMenu({
            selector: '.swimlane',
            trigger: 'right',
            zIndex: 300,
            callback: function (key, options) {
                if (key == "remove-swimlane") {
                    rule = options.$trigger.parents(".workflowRule");
                    swimlaneCount = rule.find(".swimlane").length;
                    if (swimlaneCount > 1) {
                        instance = rule.data("jsPlumbInstance");
                        instance.removeAllEndpoints(options.$trigger, true);
                        options.$trigger.remove();
                        newHeight = 100 / (swimlaneCount - 1);
                        rule.find(".swimlane").each(function (swimlaneIndex, swimlaneDiv) {
                            $(swimlaneDiv).css("height", newHeight + "%");
                        });
                        instance.recalculateOffsets();
                        instance.repaintEverything();
                        ChangedSinceLastSave = true;
                    }
                    else
                        alert("Pravidlo musí mít alspoň jednu swimlane, nelze smazat všechny.");
                }
            },
            items: {
                "remove-swimlane": { name: "Remove swimlane", icon: "delete" },
            }
        });
    }
});

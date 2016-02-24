﻿$(function () {
    if (CurrentModuleIs("overviewModule")) {
        addBlockDialog = $("#add-block-dialog").dialog({
            autoOpen: false,
            resizable: false,
            width: 400,
            height: 180,
            buttons: {
                "Add": function () {
                    addBlockDialog_SubmitData();
                },
                Cancel: function () {
                    addBlockDialog.dialog("close");
                }
            },
            create: function () {
                $(this).keypress(function (e) {
                    if (e.keyCode == $.ui.keyCode.ENTER) {
                        addBlockDialog_SubmitData();
                        return false;
                    }
                })
            },
            open: function () {
                $(this).find("#block-name").val("");
                $(this).find("#table-dropdown option").remove();
                for (i = 1; i < 6; i++) {
                    fakeTableName = "Table" + i; // TODO: replace with real data - load from a list of FSPOC tables
                    $(this).find("#table-dropdown").append($('<option value="' + fakeTableName + '">' + fakeTableName + '</option>'));
                }
            }
        });
        function addBlockDialog_SubmitData() {
            blockName = addBlockDialog.find("#block-name").val();
            tableName = addBlockDialog.find("#table-dropdown").val();
            newBlock = $('<div class="block"><div class="blockName">' + blockName + '</div><div class="blockInfo"></div></div>');
            $("#overviewPanel .scrollArea").append(newBlock);
            instance.draggable(newBlock, { containment: "parent" });
            newBlock.css("top", $("#overviewPanel").scrollTop() + 20);
            newBlock.css("left", $("#overviewPanel").scrollLeft() + 20);
            newBlock.on("dblclick", function () {
                blockToOpen = $(this);
                SaveMetablock(function () {
                    openBlockForm = $("#openBlockForm");
                    openBlockForm.find("input[name='blockId']").val(blockToOpen.attr("blockId"));
                    openBlockForm.submit();
                });
            });
            addBlockDialog.dialog("close");
        }
        addMetablockDialog = $("#add-metablock-dialog").dialog({
            autoOpen: false,
            resizable: false,
            width: 400,
            height: 180,
            buttons: {
                "Add": function () {
                    addMetablockDialog_SubmitData();
                },
                Cancel: function () {
                    addMetablockDialog.dialog("close");
                }
            },
            create: function () {
                $(this).keypress(function (e) {
                    if (e.keyCode == $.ui.keyCode.ENTER) {
                        addMetablockDialog_SubmitData();
                        return false;
                    }
                })
            },
            open: function () {
                $(this).find("#metablock-name").val("");
            }
        });
        function addMetablockDialog_SubmitData() {
            metablockName = addMetablockDialog.find("#metablock-name").val();
            newMetablock = $('<div class="metablock"><div class="metablockName">'
                + metablockName + '</div><div class="metablockSymbol fa fa-th-large"></div><div class="metablockInfo"></div></div>');
            $("#overviewPanel .scrollArea").append(newMetablock);
            instance.draggable(newMetablock, { containment: "parent" });
            newMetablock.css("top", $("#overviewPanel").scrollTop() + 20);
            newMetablock.css("left", $("#overviewPanel").scrollLeft() + 20);
            newMetablock.on("dblclick", function () {
                metablockToOpen = $(this);
                SaveMetablock(function () {
                    openMetablockForm = $("#openMetablockForm");
                    openMetablockForm.find("input[name='metablockId']").val(metablockToOpen.attr("metablockId"));
                    openMetablockForm.submit();
                });
            });
            addMetablockDialog.dialog("close");
        }
        renameMetablockDialog = $("#rename-metablock-dialog").dialog({
            autoOpen: false,
            width: 400,
            height: 190,
            buttons: {
                "Save": function () {
                    renameMetablockDialog_SubmitData();
                },
                Cancel: function () {
                    renameMetablockDialog.dialog("close");
                }
            },
            create: function () {
                $(this).keypress(function (e) {
                    if (e.keyCode == $.ui.keyCode.ENTER) {
                        renameMetablockDialog_SubmitData();
                        return false;
                    }
                })
            },
            open: function () {
                renameMetablockDialog.find("#metablock-name").val($("#headerMetablockName").text());
            }
        });
        function renameMetablockDialog_SubmitData() {
            renameMetablockDialog.dialog("close");
            $("#headerMetablockName").text(renameMetablockDialog.find("#metablock-name").val());
        }

        blockPropertiesDialog = $('#block-properties-dialog').dialog({
            autoOpen: false,
            width: 500,
            height: 250,
            buttons: {
                "Save": function () {
                    blockPropertiesDialog_SubmitData();
                },
                "Cancel": function () {
                    blockPropertiesDialog.dialog("close");
                }
            },
            create: function () {
                $(this).keypress(function (e) {
                    if (e.keyCode == $.ui.keyCode.ENTER) {
                        blockPropertiesDialog_SubmitData();
                        return false;
                    }
                });
            },
            open: function () {
                blockPropertiesDialog.find("#p-block-name").val(currentBlock.find('.blockName').text());
                blockPropertiesDialog.find("#block-is-in-menu").prop('checked', currentBlock.data('IsInMenu'));
                blockPropertiesDialog.find("#block-set-as-initial").prop('checked', currentBlock.attr('isinitial') == 'true');
            }
        });
        function blockPropertiesDialog_SubmitData() {
            blockPropertiesDialog.dialog("close");
            currentBlock.data("IsInMenu", blockPropertiesDialog.find("#block-is-in-menu").is(':checked'));
            
            var isInitial = blockPropertiesDialog.find("#block-set-as-initial").is(':checked') ? true : false;
            if (isInitial) {
                $("#overviewPanel .block").each(function (index, element) {
                    $(element).attr("isInitial", false);
                    $(element).find(".blockInfo").text("");
                });
            }
            currentBlock.attr("isInitial", isInitial);
            currentBlock.find(".blockInfo").text(isInitial ? "Initial" : "");

            if (blockPropertiesDialog.find("#p-block-name").val().length) {
                currentBlock.find('.blockName').html(blockPropertiesDialog.find("#p-block-name").val());
            }
        }

        metablockPropertiesDialog = $('#metablock-properties-dialog').dialog({
            autoOpen: false,
            width: 500,
            height: 250,
            buttons: {
                "Save": function () {
                    metablockPropertiesDialog_SubmitData();
                },
                "Cancel": function () {
                    metablockPropertiesDialog.dialog("close");
                }
            },
            create: function () {
                $(this).keypress(function (e) {
                    if (e.keyCode == $.ui.keyCode.ENTER) {
                        metablockPropertiesDialog_SubmitData();
                        return false;
                    }
                });
            },
            open: function () {
                metablockPropertiesDialog.find("#p-metablock-name").val(currentMetablock.find('.metablockName').text());
                metablockPropertiesDialog.find("#metablock-is-in-menu").prop('checked', currentMetablock.data('IsInMenu'));
                metablockPropertiesDialog.find("#metablock-set-as-initial").prop('checked', currentMetablock.attr('isinitial') == 'true');
            }
        });
        function metablockPropertiesDialog_SubmitData() {
            metablockPropertiesDialog.dialog("close");

            var isInitial = metablockPropertiesDialog.find("#metablock-set-as-initial").is(':checked') ? true : false;
            if(isInitial)
            {
                $("#overviewPanel .metablock").each(function (index, element) {
                    $(element).attr("isInitial", false);
                    $(element).find(".metablockInfo").text("");
                });
            }
            currentMetablock.attr("isInitial", isInitial);
            currentMetablock.find(".metablockInfo").text(isInitial ? "Initial" : "");

            currentMetablock.data("IsInMenu", metablockPropertiesDialog.find("#metablock-is-in-menu").is(':checked'));
            if (metablockPropertiesDialog.find("#p-metablock-name").val().length) {
                currentMetablock.find('.metablockName').html(metablockPropertiesDialog.find("#p-metablock-name").val());
            }
        }
    }
});

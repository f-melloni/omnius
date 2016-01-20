﻿function SaveMetablock(callback) {
    blockArray = [];
    metablockArray = [];

    $("#overviewPanel .block").each(function (blockIndex, blockDiv) {
        currentBlock = $(blockDiv);
        currentBlock.attr("tempId", blockIndex);
        if (currentBlock.attr("blockId") == undefined) {
            isNew = true;
            currentBlock.attr("blockId", blockIndex);
        }
        else
            isNew = false;
        blockArray.push({
            Id: currentBlock.attr("blockId"),
            Name: currentBlock.find(".blockName").text(),
            AssociatedTableName: currentBlock.find(".tableName").text(),
            AssociatedTableId: currentBlock.attr("tableId"),
            PositionX: parseInt(currentBlock.css("left")),
            PositionY: parseInt(currentBlock.css("top")),
            IsNew: isNew
        });
    });
    $("#overviewPanel .metablock").each(function (metablockIndex, metablockDiv) {
        currentMetablock = $(metablockDiv);
        currentMetablock.attr("tempId", metablockIndex);
        if (currentMetablock.attr("metablockId") == undefined) {
            isNew = true;
            currentMetablock.attr("metablockId", metablockIndex);
        }
        else
            isNew = false;
        metablockArray.push({
            Id: currentMetablock.attr("metablockId"),
            Name: currentMetablock.find(".metablockName").text(),
            PositionX: parseInt(currentMetablock.css("left")),
            PositionY: parseInt(currentMetablock.css("top")),
            IsNew: isNew
        });
    });
    postData = {
        Name: $("#headerMetablockName").text(),
        Blocks: blockArray,
        Metablocks: metablockArray
    };
    appId = $("#currentAppId").val();
    metablockId = $("#currentMetablockId").val();
    $.ajax({
        type: "POST",
        url: "/api/tapestry/apps/" + appId + "/metablocks/" + metablockId,
        dataType: "json",
        data: postData,
        error: function () { alert("ERROR") },
        success: function (data) {
            for (i = 0; i < data.BlockIdPairs.length; i++) {
                temporaryId = data.BlockIdPairs[i].TemporaryId;
                realId = data.BlockIdPairs[i].RealId;
                $("#overviewPanel .block[tempId='" + temporaryId + "']").attr("blockId", realId);
            }
            for (i = 0; i < data.MetablockIdPairs.length; i++) {
                temporaryId = data.MetablockIdPairs[i].TemporaryId;
                realId = data.MetablockIdPairs[i].RealId;
                $("#overviewPanel .metablock[tempId='" + temporaryId + "']").attr("metablockId", realId);
            }
            if (callback)
                callback();
        }
    });
}
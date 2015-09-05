﻿function SaveDbScheme(commitMessage) {
    columnIdCounter = 0;
    tableArray = [];
    relationArray = [];
    viewArray = [];
    $("#database-container .dbTable").each(function (tableIndex, tableDiv) {
        columnArray = [];
        indexArray = [];
        $(tableDiv).data("dbTableId", tableIndex);
        $(tableDiv).find(".dbColumn").each(function (columnIndex, columnDiv) {
            columnArray.push({
                Id: columnIdCounter,
                Name: $(columnDiv).find(".dbColumnName").text(),
                Type: $(columnDiv).attr("dbColumnType"),
                PrimaryKey: $(columnDiv).hasClass("dbPrimaryKey"),
                AllowNull: $(columnDiv).data("dbAllowNull"),
                DefaultValue: $(columnDiv).data("dbDefaultValue"),
                ColumnLength: $(columnDiv).data("dbColumnLength"),
                ColumnLengthIsMax: $(columnDiv).data("dbColumnLengthMax")
            });
            $(columnDiv).data("dbColumnId", columnIdCounter);
            columnIdCounter++;
        });
        $(tableDiv).find(".dbIndex").each(function (indexIndex, indexDiv) {
            indexArray.push({
                Id: indexIndex,
                Name: $(indexDiv).data("indexName"),
                FirstColumnName: $(indexDiv).data("firstColumn"),
                SecondColumnName: $(indexDiv).data("secondColumn"),
                Unique: $(indexDiv).data("unique")
            });
        });
        tableArray.push({
            Id: tableIndex,
            Name: $(tableDiv).find(".dbTableName").text(),
            PositionX: parseInt($(tableDiv).css("left")),
            PositionY: parseInt($(tableDiv).css("top")),
            Columns: columnArray,
            Indices: indexArray
        });
    });
    jsPlumbConnections = instance.getAllConnections();

    for (i = 0; i < jsPlumbConnections.length; i++) {
        currentConnection = jsPlumbConnections[i];
        sourceDiv = $(currentConnection.source);
        targetDiv = $(currentConnection.target);
        relationArray.push({
            LeftTable: sourceDiv.parents(".dbTable").data("dbTableId"),
            rightTable: targetDiv.parents(".dbTable").data("dbTableId"),
            LeftColumn: sourceDiv.data("dbColumnId"),
            RightColumn: targetDiv.data("dbColumnId"),
            Type: $(currentConnection).data("relationType")
        });
    }
    $("#database-container .dbView").each(function (viewIndex, viewDiv) {
        viewArray.push({
            Id: viewIndex,
            Name: $(viewDiv).data("dbViewName"),
            Query: $(viewDiv).data("dbViewQuery"),
            PositionX: parseInt($(viewDiv).css("left")),
            PositionY: parseInt($(viewDiv).css("top"))
        });
    });
    postData = {
        CommitMessage: commitMessage,
        Tables: tableArray,
        Relations: relationArray,
        Views: viewArray
    }
    $.ajax({
        type: "POST",
        url: "/api/database/commits",
        data: postData,
        success: function () { alert("OK") },
        error: function () { alert("ERROR") }
    });
}
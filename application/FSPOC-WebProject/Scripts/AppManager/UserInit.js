﻿$(function () {
    $("#appManagerIcon").on("click", function () {
        if (!$("#appManagerIcon").hasClass("activeIcon"))
            window.location.href = "/Master";
    });
    $("#adminSectionIcon").on("click", function () {
        if (!$("#adminSectionIcon").hasClass("activeIcon"))
            window.location.href = "/CORE";
    });
    $("#helpIcon").on("click", function () {
        if (!$("#helpIcon").hasClass("activeIcon"))
            window.location.href = "/Master/Home/Help";
    });
    $("#hideMenuIcon").on("click", function () {
        $("#userLeftBar").hide();
        $("#userLeftBarMinimized").show();
        $("#userContentArea").css("left", 60);
        $("#helpContainer").css("left", 60);
    });
    $("#showMenuIcon").on("click", function () {
        $("#userLeftBarMinimized").hide();
        $("#userLeftBar").show();
        $("#userContentArea").css("left", 310);
        $("#helpContainer").css("left", 310);
    });
    if (CurrentModuleIs("appManagerModule")) {
        $(".appPanel").draggable({
            grid: [120, 120],
            revert: "invalid",
            stop: function () {
                $(this).draggable("option", "revert", "invalid");
            }
        });
        $(".appWorkspace").droppable({
            tolerance: "fit"
        });
        $(".appPanel").droppable({
            greedy: true,
            tolerance: "touch",
            drop: function (event, ui) {
                ui.draggable.draggable("option", "revert", true);
            }
        });
        $(".appPanel").bind("dragstart", function (event, ui) {
            ui.originalPosition.top = $(this).position().top;
            ui.originalPosition.left = $(this).position().left;
        });
    }
    else if (CurrentModuleIs("helpModule")) {
        $("#appManagerIcon").removeClass("activeIcon");
        $("#helpIcon").addClass("activeIcon");
    }
    else if (CurrentModuleIs("userDetailsModule")) {
        $("#appManagerIcon").removeClass("activeIcon");
    };
});
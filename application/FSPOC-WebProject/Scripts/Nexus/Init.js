﻿$(function () {
    if (CurrentModuleIs("nexusModule")) {
        if ($("#ldapMenuArea").length) {
            $("#nexusMenuLDAP").addClass("highlighted");
        }
        else if ($("#wsMenuArea").length) {
            $("#nexusMenuWebServices").addClass("highlighted");
        }
        else if ($("#extDbMenuArea").length) {
            $("#nexusMenuExtDB").addClass("highlighted");
        }
        $("#nexusMenuLDAP").on("click", function () {
            if(!$("#nexusMenuLDAP").hasClass("highlighted"))
                window.location.href = "/Nexus/LDAP";
        });
        $("#nexusMenuWebServices").on("click", function () {
            if (!$("#nexusMenuWebServices").hasClass("highlighted"))
                window.location.href = "/Nexus/WS";
        });
        $("#nexusMenuExtDB").on("click", function () {
            if (!$("#nexusMenuExtDB").hasClass("highlighted"))
                window.location.href = "/Nexus/ExtDB";
        });
        $("#nexusMenuExtDB").on("click", function () {
            //window.location.href = "/Nexus/LDAP";
        });
        $("#hideNexusPanelIcon").on("click", function () {
            $("#nexusUpperPanel").hide();
            $("#minimizedNexusUpperPanel").show();
            $("#nexusLowerPanel").css("top", 115);
        });
        $("#showNexusPanelIcon").on("click", function () {
            $("#minimizedNexusUpperPanel").hide();
            $("#nexusUpperPanel").show();
            $("#nexusLowerPanel").css("top", 432);
        });
    }
});
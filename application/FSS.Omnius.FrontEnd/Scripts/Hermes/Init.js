﻿$(function () {
    if (CurrentModuleIs("hermesModule")) {
        if ($("#smtpMenuArea").length) {
            $("#hermesMenuSMTP").addClass("highlighted");
        }
        if ($("#templateMenuArea").length) {
            $("#hermesMenuTemplate").addClass("highlighted");
        }
        if ($("#queueMenuArea").length) {
            $("#hermesMenuQueue").addClass("highlighted");
        }
    }
});

﻿// IE Buster

if (!window.jQuery) {
    var message;
    if (/^Mozilla\/4\.0.*\bMSIE\b/.test(navigator.userAgent)) {
        // (emulované) IE5 .. IE8 se hlásí jako Mozilla/4.0, novější prohlížeče jako Mozilla/5.0 a fungují
        message = "Omlouváme se, ale Vaše verze Internet Exploreru nepodporuje základní funkce jazyka Javascript, " +
        "které jsou pro chod aplikace nezbytné.  Kontaktujte helpdesk nebo administrátory platformy. ";
    } else {
        message = "Omlouváme se, ale verze Vašeho prohlížeče nepodporuje základní funkce jazyka Javascript, " +
            "které jsou pro chod aplikace nezbytné.  Kontaktujte helpdesk nebo administrátory platformy. ";
    }
    var style = "body {background: white !important} div {margin: 25px; border: 5px solid red; padding: 25px; font-weight: bold}";

    document.body.innerHTML = "<div>" + message + "</div><style> " + style + "</style>";
}

function CurrentModuleIs(moduleClass) {
    return $("body").hasClass(moduleClass) ? true : false;
}

var DataTablesLocales = {
    "cs": {
        "sEmptyTable": "Tabulka neobsahuje žádná data",
        "sInfo": "Zobrazuji _START_ až _END_ z celkem _TOTAL_ záznamů",
        "sInfoEmpty": "Zobrazuji 0 až 0 z 0 záznamů",
        "sInfoFiltered": "(filtrováno z celkem _MAX_ záznamů)",
        "sInfoPostFix": "",
        "sInfoThousands": " ",
        "sLengthMenu": "Zobraz záznamů _MENU_",
        "sLoadingRecords": "Načítám...",
        "sProcessing": "Provádím...",
        "sSearch": "Hledat:",
        "sZeroRecords": "Žádné záznamy nebyly nalezeny",
        "oPaginate": {
            "sFirst": "První",
            "sLast": "Poslední",
            "sNext": "Další",
            "sPrevious": "Předchozí"
        },
        "oAria": {
            "sSortAscending": ": aktivujte pro řazení sloupce vzestupně",
            "sSortDescending": ": aktivujte pro řazení sloupce sestupně"
        }
    },
    "en": {
        "sEmptyTable": "No data available in table",
        "sInfo": "Showing _START_ to _END_ of _TOTAL_ entries",
        "sInfoEmpty": "Showing 0 to 0 of 0 entries",
        "sInfoFiltered": "(filtered from _MAX_ total entries)",
        "sInfoPostFix": "",
        "sInfoThousands": ",",
        "sLengthMenu": "Show _MENU_ entries",
        "sLoadingRecords": "Loading...",
        "sProcessing": "Processing...",
        "sSearch": "Search:",
        "sZeroRecords": "No matching records found",
        "oPaginate": {
            "sFirst": "First",
            "sLast": "Last",
            "sNext": "Next",
            "sPrevious": "Previous"
        },
        "oAria": {
            "sSortAscending": ": activate to sort column ascending",
            "sSortDescending": ": activate to sort column descending"
        }
    }
}

function GetDataTablesLocaleBundle(locale) {
    var store = DataTablesLocales[locale];
    if (store == null) {
        return DataTablesLocales["cs"];
    } else {
        return store;
    }
}

function CreateDataTable(element, locale, simpleMode) {
    var localeBundle = GetDataTablesLocaleBundle(locale);
    featureSwitch = !simpleMode;
    element.DataTable({
        "paging": featureSwitch,
        "pageLength": 50,
        "lengthMenu": [[10, 20, 50, 100, 200, 500, 1000, -1], [10, 20, 50, 100, 200, 500, 1000, "Vše"]],
        "info": featureSwitch,
        "filter": featureSwitch,
        "order": [[0, "desc"]],
        "language": localeBundle,
        "drawCallback": function () {
            if (typeof InitDataTable == 'function') {
                InitDataTable($(this));
            }
        }
    });

}

function CreateCzechDataTable(element, simpleMode) {
    CreateDataTable(element, "cs", simpleMode);
}

jQuery(function ($) {
    $.datepicker.regional['cs'] = {
        closeText: 'Zavřít',
        prevText: 'Předchozí',
        nextText: 'Další',
        currentText: 'Dnes',
        monthNames: ['Leden', 'Únor', 'Březen', 'Duben', 'Květen', 'Červen', 'Červenec', 'Srpen', 'Září', 'Říjen', 'Listopad', 'Prosinec'],
        monthNamesShort: ['Led', 'Úno', 'Bře', 'Dub', 'Kvě', 'Čer', 'Čvc', 'Srp', 'Zář', 'Říj', 'Lis', 'Pro'],
        dayNames: ['Neděle', 'Pondělí', 'Úterý', 'Středa', 'Čtvrtek', 'Pátek', 'Sobota'],
        dayNamesShort: ['Ne', 'Po', 'Út', 'St', 'Čt', 'Pá', 'So', ],
        dayNamesMin: ['Ne', 'Po', 'Út', 'St', 'Čt', 'Pá', 'So'],
        weekHeader: 'Týd',
        dateFormat: 'd. M. yyyy H:mm:ss',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: ''
    };
    $.datepicker.setDefaults($.datepicker.regional['cs']);
});
$.countdown.setDefaults($.countdown.regionalOptions['cs']);
jQuery.datetimepicker.setLocale('cs');
function CreateColorPicker(target) {
    var targetValue = target.val();
    var initialColor = targetValue ? targetValue : '#f00';
    target.spectrum({
        showPaletteOnly: true,
        togglePaletteOnly: true,
        togglePaletteMoreText: 'more',
        togglePaletteLessText: 'less',
        color: initialColor,
        clickoutFiresChange: true,
        palette: [
            ["#000", "#444", "#666", "#999", "#ccc", "#eee", "#f3f3f3", "#fff"],
            ["#f00", "#f90", "#ff0", "#0f0", "#0ff", "#00f", "#90f", "#f0f"],
            ["#f4cccc", "#fce5cd", "#fff2cc", "#d9ead3", "#d0e0e3", "#cfe2f3", "#d9d2e9", "#ead1dc"],
            ["#ea9999", "#f9cb9c", "#ffe599", "#b6d7a8", "#a2c4c9", "#9fc5e8", "#b4a7d6", "#d5a6bd"],
            ["#e06666", "#f6b26b", "#ffd966", "#93c47d", "#76a5af", "#6fa8dc", "#8e7cc3", "#c27ba0"],
            ["#c00", "#e69138", "#f1c232", "#6aa84f", "#45818e", "#3d85c6", "#674ea7", "#a64d79"],
            ["#900", "#b45f06", "#bf9000", "#38761d", "#134f5c", "#0b5394", "#351c75", "#741b47"],
            ["#600", "#783f04", "#7f6000", "#274e13", "#0c343d", "#073763", "#20124d", "#4c1130"]
        ]
    });
}
moment.locale("cs");
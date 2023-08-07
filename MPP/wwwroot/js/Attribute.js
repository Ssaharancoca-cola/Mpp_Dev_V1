﻿$(document).ready(function () {
    $("#btnAddNew").click(function () {
        debugger;
        $("#cmdName")[0].value = "AttributeDetail";
        $('#viewPlaceHolder').empty();
        $('#btnUpdate').hide();
    });
    $("#btnSearch").click(function () {
        $('#btnUpdate').show();
    });
    $("#btnViewAll").click(function () {
        $("#cmdName")[0].value = "viewPlaceHolder";
        $('#btnUpdate').show();
    });
    $("#btnExport").click(function () {

        $("#cmdName")[0].value = "AttributeDetail";
        $('#viewPlaceHolder').empty();
        $('#btnUpdate').hide();
    });
    $("#btnImport").click(function () {

        $("#cmdName")[0].value = "AttributeDetail";
        $('#viewPlaceHolder').empty();
        $('#btnUpdate').hide();
    });
    $("#btnWorkflow").click(function () {

        $("#cmdName")[0].value = "AttributeDetail";
        $('#viewPlaceHolder').empty();
        $('#btnUpdate').hide();
    });
    $(function () {
        $("#txtFrom").datepicker({ changeMonth: true, changeYear: true, yearRange: "2023:2050" });
        $("#txtTo").datepicker({ changeMonth: true, changeYear: true, yearRange: "2023:2050" });
    });
});

function onBegin() {
    $('#divLoading').show();

}
function onSuccess(result) {
    var r = result;
    debugger;
    $('#lblcaption').val('@ViewData["EntityName"]');
    var fileId = $('#cmdName').val();
    if (r.indexOf("error") == 0) {
        $('#' + fileId).html("");
        $('#btnUpdate').hide();
        $('#lblTotalRecords').html("0");
        alert(r.replace("error", ""));
    }
    else {
        var startindex = r.indexOf("totalrecord");
        var endindex = r.indexOf("</div>");
        var count = r.substring(startindex + 11, endindex);

        $('#' + fileId).html(result);
        if (startindex > 0) {
            $('#lblTotalRecords').html(count);
            var pagecount = Math.ceil(count / 50);
            $('#lblCurrentPageDetails').html(pagecount);
        }
        $('#lblSearchWarning').html("");
    }
}
function onFailure(error) {
    alert(error);
}
function onComplete() {
    $('#divLoading').hide();
    $('#divLoad').hide();
}
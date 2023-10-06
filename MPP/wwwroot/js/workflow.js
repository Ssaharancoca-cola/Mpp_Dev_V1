$(document).ready(function () {
    $("#btnCancel").click(function () {
        $("#cmdName")[0].value = "AttributeDetail";
        $('#viewPlaceHolder').empty();


    });
    $("#btnCancelApprove").click(function () {
        $("#cmdName")[0].value = "AttributeDetail";
        $('#viewPlaceHolder').empty();


    });
    $("#btnCancelReject").click(function () {
        $("#cmdName")[0].value = "AttributeDetail";
        $('#viewPlaceHolder').empty();


    });


    $("#chkSelectAll").change(function () {
        var status = this.checked; // "select all" checked status
        $('.checkbox').each(function () { //iterate all listed checkbox items
            this.checked = status; //change ".checkbox" checked status
        });
    });

    $("#chkSelectAllRejected").change(function () {
        var status = this.checked; // "select all" checked status
        $('.checkboxRejected').each(function () { //iterate all listed checkbox items
            this.checked = status; //change ".checkbox" checked status
        });
    });
    $("#chkSelectAlApprovalPending").change(function () {
        var status = this.checked; // "select all" checked status
        $('.checkboxApprovalPending').each(function () { //iterate all listed checkbox items
            this.checked = status; //change ".checkbox" checked status
        });
    });


});

function onBegin() {

    $('#divLoading').show();

}
function onSuccess(result) {
    debugger;
    var r = result;
    $('#lblcaption').val = '@Session["EntityName"]';
    var fileId = $('#cmdName').val();
    $('#AttributeDetail').empty();

    $('#lblcaption').html('@Session["EntityName"]');
    $('#viewPlaceHolder').empty();
    $('#' + fileId).html(result);


}
function onFailure(error) {
    alert(error);
}
function onComplete() {
    $('#divLoading').hide();
}


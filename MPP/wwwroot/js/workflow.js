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
    $('.checkbox').change(function () {
        if (this.checked === false) {
            $('#chkSelectAll')[0].checked = false;
        } else {
            // Check if all other checkboxes are checked
            if ($('.checkbox:checked').length === $('.checkbox').length) {
                $('#chkSelectAll')[0].checked = true;
            }
        }
    });
    $("#chkSelectAllRejected").change(function () {
        var status = this.checked; // "select all" checked status
        $('.checkboxRejected').each(function () { //iterate all listed checkbox items
            this.checked = status; //change ".checkbox" checked status
        });
    });
    $('.checkboxRejected').change(function () {
        if (this.checked === false) {
            $('#chkSelectAllRejected')[0].checked = false;
        } else {
            // Check if all other checkboxes are checked
            if ($('.checkboxRejected:checked').length === $('.checkboxRejected').length) {
                $('#chkSelectAllRejected')[0].checked = true;
            }
        }
    });
    $("#chkSelectAlApprovalPending").change(function () {
        var status = this.checked; // "select all" checked status
        $('.checkboxApprovalPending').each(function () { //iterate all listed checkbox items
            this.checked = status; //change ".checkbox" checked status
        });
    });
    $('.checkboxApprovalPending').change(function () {
        if (this.checked === false) {
            $('#chkSelectAlApprovalPending')[0].checked = false;
        } else {
            // Check if all other checkboxes are checked
            if ($('.checkboxApprovalPending:checked').length === $('.checkboxApprovalPending').length) {
                $('#chkSelectAlApprovalPending')[0].checked = true;
            }
        }
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


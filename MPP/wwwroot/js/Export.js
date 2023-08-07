$(document).ready(function () {
    $("#btnCancel").click(function () {
        $('#AttributeDetail').html();
        $('#viewPlaceHolder').html();
    });
    $("#chkSelectAll").change(function () {
        debugger;
        var staus = this.checked;
        $('.label').each(function () {
            this.checked = status;
        });
    });
});

function onBegin() {
    $('#divLoading').show();
}
function onSuccess(result) {
    debugger;
    if (result.indexOf("error") == 0) {
        alert(result.replace("error", ""));
    }
    if (result.indexOf("export") == 0) {
        window.location.href = result.replace("export", "") + 'Export/Download';
    }
    else {
        $('#AttributeDetail').empty();
        $('#viewPlaceHolder').empty();
        $('#AttributeDetail').html(result);
    }
}
function onFailure(error) {
    alert(error);
}
function onComplete() {
    $('#divLoading').hide();
}
function onBegin() {
    $('#divLoading').show();
}
function onComplete() {
    $('#divLoading').hide();
}
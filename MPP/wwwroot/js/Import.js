$(document).ready(function () {
    $("chkSelectAll").change(function () {
        var status = this.checked;
        $('.label').each(function () {
            this.checked = status;
        });
    });
});
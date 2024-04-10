//Sidebar Open and close
$(document).on('click', '.sidebarToggleBtn', function () {
    $('body').toggleClass('sideOpen');
})
//Sidebar close
$(document).on('click', '.navbar-brand, #lblAdmin', function () {
    $('body').removeClass('sideOpen');
})
//Header Menu remove and add active class
$(document).on('click', '.customNavbar .navbar-nav li', function () {
    $('.customNavbar .navbar-nav li a').removeClass('active');
    $(this).find('a').addClass('active');
//    $('body').addClass('sideOpen');
});
//Sidebar Menu remove and add active class
$(document).on('click', '.mainSidebar ul li', function () {
    $('.mainSidebar ul li a').removeClass('active');
    $(this).find('a').addClass('active');
});

//Main Toolbar remove and add active class
$(document).on('click', 'btn-group', function () {
    $('button').removeClass('active');
    $(this).find('button').addClass('active');
});

// Data Table Sorting icon 
function addArrowIcons() {
    var headers = document.querySelectorAll("#tblsearchdata th");
    headers.forEach(function (header) {
        var upIcon = document.createElement("i");
        upIcon.classList.add("fas", "fa-arrow-up");
        upIcon.style.display = "none";

        var downIcon = document.createElement("i");
        downIcon.classList.add("fas", "fa-arrow-down");

        header.appendChild(upIcon);
        header.appendChild(downIcon);

        header.addEventListener("click", function () {
            toggleArrowIcons(header);
            /*sortTable(header.cellIndex);*/
        });
    });
}

function toggleArrowIcons(header) {
    var arrowIcons = header.querySelectorAll("i");
    arrowIcons.forEach(function (icon) {
        icon.style.display = icon.style.display === "none" ? "inline-block" : "none";
        icon.classList.toggle("active");
    });


}

// Add arrow icons when the page loads
window.onload = function () {
    addArrowIcons();
};
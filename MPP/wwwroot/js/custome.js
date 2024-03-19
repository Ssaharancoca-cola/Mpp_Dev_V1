//Sidebar Open and close
//$(document).on('click', '.sidebarToggleBtn', function () {
//    $('body').toggleClass('sideOpen');
//})
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
if (navigator.onLine) {
    //alert('You are online!');
} else {
    swal('Su conexión a Internet se ha desconectado!');
}

$(function () {
    sendRequest(true);

    $("#textInput").focus();

    $('.active-notification').on('click', function () {
        var msg = $(this).text();

        toastr.info(msg)
    });

    $('.work-inprogress').on('click', function () {
        swal("En desarrollo!", "Estamos trabajando en esta funcionalidad", "info")
    });

    $('[data-toggle="tooltip"]').tooltip();


    $("#selectLanguage select").change(function () {
        $(this).parent().submit();
    });

}); //End Init


function currentTime() {
    var now = new Date();
    now = now.getHours() + ':' + now.getMinutes() + ':' + now.getSeconds();

    return now;
}


function NotificationToast(type, message, title) {
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-top-center",
        "preventDuplicates": true,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };

    toastr[type](message, title);

    //switch (type) {
    //    case "success":
    //        toastr.success(message, title);
    //        break;

    //    case "error":
    //        toastr.error(message, title);
    //        break;
    //    default:
    //        toastr.info(message, title);
    //        break;
    //}
}

function validateEmail(sEmail) {
    var filter = /^[\w\-\.\+]+\@[a-zA-Z0-9\.\-]+\.[a-zA-z0-9]{2,4}$/;

    if (filter.test(sEmail)) {
        return true;
    }
    else {
        return false;
    }
}
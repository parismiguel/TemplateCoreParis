var _contextAction;
var _password;
var _userName;

$(function () {
    $(".collapse-chat").click(function () {
        $("#chatbot").toggle();
        $("#textInput").focus();
    });

    $("#scrollingChat").scroll(function () {
        $("#countChats").text(0);
    });

}); // Init



function sendRequest(init, _action, _isPayload) {
    var url = "/Api/ChatBot/";

    var msg = $("#textInput").val();
    var valid = false;

    $("#textInput").prop("disabled", true);
    $("#chat_loader").show();

    if (init) {
        msg = 'hola';
    }

    if ($("#textInput").val() === "" && init === undefined) {
        toastr.error("Debe ingresar un texto", "Error")
        resetInputChat();
        return;
    }

    if (_isPayload) {
        _contextAction = _action;
    }

    if (init !== true) {
        if (!_contextAction) {
            if (_isPayload !== true) {
                appendMessage(true, msg);
            }
        }
    }

    switch (_contextAction) {

        case "emailToValidate":
            if (validateEmail(msg)) {
                valid = true;
                appendMessage(true, msg);
            }
            else {
                NotificationToast("error", "Correo inválido", "Error");
                valid = false;
            }

            break;

        case "passwordToValidate":
            if (msg.length < 8) {
                NotificationToast("error", "La clave debe ser mínimo de 8 caracteres", "Error");

                appendMessage(false, "La clave debe ser mínimo de 8 caracteres. Intente de nuevo.");

                resetInputChat();
                return;
            }
            else {
                valid = true;
                _password = msg;
            }

            break;

        case "confirmationToValidate":
            if (_password !== msg) {
                NotificationToast("error", "La confirmación no coincide con la clave. Intente de nuevo.", "Error");

                _contextAction = "passwordToValidate";
                valid = false;
            }
            else {
                valid = true;
            }
         

            break;

        default:
            if (init !== true && _isPayload === false) {
                appendMessage(true, msg);
                valid = false;
            }

            break;
    }




    $.post(url, { msg: msg, isInit: init, isValid: valid, actionPayload: _contextAction }, function (result) {
        var obj = JSON.parse(result)
        var total = obj.output.text.length;

        _contextAction = obj.context.action;
        valid = obj.context.valid;

        for (var i = 0; i < total; i++) {
            appendMessage(false, obj.output.text[i]);
        }

        $("#countChats").text(total);

        if (_contextAction === "passwordToValidate" || _contextAction === "confirmationToValidate") {
            $("#textInput").attr("type", "password");
        }
        else {
            $("#textInput").attr("type", "text");

        }


        if (_contextAction === "success") {
            NotificationToast("success", "Clave modificada con éxito!", "Confirmación");
            valid = false;
            _contextAction = null;
        }

        if (obj.context.username !== null) {
            _userName = obj.context.username;
        }

        resetInputChat();

    })
        .done(function () {
            resetInputChat();

        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            resetInputChat();
            var _errorMsg;

            switch (jqXHR.status) {
                case 500:
                    _errorMsg = "Error en el servidor. " + errorThrown;
                    break;
                case 404:
                    _errorMsg = "No se ha encontrado el recurso";
                    break;
                default:
                    _errorMsg = "Error de conexión a Internet";
                    break;
            }

           
                NotificationToast("error", _errorMsg, "Error");
        });

}

function resetInputChat() {
    $("#textInput").prop("disabled", false);
    $("#textInput").val(null);
    $("#chat_loader").hide();

    $("#textInput").focus();

}


function resetChat() {
    _contextAction = "";
    _password = "";
    _userName = "";

    $("#scrollingChat").text(null);
    sendRequest(true, null, false);
}

function appendMessage(isUser, message) {
    var nombre = "AjeBot";
    var clase = "direct-chat-msg";
    var imagen = "/images/user1-128x128.jpg";
    var alig1 = "left";
    var alig2 = "right";

    if (isUser) {
        if (_userName !== null && _userName !== undefined) {
            nombre = _userName;
        }
        else {
            nombre = "Usuario";
        }

        clase = "direct-chat-msg right";
        imagen = "/images/user3-128x128.jpg";
        alig1 = "right";
        alig2 = "left";
    }

    var hora = currentTime();

    var element = "<div class='" + clase + "'>" +
        "<div class='direct-chat-info clearfix'>" +
        "<span class='direct-chat-name pull-" + alig1 + "'>" + nombre + "</span>" +
        "<span class='direct-chat-timestamp pull-" + alig2 + "'>" + hora + "</span>" +
        "</div>" +
        "<img class='direct-chat-img' src='" + imagen + "' alt= '" + nombre + "'>" +
        "<div class='direct-chat-text'>" + message +
        "</div>" +
        "</div>"

    $("#scrollingChat").append(element);

    $("#bodyChat").scrollTop($("#scrollingChat")[0].scrollHeight);

}

function getGoogleUserInfo(_myUserEmail) {

    if (_myUserEmail === null || _myUserEmail === undefined) {
        return;
    }

    var url = "/Home/GetGoogleUserInfoTask/";

    $.post(url, { userEmail: _myUserEmail }, function (result) {
        var obj = JSON.parse(result)
        var _text = JSON.stringify(result);

        swal({
            title: "Datos Usuario",
            text: "<div style='word-wrap: break-word;'>" + result + "</div>",
            html: true,
            type: "info"
        });

    })
        .done(function () {

        })
        .fail(function (error) {
            NotificationToast("error", error.statusText, "Error");
        });
}

function getGoogleTokens(_myUserEmail) {
    if (_myUserEmail === null || _myUserEmail === undefined) {
        return;
    }

    var url = "/Home/GetGoogleTokensTask/";

    $.post(url, { userEmail: _myUserEmail }, function (result) {
        getGoogleUserInfo(_myUserEmail);

    })
        .done(function () {

        })
        .fail(function (error) {
            NotificationToast("error", error.statusText, "Error");
        });

}
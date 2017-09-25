var _contextAction;
var _password;
var _userName;

$(function () {

    Sidebar.init();

    $(".collapse-chat").click(function () {
        $("#chatbot").toggle();
        $("#textInput").focus();
    });

    $("#scrollingChat").scroll(function () {
        $("#countChats").text(0);
    });

    $("#iconPicture").click(function () {
        $("#myImage").trigger('click');
    });

    $('#myImage').on('change', function () {
        var val = $(this).val();
        $(this).siblings('span').text(val);

        var file = $('#myImage')[0].files[0];
        var fileURL = URL.createObjectURL(file);

        appendImage(true, fileURL);
        sendImage(file);

        //var fileReader = new FileReader();
        //fileReader.onloadend = function (e) {
        //    var arrayBuffer = e.target.result;
        //    var byteArray = new Uint8Array(arrayBuffer);

        //    var myString = Decodeuint8arr(byteArray);

        //    sendImage(myString);

        //};
        //fileReader.readAsArrayBuffer(file);
        ////fileReader.readAsText(file);

        //console.log('here is a link', fileURL);


        //var fileReader = new FileReader();
        ////var arrayBuffer;

        //fileReader.onloadend = function (e) {
        //    arrayBuffer = e.target.result;
        //    var fileType = "image/png";
        //    blobUtil.arrayBufferToBlob(arrayBuffer, fileType).then(function (blob) {
        //        console.log('here is a blob', blob);
        //        console.log('its size is', blob.size);
        //        console.log('its type is', blob.type);

        //        objectURL = URL.createObjectURL(blob);

        //        sendImage(file);

        //        console.log('here is a link', objectURL);

        //    }).catch(console.log.bind(console));
        //};
        //fileReader.readAsArrayBuffer(file);

        //var byteArray = new Uint8Array(arrayBuffer);

        //sendImage(byteArray);

    })

}); // Init


function sendRequest(init, _action, _isPayload, btnMsg) {
    var url = "/ChatBot/MessageChatAsync/";

    var msg = $("#textInput").val();
    var valid = false;

    if (btnMsg !== null && btnMsg !== undefined) {
        msg = btnMsg;
    }

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
            if (init !== true && _isPayload === false && btnMsg === undefined) {
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

function sendImage(_file) {
    var url = "/Faces/MsFaceIdentifyJson/";

    var counter = 0;

    var data = new FormData();
    data.append('file', _file);


    $("#textInput").prop("disabled", true);
    $("#chat_loader").show();

    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        processData: false,
        contentType: false
    })
        .done(function (result) {
        //var obj = JSON.parse(result);

         appendMessage(false, result.text);
         counter++;

         if (result.joke !== null && result.joke !== undefined) {
             appendMessage(false, "¿Qué tal un chistecito para animar el día?");
             counter++;
             appendMessage(false, result.joke);
             counter++;
         }

         if (result.promo !== null && result.promo !== undefined) {
             appendMessage(false, "Veo que usas anteojos. ¿Te gustaria adquirir uno nuevo?");
             counter++;
             appendMessage(false, result.promo);
             counter++;
         }

         $("#countChats").text(counter);

        resetInputChat();

    })
        .fail(function (jqXHR, textStatus, errorThrown) {
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

    swal({
        title: "Está seguro?",
        text: "Desea eliminar el historial de su conversación?",
        type: "info",
        showCancelButton: true,
        confirmButtonText: "Aceptar",
        cancelButtonText: "Cancelar",
        closeOnConfirm: true,
        closeOnCancel: true
    }, function (isConfirm) {
        if (isConfirm === true) {

            localStorage.removeItem("myPayload");
            localStorage.removeItem("myResponse");

            _contextAction = "";
            _password = "";
            _userName = "";

            $("#scrollingChat").text(null);

            sendRequest(true, null, false);
        }
        else {
            return false;
        }
    });
}

function appendMessage(isUser, message) {
    var nombre = "VCAbot";
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

function appendImage(isUser, url) {
    var nombre = "VCAbot";
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
        "<div class='direct-chat-text' style='text-align:center;'>" +
        "<img class='responsive' src='" + url + "' alt='Imágen' style='height:17rem; width: 100%;'>" +
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


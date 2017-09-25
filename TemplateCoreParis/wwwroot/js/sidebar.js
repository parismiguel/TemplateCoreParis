var Sidebar = (function () {
    'use strict';

    var ids = {
        sidebar: 'sidebar',
        suggestionList: 'suggestion-list'
    };

    var suggestions = [
        'Qué es VCA?',
        'Qué es Bluemix?',
        'Qué es IBM?',
        'Dime más sobre Cloud Services',
        'Qués es IBM Cognos?',
        'Qué es IBM Analytics?',
        'Qué es IoT?'
    ];


    // Publicly accessible methods defined
    return {
        init: init,
        toggle: toggle,
        resetChat: resetChat,
        resetChat2: resetChat2,
        saveChat: saveChat
    };

    // Initialize the Sidebar module
    function init() {
        populateSuggestions();
    }

    // Populate the suggested user messages in the sidebar
    function populateSuggestions() {
        var suggestionList = document.getElementById(ids.suggestionList);
        for (var i = 0; i < suggestions.length; i++) {
            var listItemJson = {
                'tagName': 'li',
                'children': [{
                    'tagName': 'button',
                    'text': suggestions[i],
                    'classNames': ['suggestion-btn'],
                    'attributes': [{
                        'name': 'onclick',
                        'value': 'Sidebar.toggle(); sendRequest(false, null, false, "' + suggestions[i] + '")'
                    }]
                }]
            };
            suggestionList.appendChild(Common.buildDomElement(listItemJson));
        }
    }

    // Toggle whether the sidebar is displayed
    function toggle() {
        Common.toggleClass(document.getElementById(ids.sidebar), 'is-active');
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

                //$.removeCookie("myPayload");
                //$.removeCookie("myPayload", { path: "/" });

                resetChat2();
            }
            else {
                return false;
            }
        });
    }

    function resetChat2() {

                localStorage.removeItem("myPayload");
                localStorage.removeItem("myResponse");

                $("#scrollingChat").html(null);

        //Api.setResponsePayload(null); //Cannot read property output

                //Api.sendRequest(null, null);
                sendRequest(true, null, false);

    }

    function saveChat() {
        var body = $("#scrollingChat").html();

        var jsonObject = {
            Conversation: body,
            DateSaved: Date()
        };

      
        $.ajax({
            url: "/Home/SaveChat/",
            type: "POST",
            data: JSON.stringify(jsonObject),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            error: function (response) {
                NotificationToast("error", response.statusText, "Error");
            },
            success: function (response) {
                NotificationToast("success", "Conversación Grabada!", "Confirmación");
        }
    });
    }

}());

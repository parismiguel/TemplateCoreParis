function GetReniecDNI(_dni) {
    var tecactusApi = new TecactusApi("bki9O9ocOy5uRKCs00FNQzVoXv17CGXUFOKHQ69H")

    swal({
        title: "Consulta RENIEC",
        text: "Ingrese su DNI:",
        type: "input",
        showCancelButton: true,
        closeOnConfirm: false,
        animation: "slide-from-top",
        inputPlaceholder: "Número de DNI"
    },
        function (inputValue) {
            if (inputValue === false) return false;

            if (inputValue === "") {
                swal.showInputError("Es necesario que digite su DNI");
                return false
            }

            var msgTitle;
            var msgBody;

            tecactusApi.Reniec.getDni(inputValue)
                .then(function (response) {
                    console.log("consulta correcta!")
                    console.log(response.data)

                    var obj = response.data;
                    msgTitle = "Hola " + obj.nombres + "!";
                    msgBody = "Tus Apellidos según RENIEC para el DNI " +
                        obj.dni + " son: <br/ >" + obj.apellido_paterno + " " +
                        obj.apellido_materno + "<br/ >Caracter de verificación: " + obj.caracter_verificacion;

                  
                    swal({
                        title: msgTitle,
                        text: msgBody,
                        html: true,
                        type: "info"
                    });

                    //swal(JSON.stringify(response.data));
                })
                .catch(function (response) {
                    console.log("algo ocurrió")
                    console.log("código de error: " + response.code)
                    console.log("mensaje de respuesta: " + response.status)
                    console.log(response.data)
                })


        });



}


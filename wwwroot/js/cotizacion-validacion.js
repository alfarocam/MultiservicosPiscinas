$(document).ready(function () {
    // Regla personalizada para el formato de teléfono
    $.validator.addMethod('telefonoValido', function (value, element) {
        if (!value) return true;
        return /^[0-9]{4}-[0-9]{4}$/.test(value);
    }, 'El teléfono debe tener el formato 1234-5678.');

    $.validator.addMethod('emailValido', function (value, element) {
        if (!value) return true;
        // Verifica que tenga formato de usuario válido y termine en @gmail.com, @hotmail.com, @yahoo.com o @outlook.com
        return /^[a-zA-Z0-9._%+-]+@(gmail\.com|hotmail\.com|yahoo\.com|outlook\.com)$/i.test(value.trim());
    }, 'El correo debe ser @gmail.com, @hotmail.com, @yahoo.com o @outlook.com.');

    $('#formCliente').validate({
        errorClass: 'text-danger small mt-1 d-block',
        rules: {
            NombreCliente: {
                required: true,
                minlength: 3
            },
            ApellidoPaterno: {
                required: true,
                minlength: 3
            },
            ApellidoMaterno: {
                required: true,
                minlength: 3
            },
            CorreoCliente: {
                required: true,
                email: true,
                emailValido: true
            },
            TelefonoCliente: {
                required: true,
                telefonoValido: true
            }
        },
        messages: {
            NombreCliente: {
                required: 'El nombre es obligatorio.',
                minlength: 'El nombre debe tener al menos 3 caracteres.'
            },
            ApellidoPaterno: {
                required: 'El apellido paterno es obligatorio.',
                minlength: 'El apellido paterno debe tener al menos 3 caracteres.'
            },
            ApellidoMaterno: {
                required: 'El apellido materno es obligatorio.',
                minlength: 'El apellido materno debe tener al menos 3 caracteres.'
            },
            CorreoCliente: {
                required: 'El correo es obligatorio.',
                email: 'Ingrese un correo electrónico válido.',
                emailValido: 'El correo debe terminar en @gmail.com, @hotmail.com, @yahoo.com o @outlook.com.'
            },
            TelefonoCliente: {
                required: 'El teléfono es obligatorio.'
            }
        }
    });
});

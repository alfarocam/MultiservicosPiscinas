$(document).ready(function () {
    // Regla personalizada para el formato de teléfono (8 dígitos con formato 8888-8888)
    $.validator.addMethod('telefonoValido', function (value, element) {
        if (this.optional(element)) return true;
        return /^\d{4}-\d{4}$/.test($.trim(value));
    }, 'El teléfono debe tener el formato 8888-8888 (8 dígitos).');

    $.validator.addMethod('emailValido', function (value, element) {
        if (this.optional(element)) return true;
        return /^[a-zA-Z0-9._%+-]+@(gmail\.com|hotmail\.com|yahoo\.com|outlook\.com|icloud\.com|live\.com|piscinas\.com|multiservicios\.com|test\.com|correo\.com|ejemplo\.com)$/i.test(value.trim());
    }, 'Ingresá un correo válido (@gmail.com, @hotmail.com, @yahoo.com, @outlook.com o institucional).');

    $('#formCliente').validate({
        errorClass: 'text-danger small mt-1 d-block',
        errorElement: 'span',
        highlight: function (element) {
            $(element).addClass('is-invalid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid');
        },
        rules: {
            NombreCliente: {
                required: true,
                minlength: 3,
                normalizer: function (value) { return $.trim(value); }
            },
            ApellidoPaterno: {
                required: true,
                minlength: 3,
                normalizer: function (value) { return $.trim(value); }
            },
            ApellidoMaterno: {
                required: true,
                minlength: 3,
                normalizer: function (value) { return $.trim(value); }
            },
            CorreoCliente: {
                required: true,
                email: true,
                emailValido: true,
                normalizer: function (value) { return $.trim(value); }
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
                email: 'Ingrese un correo electrónico válido.'
            },
            TelefonoCliente: {
                required: 'El teléfono es obligatorio.'
            }
        },
        submitHandler: function (form) {
            if (typeof Swal === 'undefined') {
                form.submit();
                return;
            }

            Swal.fire({
                title: '¿Generar cotización?',
                text: 'Se registrará la cotización y se descargará el PDF.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#0d6efd',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, generar',
                cancelButtonText: 'Cancelar'
            }).then(function (result) {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        }
    });
});

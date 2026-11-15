$(document).ready(function () {
    var $telefono = $('#Telefono');

    // Bloquea letras/símbolos y recorta a máximo 8 dígitos (estándar Costa Rica)
    $telefono.attr('maxlength', '8').on('input', function () {
        var soloNumeros = $(this).val().replace(/\D/g, '').slice(0, 8);
        if ($(this).val() !== soloNumeros) {
            $(this).val(soloNumeros);
        }
    });

    $.validator.addMethod('telefonoCostaRica', function (value, element) {
        if (this.optional(element)) return true;
        return /^\d{1,8}$/.test(value);
    }, 'El teléfono debe contener solo números y máximo 8 dígitos.');

    $.validator.addMethod('correoValido', function (value, element) {
        if (this.optional(element)) return true;
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim());
    }, 'Ingresá un correo válido.');

    var urlVerificarDuplicado = $('#urlVerificarDuplicado').val();
    var validandoEnvio = false;

    function mostrarAlertaDuplicado(mensaje) {
        if (typeof Swal === 'undefined') {
            alert(mensaje);
            return;
        }

        Swal.fire({
            title: 'Datos ya registrados',
            text: mensaje,
            icon: 'warning',
            confirmButtonText: 'Corregir datos',
            confirmButtonColor: '#0d6efd'
        });
    }

    $('#formRegistro').validate({
        errorClass: 'text-danger small mt-1 d-block',
        rules: {
            Nombre: {
                required: true,
                normalizer: function (value) { return $.trim(value); }
            },
            ApellidoPaterno: {
                required: true,
                normalizer: function (value) { return $.trim(value); }
            },
            ApellidoMaterno: {
                required: true,
                normalizer: function (value) { return $.trim(value); }
            },
            Correo: {
                required: true,
                correoValido: true,
                normalizer: function (value) { return $.trim(value); }
            },
            Telefono: {
                required: true,
                telefonoCostaRica: true
            },
            Contrasena: {
                required: true
            },
            provinciaId: {
                required: true
            },
            cantonId: {
                required: true
            },
            DistritoId: {
                required: true
            },
            Direccion: {
                required: true,
                normalizer: function (value) { return $.trim(value); }
            }
        },
        messages: {
            Nombre: {
                required: 'El nombre es obligatorio.'
            },
            ApellidoPaterno: {
                required: 'El apellido paterno es obligatorio.'
            },
            ApellidoMaterno: {
                required: 'El apellido materno es obligatorio.'
            },
            Correo: {
                required: 'El correo es obligatorio.'
            },
            Telefono: {
                required: 'El teléfono es obligatorio.'
            },
            Contrasena: {
                required: 'La contraseña es obligatoria.'
            },
            provinciaId: {
                required: 'Debe seleccionar una provincia.'
            },
            cantonId: {
                required: 'Debe seleccionar un cantón.'
            },
            DistritoId: {
                required: 'Debe seleccionar un distrito.'
            },
            Direccion: {
                required: 'La dirección es obligatoria.'
            }
        },
        submitHandler: function (form) {
            if (validandoEnvio || !urlVerificarDuplicado) {
                form.submit();
                return;
            }

            validandoEnvio = true;
            var correo = $.trim($('#Correo').val() || '');
            var telefono = $.trim($('#Telefono').val() || '');

            $.getJSON(urlVerificarDuplicado, { correo: correo, telefono: telefono })
                .done(function (data) {
                    if (data && data.existe) {
                        validandoEnvio = false;
                        mostrarAlertaDuplicado(
                            data.mensaje || 'Ya existe una cuenta registrada con esa información.'
                        );
                    } else {
                        form.submit();
                    }
                })
                .fail(function () {
                    // Si falla la verificación AJAX, el servidor igualmente valida duplicados en POST
                    form.submit();
                });
        }
    });
});

$(document).ready(function () {
    var $form = $('#formCrearCliente, #formEditarCliente').first();
    if (!$form.length) return;

    var $telefono = $('#Telefono');
    if ($telefono.length) {
        $telefono.attr('maxlength', '9').on('input', function () {
            var val = $(this).val().replace(/\D/g, '').slice(0, 8);
            if (val.length > 4) {
                val = val.substring(0, 4) + '-' + val.substring(4, 8);
            }
            if ($(this).val() !== val) {
                $(this).val(val);
            }
        }).trigger('input');
    }

    var urlVerificarDuplicado = $('#urlVerificarDuplicado').val();
    var validandoEnvio = false;

    function mostrarAlertaClienteExistente(mensaje, urlPerfil) {
        if (typeof Swal === 'undefined') {
            alert(mensaje);
            return;
        }

        var opciones = {
            title: 'Cliente ya registrado',
            text: mensaje,
            icon: 'warning',
            confirmButtonColor: '#0d6efd'
        };

        if (urlPerfil) {
            opciones.showCancelButton = true;
            opciones.confirmButtonText = 'Ver perfil existente';
            opciones.cancelButtonText = 'Corregir datos';
            opciones.cancelButtonColor = '#6c757d';
        } else {
            opciones.confirmButtonText = 'Aceptar';
        }

        Swal.fire(opciones).then(function (result) {
            if (result.isConfirmed && urlPerfil) {
                window.location.href = urlPerfil;
            }
        });
    }

    // Respaldo si el servidor devolvió la vista tras detectar un duplicado en POST
    var mensajeServidor = $('#clienteExistenteMensaje').val();
    var urlServidor = $('#clienteExistenteUrl').val();
    if (mensajeServidor) {
        mostrarAlertaClienteExistente(mensajeServidor, urlServidor);
    }

    $.validator.addMethod('noSoloEspacios', function (value, element) {
        return this.optional(element) || $.trim(value).length > 0;
    }, 'Este campo no puede contener solo espacios.');

    $.validator.addMethod('telefonoValido', function (value, element) {
        if (this.optional(element)) return true;
        return /^\d{4}-\d{4}$/.test($.trim(value));
    }, 'El teléfono debe tener el formato 8888-8888 (8 dígitos).');

    $.validator.addMethod('correoValido', function (value, element) {
        if (this.optional(element)) return true;
        return /^[a-zA-Z0-9._%+-]+@(gmail\.com|hotmail\.com|yahoo\.com|outlook\.com|icloud\.com|live\.com|piscinas\.com|multiservicios\.com|test\.com|correo\.com|ejemplo\.com)$/i.test(value.trim());
    }, 'Ingresá un correo válido (@gmail.com, @hotmail.com, @yahoo.com, @outlook.com o institucional).');

    $form.validate({
        errorClass: 'text-danger small mt-1 d-block',
        errorElement: 'span',
        highlight: function (element) {
            $(element).addClass('is-invalid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid');
        },
        errorPlacement: function (error, element) {
            var $validationSpan = element.closest('div').find('[data-valmsg-for="' + element.attr('name') + '"]');
            if ($validationSpan.length) {
                $validationSpan.html(error);
            } else {
                error.insertAfter(element);
            }
        },
        rules: {
            Nombre: {
                required: true,
                noSoloEspacios: true,
                normalizer: function (value) { return $.trim(value); }
            },
            ApellidoPaterno: {
                required: true,
                noSoloEspacios: true,
                normalizer: function (value) { return $.trim(value); }
            },
            ApellidoMaterno: {
                required: true,
                noSoloEspacios: true,
                normalizer: function (value) { return $.trim(value); }
            },
            Correo: {
                required: true,
                email: true,
                correoValido: true,
                normalizer: function (value) { return $.trim(value); }
            },
            Telefono: {
                required: true,
                telefonoValido: true
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
                noSoloEspacios: true,
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
                required: 'El correo electrónico es obligatorio.',
                email: 'Ingrese un correo electrónico válido.'
            },
            Telefono: {
                required: 'El teléfono es obligatorio.'
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
            var excluirClienteId = $('#excluirClienteId').val() || '';

            $.getJSON(urlVerificarDuplicado, {
                correo: correo,
                telefono: telefono,
                excluirClienteId: excluirClienteId
            })
                .done(function (data) {
                    if (data && data.existe) {
                        validandoEnvio = false;
                        mostrarAlertaClienteExistente(
                            data.mensaje || 'Ya existe un cliente registrado con esa información.',
                            data.urlPerfil || ''
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

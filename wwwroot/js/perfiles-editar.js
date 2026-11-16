$(document).ready(function () {
    var $telefono = $('#telefono');

    // Bloquea letras/símbolos y recorta a máximo 8 dígitos (estándar Costa Rica)
    if ($telefono.length > 0) {
        $telefono.attr('maxlength', '8').on('input', function () {
            var soloNumeros = $(this).val().replace(/\D/g, '').slice(0, 8);
            if ($(this).val() !== soloNumeros) {
                $(this).val(soloNumeros);
            }
        });
    }

    $.validator.addMethod('telefonoCostaRica', function (value, element) {
        if (this.optional(element)) return true;
        return /^\d{1,8}$/.test(value);
    }, 'El teléfono debe contener solo números y máximo 8 dígitos.');

    $.validator.addMethod('correoValido', function (value, element) {
        if (this.optional(element)) return true;
        return /^[a-zA-Z0-9._%+-]+@(gmail\.com|hotmail\.com|yahoo\.com|outlook\.com|icloud\.com|live\.com|piscinas\.com)$/i.test(value.trim());
    }, 'Ingresá un correo válido (@gmail.com, @hotmail.com, @yahoo.com u @outlook.com).');

    $.validator.addMethod('noSoloEspacios', function (value, element) {
        return this.optional(element) || $.trim(value).length > 0;
    }, 'Este campo no puede contener solo espacios.');

    var $formEditar = $('#formEditarPerfil');
    if ($formEditar.length > 0) {
        $formEditar.validate({
            errorClass: 'text-danger small mt-1 d-block',
            errorElement: 'span',
            highlight: function (element) {
                $(element).addClass('is-invalid');
            },
            unhighlight: function (element) {
                $(element).removeClass('is-invalid');
            },
            rules: {
                nombre: {
                    required: true,
                    normalizer: function (value) { return $.trim(value); }
                },
                apellidoPaterno: {
                    required: true,
                    normalizer: function (value) { return $.trim(value); }
                },
                apellidoMaterno: {
                    required: true,
                    normalizer: function (value) { return $.trim(value); }
                },
                correo: {
                    required: true,
                    correoValido: true,
                    normalizer: function (value) { return $.trim(value); }
                },
                telefono: {
                    required: true,
                    telefonoCostaRica: true
                },
                provinciaId: {
                    required: true
                },
                cantonId: {
                    required: true
                },
                distritoId: {
                    required: true
                },
                detalles: {
                    required: true,
                    normalizer: function (value) { return $.trim(value); }
                }
            },
            messages: {
                nombre: {
                    required: 'El nombre es obligatorio.'
                },
                apellidoPaterno: {
                    required: 'El primer apellido es obligatorio.'
                },
                apellidoMaterno: {
                    required: 'El segundo apellido es obligatorio.'
                },
                correo: {
                    required: 'El correo electrónico es obligatorio.'
                },
                telefono: {
                    required: 'El teléfono es obligatorio.'
                },
                provinciaId: {
                    required: 'Debe seleccionar una provincia.'
                },
                cantonId: {
                    required: 'Debe seleccionar un cantón.'
                },
                distritoId: {
                    required: 'Debe seleccionar un distrito.'
                },
                detalles: {
                    required: 'Los detalles de la dirección son obligatorios.'
                }
            },
            submitHandler: function (form) {
                if (typeof Swal === 'undefined') {
                    form.submit();
                    return;
                }

                Swal.fire({
                    title: '¿Guardar cambios del perfil?',
                    text: 'Se actualizará tu información personal y de contacto.',
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#0d6efd',
                    cancelButtonColor: '#6c757d',
                    confirmButtonText: 'Sí, guardar cambios',
                    cancelButtonText: 'Cancelar'
                }).then(function (result) {
                    if (result.isConfirmed) {
                        form.submit();
                    }
                });
            }
        });
    }

    var $formContrasena = $('#formCambiarContrasena');
    if ($formContrasena.length > 0) {
        $formContrasena.validate({
            errorClass: 'text-danger small mt-1 d-block',
            errorElement: 'span',
            highlight: function (element) {
                $(element).addClass('is-invalid');
            },
            unhighlight: function (element) {
                $(element).removeClass('is-invalid');
            },
            rules: {
                contrasena: {
                    required: true,
                    noSoloEspacios: true
                },
                confirmarContrasena: {
                    required: true,
                    equalTo: '#contrasena'
                }
            },
            messages: {
                contrasena: {
                    required: 'La nueva contraseña es obligatoria.'
                },
                confirmarContrasena: {
                    required: 'Debes confirmar la nueva contraseña.',
                    equalTo: 'Las contraseñas no coinciden.'
                }
            },
            submitHandler: function (form) {
                if (typeof Swal === 'undefined') {
                    form.submit();
                    return;
                }

                Swal.fire({
                    title: '¿Cambiar contraseña?',
                    text: 'Se actualizará tu contraseña de acceso al sistema.',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#0d6efd',
                    cancelButtonColor: '#6c757d',
                    confirmButtonText: 'Sí, cambiar contraseña',
                    cancelButtonText: 'Cancelar'
                }).then(function (result) {
                    if (result.isConfirmed) {
                        form.submit();
                    }
                });
            }
        });
    }
});

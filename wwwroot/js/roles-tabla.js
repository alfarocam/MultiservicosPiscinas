$(document).ready(function () {
    if ($('#tablaUsuarios').length > 0) {
        $('#tablaUsuarios').DataTable({
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
            },
            pageLength: 10,
            order: [[0, 'desc']],
            columnDefs: [
                { orderable: false, targets: -1 }
            ]
        });
    }

    if ($('#tablaRoles').length > 0) {
        $('#tablaRoles').DataTable({
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
            },
            pageLength: 10,
            order: [[0, 'asc']]
        });
    }

    function confirmarAccionBoton(selector, opciones) {
        $(document).on('click', selector, function (e) {
            e.preventDefault();
            var form = $(this).closest('form')[0];
            if (!form) return;

            if (typeof Swal === 'undefined') {
                form.submit();
                return;
            }

            Swal.fire(Object.assign({
                showCancelButton: true,
                cancelButtonColor: '#6c757d',
                cancelButtonText: 'Cancelar'
            }, opciones)).then(function (result) {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        });
    }

    function confirmarEnvioSwal(form, opciones) {
        if (typeof Swal === 'undefined') {
            form.submit();
            return;
        }

        Swal.fire(Object.assign({
            showCancelButton: true,
            cancelButtonColor: '#6c757d',
            cancelButtonText: 'Cancelar'
        }, opciones)).then(function (result) {
            if (result.isConfirmed) {
                form.submit();
            }
        });
    }

    if ($.validator) {
        $.validator.addMethod('correoValido', function (value, element) {
            if (this.optional(element)) return true;
            return /^[a-zA-Z0-9._%+-]+@(gmail\.com|hotmail\.com|yahoo\.com|outlook\.com|icloud\.com|live\.com|piscinas\.com|multiservicios\.com)$/i.test(value.trim());
        }, 'Ingresá un correo válido (@gmail.com, @hotmail.com, @yahoo.com, @outlook.com o institucional).');

        $.validator.addMethod('noSoloEspacios', function (value, element) {
            return this.optional(element) || $.trim(value).length > 0;
        }, 'Este campo no puede contener solo espacios.');
    }

    function configurarValidacionUsuario(selector, opcionesSwal, reglasExtra, mensajesExtra) {
        var $form = $(selector);
        if (!$form.length) return;

        if (!$.fn.validate) {
            $form.on('submit', function (e) {
                if (!this.checkValidity()) return;
                e.preventDefault();
                confirmarEnvioSwal(this, opcionesSwal);
            });
            return;
        }

        var rules = Object.assign({
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
            }
        }, reglasExtra || {});

        var messages = Object.assign({
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
                required: 'El correo electrónico es obligatorio.'
            }
        }, mensajesExtra || {});

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
            rules: rules,
            messages: messages,
            submitHandler: function (form) {
                confirmarEnvioSwal(form, opcionesSwal);
            }
        });
    }

    confirmarAccionBoton('.btn-inactivar-usuario', {
        title: '¿Inactivar usuario?',
        text: 'El usuario no podrá iniciar sesión mientras esté inactivo.',
        icon: 'warning',
        confirmButtonColor: '#d33',
        confirmButtonText: 'Sí, inactivar'
    });

    confirmarAccionBoton('.btn-activar-usuario', {
        title: '¿Activar usuario?',
        text: 'El usuario volverá a tener acceso al sistema.',
        icon: 'question',
        confirmButtonColor: '#198754',
        confirmButtonText: 'Sí, activar'
    });

    configurarValidacionUsuario('#formEditarUsuario', {
        title: '¿Guardar cambios del usuario?',
        text: 'Se actualizarán los datos del usuario en el sistema.',
        icon: 'warning',
        confirmButtonColor: '#0d6efd',
        confirmButtonText: 'Sí, guardar'
    });

    configurarValidacionUsuario('#formCrearUsuario', {
        title: '¿Registrar nuevo colaborador?',
        text: 'Se creará la cuenta con los datos ingresados.',
        icon: 'question',
        confirmButtonColor: '#0d6efd',
        confirmButtonText: 'Sí, registrar'
    }, {
        RolId: {
            required: true
        },
        Contrasena: {
            required: true,
            noSoloEspacios: true
        }
    }, {
        RolId: {
            required: 'Debe seleccionar un rol.'
        },
        Contrasena: {
            required: 'La contraseña temporal es obligatoria.'
        }
    });
});

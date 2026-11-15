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

    function confirmarEnvioFormulario(selector, opciones) {
        $(selector).on('submit', function (e) {
            var form = this;
            if (!form.checkValidity()) {
                return;
            }

            if (typeof Swal === 'undefined') {
                return;
            }

            e.preventDefault();
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

    confirmarEnvioFormulario('#formEditarUsuario', {
        title: '¿Guardar cambios del usuario?',
        text: 'Se actualizarán los datos del usuario en el sistema.',
        icon: 'warning',
        confirmButtonColor: '#0d6efd',
        confirmButtonText: 'Sí, guardar'
    });

    confirmarEnvioFormulario('#formCrearUsuario', {
        title: '¿Registrar nuevo colaborador?',
        text: 'Se creará la cuenta con los datos ingresados.',
        icon: 'question',
        confirmButtonColor: '#0d6efd',
        confirmButtonText: 'Sí, registrar'
    });
});

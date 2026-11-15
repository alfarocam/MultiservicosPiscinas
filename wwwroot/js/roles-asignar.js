$(document).ready(function () {
    var usuarioSelect = $('#usuarioSelect');
    var rolSelect = $('#rolSelect');

    if (usuarioSelect.length > 0 && rolSelect.length > 0) {
        usuarioSelect.on('change', function () {
            var rolActualId = $(this).find(':selected').data('rolid');

            rolSelect.find('option[data-rol-option]').show();

            rolSelect.find('option[data-rol-option="' + rolActualId + '"]').hide();

            if (String(rolSelect.val()) === String(rolActualId)) {
                rolSelect.val('');
            }
        });
    }

    $('#formAsignarRol').on('submit', function (e) {
        var form = this;
        if (!form.checkValidity() || typeof Swal === 'undefined') {
            return;
        }

        e.preventDefault();
        Swal.fire({
            title: '¿Modificar el rol del usuario?',
            text: 'Esto cambiará los permisos de acceso del usuario.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#0d6efd',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Sí, modificar rol',
            cancelButtonText: 'Cancelar'
        }).then(function (result) {
            if (result.isConfirmed) {
                form.submit();
            }
        });
    });
});

$(document).ready(function () {
    var $tabla = $('#tablaCotizaciones');
    if ($tabla.length) {
        $tabla.DataTable({
            responsive: true,
            language: {
                search: 'Buscar:',
                searchPlaceholder: 'Buscar cotización...',
                lengthMenu: 'Mostrar _MENU_ registros',
                info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                infoEmpty: 'Mostrando 0 a 0 de 0 registros',
                infoFiltered: '(filtrado de _MAX_ registros totales)',
                zeroRecords: 'No se encontraron resultados',
                emptyTable: 'No hay cotizaciones registradas',
                paginate: {
                    first: 'Primero',
                    last: 'Último',
                    next: 'Siguiente',
                    previous: 'Anterior'
                }
            },
            pageLength: 10,
            order: [[2, 'desc']],
            columnDefs: [
                { orderable: false, targets: -1 }
            ]
        });
    }

    $(document).on('submit', '.js-cambiar-estado-cotizacion', function (e) {
        var form = this;
        if (form.dataset.confirmado === 'true') return true;
        e.preventDefault();

        var accion = $(form).data('accion') || 'actualizar';
        var numero = $(form).data('numero') || '';
        var texto = numero
            ? 'Se procederá a ' + accion + ' la cotización ' + numero + '.'
            : 'Se procederá a ' + accion + ' esta cotización.';

        if (typeof Swal === 'undefined') {
            if (confirm(texto)) {
                form.dataset.confirmado = 'true';
                form.submit();
            }
            return false;
        }

        Swal.fire({
            title: '¿Confirmar cambio de estado?',
            text: texto,
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#0d6efd',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Sí, continuar',
            cancelButtonText: 'Cancelar'
        }).then(function (result) {
            if (result.isConfirmed) {
                form.dataset.confirmado = 'true';
                form.submit();
            }
        });

        return false;
    });
});

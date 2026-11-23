$(document).ready(function () {
    if ($('#tablaBitacora').length > 0) {
        var direccionOrden = $('select[name="orden"]').val() === 'antiguos' ? 'asc' : 'desc';

        $('#tablaBitacora').DataTable({
            responsive: true,
            autoWidth: false,
            language: {
                search: 'Buscar:',
                searchPlaceholder: 'Buscar registro...',
                lengthMenu: 'Mostrar _MENU_ registros',
                info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                infoEmpty: 'Mostrando 0 a 0 de 0 registros',
                infoFiltered: '(filtrado de _MAX_ registros totales)',
                zeroRecords: 'No se encontraron resultados',
                emptyTable: 'No hay registros en la bitácora',
                paginate: {
                    first: 'Primero',
                    last: 'Último',
                    next: 'Siguiente',
                    previous: 'Anterior'
                }
            },
            pageLength: 10,
            order: [[0, direccionOrden]],
            columnDefs: [
                { orderable: false, targets: -1 }
            ]
        });
    }
});

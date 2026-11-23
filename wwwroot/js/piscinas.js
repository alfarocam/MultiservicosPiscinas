$(document).ready(function () {
    $('#tablaPiscinas').DataTable({
        responsive: true,
        autoWidth: false,
        language: {
            search: 'Buscar:',
            searchPlaceholder: 'Buscar piscina...',
            lengthMenu: 'Mostrar _MENU_ registros',
            info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
            infoEmpty: 'Mostrando 0 a 0 de 0 registros',
            infoFiltered: '(filtrado de _MAX_ registros totales)',
            zeroRecords: 'No se encontraron resultados',
            emptyTable: 'No hay piscinas registradas',
            paginate: {
                first: 'Primero',
                last: 'Último',
                next: 'Siguiente',
                previous: 'Anterior'
            }
        },
        pageLength: 10,
        order: [[0, 'asc']],
        columnDefs: [
            { orderable: false, targets: -1 },
            { responsivePriority: 1, targets: 0 },
            { responsivePriority: 2, targets: -1 },
            { responsivePriority: 3, targets: 2 },
            { responsivePriority: 4, targets: 4 }
        ]
    });
});

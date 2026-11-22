$(document).ready(function () {
    $(document).on('click', '.btn-eliminar', function (e) {
        e.preventDefault();
        const form = $(this).closest('form');
        
        Swal.fire({
            title: '¿Desactivar producto?',
            text: 'El producto dejará de estar disponible para la venta.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Sí, desactivar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                form.submit();
            }
        });
    });

    $(document).on('click', '.btn-activar', function (e) {
        e.preventDefault();
        const form = $(this).closest('form');
        
        Swal.fire({
            title: '¿Activar producto?',
            text: 'El producto volverá a estar disponible.',
            icon: 'info',
            showCancelButton: true,
            confirmButtonColor: '#198754',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Sí, activar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                form.submit();
            }
        });
    });

    if ($('#tablaInventario').length > 0) {
        $('#tablaInventario').DataTable({
            responsive: true,
            language: {
                search: 'Buscar:',
                searchPlaceholder: 'Buscar producto...',
                lengthMenu: 'Mostrar _MENU_ registros',
                info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                infoEmpty: 'Mostrando 0 a 0 de 0 registros',
                infoFiltered: '(filtrado de _MAX_ registros totales)',
                zeroRecords: 'No se encontraron resultados',
                emptyTable: 'No hay productos registrados',
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
                { orderable: false, targets: -1 }
            ]
        });
    }
});

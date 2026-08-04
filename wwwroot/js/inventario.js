$(document).ready(function () {
    $('.btn-eliminar').on('click', function (e) {
        e.preventDefault();
        const form = $(this).closest('form');
        
        Swal.fire({
            title: '¿Eliminar producto?',
            text: 'El producto dejará de estar disponible para la venta.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                form.submit();
            }
        });
    });

    $('.btn-activar').on('click', function (e) {
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
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
            },
            pageLength: 10,
            order: [[0, 'asc']]
        });
    }
});

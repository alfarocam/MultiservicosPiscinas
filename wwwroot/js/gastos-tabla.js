$(document).ready(function () {
    var $tablaEl = $('#tablaGastos');
    if (!$tablaEl.length) return;

    var $filtroOrigen = $('#filtroOrigen');
    var $filtroCategoria = $('#filtroCategoria');
    var $filtroEstado = $('#filtroEstado');

    if ($filtroOrigen.length || $filtroCategoria.length || $filtroEstado.length) {
        $.fn.dataTable.ext.search.push(function (settings, _data, dataIndex) {
            if (!settings.nTable || settings.nTable.id !== 'tablaGastos') {
                return true;
            }

            var rowMeta = settings.aoData && settings.aoData[dataIndex];
            var $row = rowMeta && rowMeta.nTr ? $(rowMeta.nTr) : null;
            if (!$row || !$row.length) return true;

            var origenFiltro = $filtroOrigen.val() || '';
            var categoriaFiltro = $filtroCategoria.val() || '';
            var estadoFiltro = $filtroEstado.val() || '';

            if (origenFiltro && ($row.attr('data-origen') || '') !== origenFiltro) {
                return false;
            }
            if (categoriaFiltro && ($row.attr('data-categoria') || '') !== categoriaFiltro) {
                return false;
            }
            if (estadoFiltro && ($row.attr('data-estado') || '') !== estadoFiltro) {
                return false;
            }

            return true;
        });
    }

    var tabla = $tablaEl.DataTable({
        responsive: true,
        language: {
            search: 'Buscar:',
            searchPlaceholder: 'Buscar gasto...',
            lengthMenu: 'Mostrar _MENU_ registros',
            info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
            infoEmpty: 'Mostrando 0 a 0 de 0 registros',
            infoFiltered: '(filtrado de _MAX_ registros totales)',
            zeroRecords: 'No se encontraron resultados',
            emptyTable: 'No hay gastos registrados',
            paginate: {
                first: 'Primero',
                last: 'Último',
                next: 'Siguiente',
                previous: 'Anterior'
            }
        },
        pageLength: 10,
        order: [[0, 'desc']],
        columnDefs: [
            { orderable: false, targets: -1 }
        ]
    });

    $filtroOrigen.add($filtroCategoria).add($filtroEstado).on('change', function () {
        tabla.draw();
    });

    $('#btnLimpiarFiltrosGastos').on('click', function () {
        $filtroOrigen.val('');
        $filtroCategoria.val('');
        $filtroEstado.val('');
        tabla.search('').draw();
    });
});

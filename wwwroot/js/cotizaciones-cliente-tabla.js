$(document).ready(function () {
    if ($('#tablaCotizacionesCliente').length > 0) {
        $('#tablaCotizacionesCliente').DataTable({
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
            },
            pageLength: 10,
            order: [[0, 'desc']]
        });
    }
});

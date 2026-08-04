$(document).ready(function () {
    if ($('#tablaBitacora').length > 0) {
        $('#tablaBitacora').DataTable({
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
            },
            pageLength: 10,
            order: [[0, 'desc']]
        });
    }
});

$(document).ready(function () {
    if ($('#tablaAgenda').length > 0) {
        $('#tablaAgenda').DataTable({
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
            },
            pageLength: 10,
            order: [[0, 'asc']]
        });
    }
});

$(document).ready(function () {
    // Dispara la descarga automática del PDF si se acaba de completar una compra
    const link = document.getElementById('linkDescargaAuto');
    if (link) {
        link.click();
    }

    if ($('#tablaMisCompras').length > 0) {
        $('#tablaMisCompras').DataTable({
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
            },
            pageLength: 10,
            order: [[0, 'desc']]
        });
    }
});

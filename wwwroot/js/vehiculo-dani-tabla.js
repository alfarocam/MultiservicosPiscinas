$(document).ready(function () {
    if ($('#tablaVehiculoDani').length > 0) {
        $('#tablaVehiculoDani').DataTable({
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
            },
            pageLength: 10,
            order: [[0, 'asc']]
        });
    }

    var modalEditar = document.getElementById('modalEditarVehiculo');
    if (modalEditar) {
        modalEditar.addEventListener('show.bs.modal', function (event) {
            var button = event.relatedTarget;
            document.getElementById('editId').value = button.getAttribute('data-id');
            document.getElementById('editPlaca').value = button.getAttribute('data-placa');
            document.getElementById('editMarca').value = button.getAttribute('data-marca');
            document.getElementById('editModelo').value = button.getAttribute('data-modelo') || "";
            document.getElementById('editTecnicoId').value = button.getAttribute('data-tecnicoid');
        });
    }
});

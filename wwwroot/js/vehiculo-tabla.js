$(document).ready(function () {

    ['modalNuevoVehiculo', 'modalEditarVehiculo'].forEach(function (id) {
        var modal = document.getElementById(id);
        if (modal && modal.parentElement !== document.body) {
            document.body.appendChild(modal);
        }
    });

    if ($('#tablaVehiculo').length > 0) {
        $('#tablaVehiculo').DataTable({
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

    document.querySelectorAll('#modalNuevoVehiculo form, #modalEditarVehiculo form').forEach(function (form) {
        form.addEventListener('submit', function () {
            var boton = form.querySelector('button[type="submit"]');
            if (boton && !boton.disabled) {
                boton.disabled = true;
                boton.dataset.textoOriginal = boton.innerHTML;
                boton.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Guardando...';
            }
        });
    });
});

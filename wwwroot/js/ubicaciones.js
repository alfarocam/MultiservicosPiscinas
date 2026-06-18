$(document).ready(function () {
    var urlCantones = $('#urlCantones').val();
    var urlDistritos = $('#urlDistritos').val();

    $('#provinciaSelect').on('change', function () {
        var provinciaId = $(this).val();
        var $canton = $('#cantonSelect');
        var $distrito = $('#distritoSelect');

        $canton.html('<option value="">Seleccione un cantón...</option>').prop('disabled', true);
        $distrito.html('<option value="">Seleccione un distrito...</option>').prop('disabled', true);

        if (!provinciaId) return;

        $.getJSON(urlCantones, { provinciaId: provinciaId }, function (datos) {
            $.each(datos, function (i, canton) {
                $canton.append($('<option>').val(canton.id).text(canton.nombre));
            });
            $canton.prop('disabled', false);
        });
    });

    $('#cantonSelect').on('change', function () {
        var cantonId = $(this).val();
        var $distrito = $('#distritoSelect');

        $distrito.html('<option value="">Seleccione un distrito...</option>').prop('disabled', true);

        if (!cantonId) return;

        $.getJSON(urlDistritos, { cantonId: cantonId }, function (datos) {
            $.each(datos, function (i, distrito) {
                $distrito.append($('<option>').val(distrito.id).text(distrito.nombre));
            });
            $distrito.prop('disabled', false);
        });
    });
});

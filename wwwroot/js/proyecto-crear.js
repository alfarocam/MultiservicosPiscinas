$(function () {
    var $clienteSelect = $('#clienteSelect');
    var $piscinaSelect = $('#piscinaSelect');
    var urlPiscinas    = $('#urlPiscinasPorCliente').val();

    function cargarPiscinas(clienteId) {
        $piscinaSelect
            .empty()
            .append('<option value="">Sin piscina asociada (opcional)</option>')
            .prop('disabled', true);

        if (!clienteId) return;

        $.getJSON(urlPiscinas, { clienteId: clienteId })
            .done(function (data) {
                $.each(data, function (_, p) {
                    $piscinaSelect.append($('<option>', { value: p.id, text: p.texto }));
                });
                $piscinaSelect.prop('disabled', data.length === 0);
            })
            .fail(function () {
                $piscinaSelect.prop('disabled', true);
            });
    }

    $clienteSelect.on('change', function () {
        cargarPiscinas($(this).val());
    });

    if ($clienteSelect.val()) {
        cargarPiscinas($clienteSelect.val());
    }
});

$(document).ready(function () {
    var usuarioSelect = $('#usuarioSelect');
    var rolSelect = $('#rolSelect');

    if (usuarioSelect.length === 0 || rolSelect.length === 0) {
        return;
    }

    usuarioSelect.on('change', function () {
        var rolActualId = $(this).find(':selected').data('rolid');

        rolSelect.find('option[data-rol-option]').show();

        rolSelect.find('option[data-rol-option="' + rolActualId + '"]').hide();

        if (String(rolSelect.val()) === String(rolActualId)) {
            rolSelect.val('');
        }
    });
});

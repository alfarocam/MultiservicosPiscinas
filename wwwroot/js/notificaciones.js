$(function () {
    var $badge = $('#badgeNoLeidas');
    var $lista = $('#listaNotificaciones');

    function token() {
        return $('[name="__RequestVerificationToken"]').val();
    }

    function cargarNotificaciones() {
        $.getJSON('/Notificaciones/ObtenerNoLeidas')
            .done(function (data) {
                if (data.count > 0) {
                    $badge.text(data.count).removeClass('d-none');
                } else {
                    $badge.addClass('d-none');
                }

                $lista.empty();

                if (data.notificaciones.length === 0) {
                    $lista.append('<li class="text-center text-muted small py-4">No hay notificaciones nuevas.</li>');
                    return;
                }

                $.each(data.notificaciones, function (_, n) {
                    var $item = $('<li>').addClass('notif-item px-3 py-2');
                    $item.append($('<div>').addClass('small').text(n.mensaje));

                    var $footer = $('<div>').addClass('d-flex justify-content-between align-items-center mt-1');
                    $footer.append($('<span>').addClass('text-muted').css('font-size', '0.72rem').text(n.fecha));

                    var $btn = $('<button>')
                        .addClass('btn-marcar-leida')
                        .text('Marcar como leída')
                        .on('click', function () {
                            marcarLeida(n.id);
                        });

                    $footer.append($btn);
                    $item.append($footer);
                    $lista.append($item);
                });
            });
    }

    function marcarLeida(id) {
        $.post('/Notificaciones/MarcarLeida', {
            id: id,
            __RequestVerificationToken: token()
        }).done(cargarNotificaciones);
    }

    cargarNotificaciones();
    setInterval(cargarNotificaciones, 60000);
});
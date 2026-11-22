$(document).ready(function () {
    var mensajePorDefecto = 'Por favor, seleccioná una calificación de estrellas antes de enviar tu opinión.';

    function mostrarAlertaCalificacion(mensaje) {
        var texto = mensaje || mensajePorDefecto;

        if (typeof Swal === 'undefined') {
            alert(texto);
            return;
        }

        Swal.fire({
            title: 'Calificación requerida',
            text: texto,
            icon: 'warning',
            confirmButtonText: 'Entendido',
            confirmButtonColor: '#0d6efd'
        });
    }

    var errorServidor = $.trim($('#encuestaErrorServidor').val() || '');
    if (errorServidor) {
        mostrarAlertaCalificacion(errorServidor);
    }

    var $form = $('#formEncuesta');
    if (!$form.length) return;

    $form.validate({
        errorClass: 'text-danger small mt-1 d-block',
        errorElement: 'span',
        highlight: function (element) {
            $(element).addClass('is-invalid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid');
        },
        errorPlacement: function (error, element) {
            var $validationSpan = element.closest('div').find('[data-valmsg-for="' + element.attr('name') + '"]');
            if ($validationSpan.length) {
                $validationSpan.html(error);
            } else {
                error.insertAfter(element);
            }
        },
        rules: {
            Calificacion: {
                required: true,
                range: [1, 5]
            }
        },
        messages: {
            Calificacion: {
                required: 'Debes seleccionar una calificación de estrellas para enviar la encuesta.',
                range: 'Seleccioná una calificación válida entre 1 y 5 estrellas.'
            }
        },
        invalidHandler: function () {
            mostrarAlertaCalificacion(mensajePorDefecto);
        },
        submitHandler: function (form) {
            var $comentario = $('#Comentario');
            if ($comentario.length) {
                $comentario.val($.trim($comentario.val() || ''));
            }
            form.submit();
        }
    });
});

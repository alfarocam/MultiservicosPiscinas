$(function () {
    var $form = $('#formReprogramarCita');
    if (!$form.length || !$.fn.validate) return;

    function obtenerFechaHoyLocal() {
        var ahora = new Date();
        var anio = ahora.getFullYear();
        var mes = String(ahora.getMonth() + 1).padStart(2, '0');
        var dia = String(ahora.getDate()).padStart(2, '0');
        return anio + '-' + mes + '-' + dia;
    }

    function obtenerHoraActualLocal() {
        var ahora = new Date();
        var horas = String(ahora.getHours()).padStart(2, '0');
        var minutos = String(ahora.getMinutes()).padStart(2, '0');
        return horas + ':' + minutos;
    }

    function mostrarAlertaValidacion(titulo, mensaje) {
        if (typeof Swal === 'undefined') {
            alert(mensaje);
            return;
        }

        Swal.fire({
            title: titulo,
            text: mensaje,
            icon: 'warning',
            confirmButtonText: 'Entendido',
            confirmButtonColor: '#0d6efd'
        });
    }

    if ($.validator) {
        $.validator.addMethod('fechaNoPasada', function (value, element) {
            if (this.optional(element)) return true;
            return value >= obtenerFechaHoyLocal();
        }, 'La nueva fecha no puede ser anterior a la fecha actual.');

        $.validator.addMethod('fechaHoraFutura', function (value, element) {
            if (this.optional(element)) return true;
            var fecha = $('#NuevaFecha').val();
            if (!fecha) return true;

            var hoy = obtenerFechaHoyLocal();
            if (fecha > hoy) return true;
            if (fecha < hoy) return false;

            return value > obtenerHoraActualLocal();
        }, 'La nueva fecha y hora debe ser posterior a la actual.');
    }

    var validator = $form.validate({
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
            NuevaFecha: {
                required: true,
                fechaNoPasada: true
            },
            NuevaHora: {
                required: true,
                fechaHoraFutura: true
            },
            Motivo: {
                maxlength: 500
            }
        },
        messages: {
            NuevaFecha: {
                required: 'Debe seleccionar una nueva fecha.'
            },
            NuevaHora: {
                required: 'Debe seleccionar una nueva hora.'
            },
            Motivo: {
                maxlength: 'El motivo no puede superar los 500 caracteres.'
            }
        },
        invalidHandler: function (_event, val) {
            for (var i = 0; i < val.errorList.length; i++) {
                var err = val.errorList[i];
                if (err.method === 'required' && (err.element.name === 'NuevaFecha' || err.element.name === 'NuevaHora')) {
                    mostrarAlertaValidacion(
                        'Campos requeridos',
                        'Debes seleccionar la nueva fecha y hora para reprogramar la cita.'
                    );
                    return;
                }
            }

            for (var j = 0; j < val.errorList.length; j++) {
                var errFecha = val.errorList[j];
                if (errFecha.method === 'fechaNoPasada' || errFecha.method === 'fechaHoraFutura') {
                    mostrarAlertaValidacion('Fecha u hora no válida', errFecha.message);
                    return;
                }
            }
        }
    });

    $('#NuevaFecha').on('change', function () {
        var $hora = $('#NuevaHora');
        if ($hora.val()) {
            validator.element($hora);
        }
    });
});

$(function () {
    var $clienteSelect = $('#clienteSelect');
    var $piscinaSelect = $('#piscinaSelect');
    var urlPiscinas = $('#urlPiscinasPorCliente').val();
    var piscinaInicial = $.trim($('#piscinaSeleccionadaInicial').val() || '');

    function cargarPiscinas(clienteId, seleccionarInicial) {
        $piscinaSelect
            .empty()
            .append('<option value="">Seleccione una piscina...</option>')
            .prop('disabled', true);

        if (!clienteId || !urlPiscinas) return;

        $.getJSON(urlPiscinas, { clienteId: clienteId })
            .done(function (data) {
                $.each(data, function (_, p) {
                    $piscinaSelect.append($('<option>', { value: p.id, text: p.texto }));
                });
                $piscinaSelect.prop('disabled', data.length === 0);

                if (seleccionarInicial && piscinaInicial && piscinaInicial !== '0') {
                    $piscinaSelect.val(piscinaInicial);
                    piscinaInicial = '';
                }
            })
            .fail(function () {
                $piscinaSelect.prop('disabled', true);
            });
    }

    if ($clienteSelect.length) {
        $clienteSelect.on('change', function () {
            piscinaInicial = '';
            cargarPiscinas($(this).val(), false);
        });

        if ($clienteSelect.val()) {
            cargarPiscinas($clienteSelect.val(), true);
        }
    }

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

    function mostrarAlertaFechaInvalida(mensaje) {
        if (typeof Swal === 'undefined') {
            alert(mensaje);
            return;
        }

        Swal.fire({
            title: 'Fecha no válida',
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
        }, 'La fecha seleccionada no es válida. No se puede programar una cita en una fecha que ya pasó.');

        $.validator.addMethod('fechaHoraFutura', function (value, element) {
            if (this.optional(element)) return true;
            var $form = $(element).closest('form');
            var fecha = $form.find('input[name="Fecha"]').val();
            if (!fecha) return true;

            var hoy = obtenerFechaHoyLocal();
            if (fecha > hoy) return true;
            if (fecha < hoy) return false;

            return value > obtenerHoraActualLocal();
        }, 'La fecha y hora seleccionadas deben ser posteriores a la fecha y hora actual.');
    }

    function configurarValidacionCita($form, incluirCamposCreacion) {
        if (!$form.length || !$.fn.validate) return;

        var rules = {
            TecnicoId: { required: true },
            Fecha: {
                required: true,
                fechaNoPasada: true
            },
            Hora: {
                required: true,
                fechaHoraFutura: true
            }
        };

        var messages = {
            TecnicoId: { required: 'Debe seleccionar un técnico.' },
            Fecha: { required: 'La fecha es obligatoria.' },
            Hora: { required: 'La hora es obligatoria.' }
        };

        if (incluirCamposCreacion) {
            rules.ClienteId = { required: true };
            rules.PiscinaId = { required: true };
            rules.Tipo = { required: true };

            messages.ClienteId = { required: 'Debe seleccionar un cliente.' };
            messages.PiscinaId = { required: 'Debe seleccionar una piscina.' };
            messages.Tipo = { required: 'Debe indicar el tipo de servicio.' };
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
            rules: rules,
            messages: messages,
            invalidHandler: function (_event, val) {
                for (var i = 0; i < val.errorList.length; i++) {
                    var err = val.errorList[i];
                    if (err.method === 'fechaNoPasada' || err.method === 'fechaHoraFutura') {
                        mostrarAlertaFechaInvalida(err.message);
                        break;
                    }
                }
            }
        });

        $form.find('input[name="Fecha"]').on('change', function () {
            var $hora = $form.find('input[name="Hora"]');
            if ($hora.val()) {
                validator.element($hora);
            }
        });
    }

    configurarValidacionCita($('#formCrearCita'), true);
    configurarValidacionCita($('#formEditarCita'), false);
});

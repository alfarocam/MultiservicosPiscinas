$(document).ready(function () {
    var $form = $('#formRegistrarParametro');
    if (!$form.length || !$.validator) return;

    function obtenerFechaHoyIso() {
        var hoy = new Date();
        var anio = hoy.getFullYear();
        var mes = String(hoy.getMonth() + 1).padStart(2, '0');
        var dia = String(hoy.getDate()).padStart(2, '0');
        return anio + '-' + mes + '-' + dia;
    }

    $.validator.addMethod('fechaMinimaValida', function (value, element) {
        if (this.optional(element)) return true;
        var val = $.trim(value);
        if (!/^\d{4}-\d{2}-\d{2}$/.test(val)) return false;
        return val >= '2020-01-01';
    }, 'La fecha de inspección no puede ser anterior al 01/01/2020.');

    $.validator.addMethod('fechaNoFutura', function (value, element) {
        if (this.optional(element)) return true;
        var val = $.trim(value);
        if (!/^\d{4}-\d{2}-\d{2}$/.test(val)) return false;
        return val <= obtenerFechaHoyIso();
    }, 'La fecha de inspección no puede ser posterior a la fecha actual.');

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
            var $validationSpan = element.closest('.m-form-field').find('[data-valmsg-for="' + element.attr('name') + '"]');
            if ($validationSpan.length) {
                $validationSpan.html(error);
            } else {
                error.insertAfter(element);
            }
        },
        rules: {
            FechaInspeccion: {
                required: true,
                fechaMinimaValida: true,
                fechaNoFutura: true
            },
            Ph: {
                required: true,
                number: true,
                range: [0, 14]
            },
            CloroPpm: {
                required: true,
                number: true,
                range: [0, 10]
            },
            Alcalinidad: {
                required: true,
                number: true,
                range: [0, 300]
            },
            TemperaturaC: {
                required: true,
                number: true,
                range: [0, 50]
            },
            Calcio: {
                number: true,
                range: [0, 1000]
            },
            AcidoCianurico: {
                number: true,
                range: [0, 200]
            },
            Observaciones: {
                maxlength: 1000
            }
        },
        messages: {
            FechaInspeccion: {
                required: 'La fecha de inspección es obligatoria.'
            },
            Ph: {
                required: 'El pH es obligatorio.',
                number: 'Ingresá un valor numérico válido para el pH.',
                range: 'El pH debe estar entre 0 y 14.'
            },
            CloroPpm: {
                required: 'El cloro es obligatorio.',
                number: 'Ingresá un valor numérico válido para el cloro.',
                range: 'El cloro debe estar entre 0 y 10 ppm.'
            },
            Alcalinidad: {
                required: 'La alcalinidad es obligatoria.',
                number: 'Ingresá un valor numérico válido para la alcalinidad.',
                range: 'La alcalinidad debe estar entre 0 y 300 ppm.'
            },
            TemperaturaC: {
                required: 'La temperatura es obligatoria.',
                number: 'Ingresá un valor numérico válido para la temperatura.',
                range: 'La temperatura debe estar entre 0 y 50 °C.'
            },
            Calcio: {
                number: 'Ingresá un valor numérico válido para el calcio.',
                range: 'El calcio debe estar entre 0 y 1000 ppm.'
            },
            AcidoCianurico: {
                number: 'Ingresá un valor numérico válido para el ácido cianúrico.',
                range: 'El ácido cianúrico debe estar entre 0 y 200 ppm.'
            },
            Observaciones: {
                maxlength: 'Las observaciones no pueden superar los 1000 caracteres.'
            }
        }
    });
});

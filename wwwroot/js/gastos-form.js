$(function () {
    if (!$.fn.validate) return;

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

    if ($.validator && !$.validator.methods.extensionComprobante) {
        $.validator.addMethod('extensionComprobante', function (value, element) {
            if (this.optional(element) || !value) return true;
            return /\.(jpe?g|png|pdf)$/i.test(value);
        }, 'Solo se permiten archivos JPG, PNG o PDF.');
    }

    var configuracionBase = {
        errorClass: 'text-danger small mt-1 d-block',
        errorElement: 'span',
        highlight: function (element) {
            $(element).addClass('is-invalid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid');
        },
        errorPlacement: function (error, element) {
            var nombre = element.attr('name');
            var $validationSpan = element.closest('div').find('[data-valmsg-for="' + nombre + '"]');
            if ($validationSpan.length) {
                $validationSpan.empty().append(error);
            } else {
                error.insertAfter(element);
            }
        }
    };

    function construirInvalidHandler(titulo) {
        return function (_event, validator) {
            if (!validator.errorList || !validator.errorList.length) return;
            mostrarAlertaValidacion(titulo, validator.errorList[0].message);
        };
    }

    var $formRegistrar = $('#formRegistrarGasto');
    if ($formRegistrar.length) {
        $formRegistrar.validate($.extend({}, configuracionBase, {
            rules: {
                CategoriaId: {
                    required: true
                },
                Monto: {
                    required: true,
                    normalizer: $.trim,
                    number: true,
                    min: 0.01,
                    max: 999999999.99
                },
                Fecha: {
                    required: true
                },
                CitaId: {
                    normalizer: $.trim,
                    digits: true,
                    min: 1
                },
                Descripcion: {
                    normalizer: $.trim,
                    maxlength: 500
                },
                comprobanteArchivo: {
                    extensionComprobante: true
                }
            },
            messages: {
                CategoriaId: {
                    required: 'Debe seleccionar una categoría.'
                },
                Monto: {
                    required: 'Debe ingresar el monto del gasto.',
                    number: 'Ingrese un monto numérico válido.',
                    min: 'El monto debe ser mayor a cero.',
                    max: 'El monto ingresado supera el límite permitido.'
                },
                Fecha: {
                    required: 'Debe seleccionar la fecha del gasto.'
                },
                CitaId: {
                    digits: 'El número de cita debe ser un entero positivo.',
                    min: 'El número de cita debe ser mayor a cero.'
                },
                Descripcion: {
                    maxlength: 'La descripción no puede superar los 500 caracteres.'
                }
            },
            invalidHandler: construirInvalidHandler('Datos requeridos')
        }));
    }

    var $formCorregir = $('#formCorregirGasto');
    if ($formCorregir.length) {
        $formCorregir.validate($.extend({}, configuracionBase, {
            rules: {
                CategoriaId: {
                    required: true
                },
                Monto: {
                    required: true,
                    normalizer: $.trim,
                    number: true,
                    min: 0.01,
                    max: 999999999.99
                },
                CitaId: {
                    normalizer: $.trim,
                    digits: true,
                    min: 1
                },
                Descripcion: {
                    required: true,
                    normalizer: $.trim,
                    maxlength: 500
                },
                comprobanteArchivo: {
                    extensionComprobante: true
                }
            },
            messages: {
                CategoriaId: {
                    required: 'Debe seleccionar una categoría.'
                },
                Monto: {
                    required: 'Debe ingresar el monto del gasto.',
                    number: 'Ingrese un monto numérico válido.',
                    min: 'El monto debe ser mayor a cero.',
                    max: 'El monto ingresado supera el límite permitido.'
                },
                CitaId: {
                    digits: 'El número de cita debe ser un entero positivo.',
                    min: 'El número de cita debe ser mayor a cero.'
                },
                Descripcion: {
                    required: 'Debe ingresar la descripción corregida.',
                    maxlength: 'La descripción no puede superar los 500 caracteres.'
                }
            },
            invalidHandler: construirInvalidHandler('Datos requeridos')
        }));
    }

    var $formCategoria = $('#formCrearCategoria');
    if ($formCategoria.length) {
        var valCategoria = $formCategoria.validate($.extend({}, configuracionBase, {
            rules: {
                nombreCategoria: {
                    required: true,
                    normalizer: $.trim,
                    maxlength: 100
                }
            },
            messages: {
                nombreCategoria: {
                    required: 'Debe ingresar el nombre de la categoría.',
                    maxlength: 'El nombre de la categoría no puede superar los 100 caracteres.'
                }
            },
            invalidHandler: construirInvalidHandler('Categoría requerida')
        }));

        $('#modalCrearCategoria').on('hidden.bs.modal', function () {
            valCategoria.resetForm();
            $formCategoria.find('.is-invalid').removeClass('is-invalid');
        });
    }

    $('.js-form-rechazar-gasto').each(function () {
        var $formRechazo = $(this);
        var valRechazo = $formRechazo.validate($.extend({}, configuracionBase, {
            rules: {
                motivoRechazo: {
                    required: true,
                    normalizer: $.trim,
                    maxlength: 500
                }
            },
            messages: {
                motivoRechazo: {
                    required: 'Debe escribir una justificación para rechazar el gasto.',
                    maxlength: 'La justificación no puede superar los 500 caracteres.'
                }
            },
            invalidHandler: construirInvalidHandler('Justificación requerida')
        }));

        $formRechazo.closest('.modal').on('hidden.bs.modal', function () {
            valRechazo.resetForm();
            $formRechazo.find('.is-invalid').removeClass('is-invalid');
        });
    });
});

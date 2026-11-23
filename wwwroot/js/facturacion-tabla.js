$(document).ready(function () {
    var idiomaComun = {
        search: 'Buscar:',
        lengthMenu: 'Mostrar _MENU_ registros',
        info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
        infoEmpty: 'Mostrando 0 a 0 de 0 registros',
        infoFiltered: '(filtrado de _MAX_ registros totales)',
        zeroRecords: 'No se encontraron resultados',
        paginate: {
            first: 'Primero',
            last: 'Último',
            next: 'Siguiente',
            previous: 'Anterior'
        }
    };

    var $tablaFacturas = $('#tablaFacturas');
    if ($tablaFacturas.length) {
        $tablaFacturas.DataTable({
            responsive: true,
            language: $.extend({}, idiomaComun, {
                searchPlaceholder: 'Buscar factura...',
                emptyTable: 'No hay facturas emitidas registradas'
            }),
            pageLength: 10,
            order: [[3, 'desc']],
            columnDefs: [
                { orderable: false, targets: -1 }
            ]
        });
    }

    var $tablaCotizaciones = $('#tablaCotizacionesFacturables');
    if ($tablaCotizaciones.length) {
        $tablaCotizaciones.DataTable({
            responsive: true,
            language: $.extend({}, idiomaComun, {
                searchPlaceholder: 'Buscar cotización...',
                emptyTable: 'No hay cotizaciones disponibles para facturar'
            }),
            pageLength: 10,
            order: [[2, 'desc']],
            columnDefs: [
                { orderable: false, targets: -1 }
            ]
        });
    }

    $(document).on('submit', '.js-cambiar-estado-factura', function (e) {
        var form = this;
        if (form.dataset.confirmado === 'true') return true;
        e.preventDefault();

        var accion = $(form).data('accion') || 'actualizar';
        var numero = $(form).data('numero') || '';
        var texto = numero
            ? 'Se procederá a ' + accion + ' la factura ' + numero + '.'
            : 'Se procederá a ' + accion + ' esta factura.';

        if (typeof Swal === 'undefined') {
            if (confirm(texto)) {
                form.dataset.confirmado = 'true';
                form.submit();
            }
            return false;
        }

        Swal.fire({
            title: '¿Confirmar cambio de estado?',
            text: texto,
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#0d6efd',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Sí, continuar',
            cancelButtonText: 'Cancelar'
        }).then(function (result) {
            if (result.isConfirmed) {
                form.dataset.confirmado = 'true';
                form.submit();
            }
        });

        return false;
    });

    var $formFacturar = $('#formFacturarCotizacion');
    if ($formFacturar.length && $.fn.validate) {
        if ($.validator && !$.validator.methods.extensionComprobanteFactura) {
            $.validator.addMethod('extensionComprobanteFactura', function (value, element) {
                if (this.optional(element) || !value) return true;
                return /\.(jpe?g|png)$/i.test(value);
            }, 'Solo se permiten imágenes JPG o PNG.');
        }

        $formFacturar.validate({
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
            },
            rules: {
                Comprobante: {
                    required: true,
                    extensionComprobanteFactura: true
                }
            },
            messages: {
                Comprobante: {
                    required: 'Debe seleccionar el comprobante de pago.'
                }
            },
            invalidHandler: function (_event, validator) {
                if (!validator.errorList || !validator.errorList.length) return;
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        title: 'Comprobante requerido',
                        text: validator.errorList[0].message,
                        icon: 'warning',
                        confirmButtonText: 'Entendido',
                        confirmButtonColor: '#0d6efd'
                    });
                }
            }
        });
    }
});

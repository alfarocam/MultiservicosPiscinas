$(document).ready(function () {

    ['modalNuevoVehiculo', 'modalEditarVehiculo'].forEach(function (id) {
        var modal = document.getElementById(id);
        if (modal && modal.parentElement !== document.body) {
            document.body.appendChild(modal);
        }
    });

    if ($('#tablaVehiculo').length > 0) {
        $('#tablaVehiculo').DataTable({
            responsive: true,
            autoWidth: false,
            language: {
                search: 'Buscar:',
                searchPlaceholder: 'Buscar vehículo...',
                lengthMenu: 'Mostrar _MENU_ registros',
                info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                infoEmpty: 'Mostrando 0 a 0 de 0 registros',
                infoFiltered: '(filtrado de _MAX_ registros totales)',
                zeroRecords: 'No se encontraron resultados',
                emptyTable: 'No hay vehículos registrados',
                paginate: {
                    first: 'Primero',
                    last: 'Último',
                    next: 'Siguiente',
                    previous: 'Anterior'
                }
            },
            pageLength: 10,
            order: [[0, 'asc']],
            columnDefs: [
                { orderable: false, targets: -1 },
                { responsivePriority: 1, targets: 0 },
                { responsivePriority: 2, targets: -1 },
                { responsivePriority: 3, targets: 4 }
            ]
        });
    }

    function formatearPlaca(valor) {
        var limpio = (valor || '').toUpperCase().replace(/[^A-Z0-9]/g, '');
        if (!limpio) return '';

        if (/^[A-Z]/.test(limpio)) {
            var letras = (limpio.match(/^[A-Z]{1,3}/) || [''])[0];
            var digitos = limpio.slice(letras.length).replace(/\D/g, '').slice(0, 3);
            if (letras.length === 3 && digitos.length > 0) {
                return letras + '-' + digitos;
            }
            return letras + digitos;
        }

        return limpio.replace(/\D/g, '').slice(0, 6);
    }

    $('#crearPlaca, #editPlaca').on('input', function () {
        var formateada = formatearPlaca($(this).val());
        if ($(this).val() !== formateada) {
            $(this).val(formateada);
        }
    });

    if ($.validator) {
        $.validator.addMethod('noSoloEspacios', function (value, element) {
            return this.optional(element) || $.trim(value).length > 0;
        }, 'Este campo no puede contener solo espacios.');

        $.validator.addMethod('placaValida', function (value, element) {
            if (this.optional(element)) return true;
            return /^([A-Z]{3}-\d{3}|\d{6})$/.test($.trim(value).toUpperCase());
        }, 'Formato inválido. Use ABC-123 o 123456.');

        ['#formCrearVehiculo', '#formEditarVehiculo'].forEach(function (selector) {
            var $form = $(selector);
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
                rules: {
                    Placa: {
                        required: true,
                        noSoloEspacios: true,
                        placaValida: true
                    },
                    Marca: {
                        required: true,
                        noSoloEspacios: true
                    },
                    TecnicoId: {
                        required: true
                    }
                },
                messages: {
                    Placa: {
                        required: 'La placa es obligatoria.'
                    },
                    Marca: {
                        required: 'La marca es obligatoria.'
                    },
                    TecnicoId: {
                        required: 'Debe seleccionar un técnico.'
                    }
                },
                submitHandler: function (form) {
                    var boton = form.querySelector('button[type="submit"]');
                    if (boton && !boton.disabled) {
                        boton.disabled = true;
                        boton.dataset.textoOriginal = boton.innerHTML;
                        boton.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Guardando...';
                    }
                    form.submit();
                }
            });
        });
    }

    var modalEditar = document.getElementById('modalEditarVehiculo');
    if (modalEditar) {
        modalEditar.addEventListener('show.bs.modal', function (event) {
            var button = event.relatedTarget;
            var $formEditar = $('#formEditarVehiculo');
            if ($formEditar.data('validator')) {
                $formEditar.validate().resetForm();
                $formEditar.find('.is-invalid').removeClass('is-invalid');
            }
            document.getElementById('editId').value = button.getAttribute('data-id');
            document.getElementById('editPlaca').value = button.getAttribute('data-placa');
            document.getElementById('editMarca').value = button.getAttribute('data-marca');
            document.getElementById('editModelo').value = button.getAttribute('data-modelo') || '';
            document.getElementById('editTecnicoId').value = button.getAttribute('data-tecnicoid');
        });
    }

    var modalNuevo = document.getElementById('modalNuevoVehiculo');
    if (modalNuevo) {
        modalNuevo.addEventListener('show.bs.modal', function () {
            var $formCrear = $('#formCrearVehiculo');
            if ($formCrear.length) {
                $formCrear[0].reset();
                if ($formCrear.data('validator')) {
                    $formCrear.validate().resetForm();
                    $formCrear.find('.is-invalid').removeClass('is-invalid');
                }
            }
        });
    }
});

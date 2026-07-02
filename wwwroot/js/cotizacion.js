<!-- Cotización Manual - Gestión de carrito y búsqueda de productos -->

$(document).ready(function () {
    let timerBusqueda = null;
    let timerCliente = null;

    // Búsqueda de productos con debounce
    $('#filtroProducto').on('keyup', function () {
        clearTimeout(timerBusqueda);
        const filtro = $(this).val().trim();

        timerBusqueda = setTimeout(function () {
            if (filtro.length >= 2 || filtro.length === 0) {
                buscarProductos(filtro);
            }
        }, 300);
    });

    // Búsqueda de cliente con debounce
    $('#busquedaCliente').on('blur', function () {
        const valor = $(this).val().trim();
        if (valor.length >= 3) {
            buscarCliente(valor);
        }
    });

    // Delegación para agregar al carrito
    $(document).on('click', '.btn-agregar-carrito', function (e) {
        e.preventDefault();
        const productoId = $(this).data('producto-id');
        const cantidad = 1;
        agregarAlCarrito(productoId, cantidad);
    });

    // Delegación para eliminar del carrito
    $(document).on('click', '.btn-quitar-item', function (e) {
        e.preventDefault();
        const productoId = $(this).data('producto-id');
        eliminarDelCarrito(productoId);
    });

    // Abrir carrito
    $('#btnAbrirCarrito').on('click', function () {
        obtenerCarrito();
    });

    // Validar antes de generar cotización
    $('#formCliente').on('submit', function (e) {
        const nombreCliente = $('#nombreCliente').val().trim();
        const correoCliente = $('#correoCliente').val().trim();
        const telefonoCliente = $('#telefonoCliente').val().trim();

        if (!nombreCliente || !correoCliente || !telefonoCliente) {
            e.preventDefault();
            alert('Por favor, completa todos los datos del cliente.');
            return false;
        }

        // Verificar que el carrito no esté vacío
        const carrito = obtenerCarritoLocal();
        if (carrito.length === 0) {
            e.preventDefault();
            alert('El carrito está vacío. Agrega al menos un producto.');
            return false;
        }

        return true;
    });

    function buscarProductos(filtro) {
        $.ajax({
            url: '@Url.Action("BuscarProductos", "Cotizacion")',
            type: 'GET',
            data: { filtro: filtro },
            success: function (data) {
                if (data.length === 0) {
                    $('#resultadosProductos').html('<p class="text-muted">No se encontraron productos.</p>');
                    return;
                }

                let html = '<div class="list-group">';
                $.each(data, function (i, producto) {
                    html += `
                        <div class="list-group-item">
                            <div class="d-flex w-100 justify-content-between">
                                <h6 class="mb-1">${producto.nombre}</h6>
                                <small class="text-muted">${producto.nombreCategoria}</small>
                            </div>
                            <p class="mb-1"><small>${producto.descripcion || 'Sin descripción'}</small></p>
                            <div class="d-flex justify-content-between align-items-center">
                                <span class="text-success fw-bold">₡${parseFloat(producto.precio).toFixed(2)}</span>
                                <button type="button" class="btn btn-sm btn-primary btn-agregar-carrito" data-producto-id="${producto.id}">
                                    <i class="bi bi-plus-circle"></i> Agregar
                                </button>
                            </div>
                        </div>
                    `;
                });
                html += '</div>';
                $('#resultadosProductos').html(html);
            },
            error: function () {
                $('#resultadosProductos').html('<p class="text-danger">Error al buscar productos.</p>');
            }
        });
    }

    function agregarAlCarrito(productoId, cantidad) {
        $.ajax({
            url: '@Url.Action("AgregarAlCarrito", "Cotizacion")',
            type: 'POST',
            data: {
                productoId: productoId,
                cantidad: cantidad,
                __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    mostrarNotificacion('Producto agregado al carrito.', 'success');
                    actualizarBadgeCarrito(response.totalItems);
                } else {
                    mostrarNotificacion(response.mensaje || 'Error al agregar producto.', 'warning');
                }
            },
            error: function () {
                mostrarNotificacion('Error al comunicarse con el servidor.', 'danger');
            }
        });
    }

    function eliminarDelCarrito(productoId) {
        $.ajax({
            url: '@Url.Action("EliminarDelCarrito", "Cotizacion")',
            type: 'POST',
            data: {
                productoId: productoId,
                __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    mostrarNotificacion('Producto eliminado del carrito.', 'info');
                    obtenerCarrito();
                }
            }
        });
    }

    function obtenerCarrito() {
        $.ajax({
            url: '@Url.Action("ObtenerCarrito", "Cotizacion")',
            type: 'GET',
            success: function (data) {
                if (data.cantidad === 0) {
                    $('#contenidoCarrito').html('<p class="text-muted text-center">El carrito está vacío.</p>');
                    return;
                }

                let html = `
                    <table class="table table-sm">
                        <thead class="table-light">
                            <tr>
                                <th>Descripción</th>
                                <th class="text-end">P. Unitario</th>
                                <th class="text-end">Cantidad</th>
                                <th class="text-end">Subtotal</th>
                                <th class="text-end">IVA</th>
                                <th class="text-end">Total</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                `;

                $.each(data.items, function (i, item) {
                    html += `
                        <tr>
                            <td>
                                <strong>${item.nombre}</strong>
                                <br><small class="text-muted">${item.descripcion || 'Sin descripción'}</small>
                            </td>
                            <td class="text-end">₡${parseFloat(item.precioUnitario).toFixed(2)}</td>
                            <td class="text-end">${parseFloat(item.cantidad).toFixed(2)}</td>
                            <td class="text-end">₡${parseFloat(item.lineaSubtotal).toFixed(2)}</td>
                            <td class="text-end">₡${parseFloat(item.lineaImpuesto).toFixed(2)}</td>
                            <td class="text-end fw-bold">₡${parseFloat(item.lineaTotal).toFixed(2)}</td>
                            <td>
                                <button type="button" class="btn btn-sm btn-danger btn-quitar-item" data-producto-id="${item.productoId}">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </td>
                        </tr>
                    `;
                });

                html += `
                        </tbody>
                        <tfoot>
                            <tr class="table-light">
                                <th colspan="3" class="text-end">Subtotal:</th>
                                <th class="text-end">₡${parseFloat(data.subtotal).toFixed(2)}</th>
                                <th class="text-end">₡${parseFloat(data.impuestoTotal).toFixed(2)}</th>
                                <th class="text-end fw-bold">₡${parseFloat(data.total).toFixed(2)}</th>
                                <th></th>
                            </tr>
                        </tfoot>
                    </table>
                `;

                $('#contenidoCarrito').html(html);
            }
        });
    }

    function buscarCliente(valor) {
        $.ajax({
            url: '@Url.Action("BuscarCliente", "Cotizacion")',
            type: 'GET',
            data: { valor: valor },
            success: function (response) {
                if (response.encontrado) {
                    $('#nombreCliente').val(response.nombreCompleto);
                    $('#correoCliente').val(response.correo);
                    $('#telefonoCliente').val(response.telefono || '');
                    mostrarNotificacion('Cliente encontrado.', 'success');
                } else {
                    $('#nombreCliente').val('');
                    $('#correoCliente').val('');
                    $('#telefonoCliente').val('');
                }
            }
        });
    }

    function obtenerCarritoLocal() {
        // Obtener datos del formulario (simulado para validación)
        return JSON.parse(sessionStorage.getItem('carritos') || '[]');
    }

    function actualizarBadgeCarrito(cantidad) {
        if (cantidad > 0) {
            $('#badgeCarrito').text(cantidad).show();
        } else {
            $('#badgeCarrito').hide();
        }
    }

    function mostrarNotificacion(mensaje, tipo) {
        const alert = $(`
            <div class="alert alert-${tipo} alert-dismissible fade show" role="alert" style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
                ${mensaje}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `);
        $('body').append(alert);
        setTimeout(function () {
            alert.alert('close');
        }, 4000);
    }
});

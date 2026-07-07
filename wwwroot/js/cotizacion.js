<!-- Cotización Manual - Gestión de carrito, categorías/productos y búsqueda de clientes -->

$(document).ready(function () {
    let timerCliente = null;

    cargarCategorias();

    // Cambio de categoría -> cargar productos en cards
    $('#categoriaProducto').on('change', function () {
        const categoriaId = $(this).val();
        if (categoriaId) {
            cargarProductosPorCategoria(categoriaId);
        } else {
            $('#productosGrid').html('<p class="text-muted">Selecciona una categoría para ver los productos disponibles.</p>');
        }
    });

    // Búsqueda de cliente (combobox con texto libre) con debounce
    $('#busquedaCliente').on('keyup', function () {
        clearTimeout(timerCliente);
        const filtro = $(this).val().trim();

        if (filtro.length < 2) {
            $('#resultadosClientes').hide().empty();
            return;
        }

        timerCliente = setTimeout(function () {
            buscarClientes(filtro);
        }, 300);
    });

    // Cerrar dropdown de clientes al hacer clic fuera
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#busquedaCliente, #resultadosClientes').length) {
            $('#resultadosClientes').hide();
        }
    });

    // Delegación para agregar al carrito (respeta la cantidad indicada en la card)
    $(document).on('click', '.btn-agregar-carrito', function (e) {
        e.preventDefault();
        const productoId = $(this).data('producto-id');
        const cantidad = parseFloat($(this).closest('.producto-card').find('.cantidad-producto').val()) || 1;
        agregarAlCarrito(productoId, cantidad);
    });

    // Delegación para eliminar del carrito
    $(document).on('click', '.btn-quitar-item', function (e) {
        e.preventDefault();
        const productoId = $(this).data('producto-id');
        eliminarDelCarrito(productoId);
    });

    // Selección de cliente desde el dropdown
    $(document).on('click', '.item-cliente', function (e) {
        e.preventDefault();
        $('#nombreCliente').val($(this).data('nombre'));
        $('#correoCliente').val($(this).data('correo'));
        $('#telefonoCliente').val($(this).data('telefono') || '');
        $('#busquedaCliente').val($(this).data('nombre'));
        $('#resultadosClientes').hide().empty();
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

        return true;
    });

    function cargarCategorias() {
        $.ajax({
            url: '/Cotizacion/ObtenerCategorias',
            type: 'GET',
            success: function (data) {
                const select = $('#categoriaProducto');
                $.each(data, function (i, categoria) {
                    select.append(`<option value="${categoria.id}">${categoria.nombreCategoria}</option>`);
                });
            },
            error: function () {
                mostrarNotificacion('Error al cargar las categorías.', 'danger');
            }
        });
    }

    function cargarProductosPorCategoria(categoriaId) {
        $('#productosGrid').html('<p class="text-muted">Cargando productos...</p>');

        $.ajax({
            url: '/Cotizacion/ObtenerProductosPorCategoria',
            type: 'GET',
            data: { categoriaId: categoriaId },
            success: function (data) {
                if (data.length === 0) {
                    $('#productosGrid').html('<p class="text-muted">No hay productos disponibles en esta categoría.</p>');
                    return;
                }

                let html = '';
                $.each(data, function (i, producto) {
                    html += `
                        <div class="col-md-6">
                            <div class="card h-100 producto-card">
                                <div class="card-body d-flex flex-column">
                                    <h6 class="card-title">${producto.nombre}</h6>
                                    <p class="card-text text-muted small flex-grow-1">${producto.descripcion || 'Sin descripción'}</p>
                                    <p class="text-success fw-bold mb-2">₡${parseFloat(producto.precio).toFixed(2)}</p>
                                    <div class="d-flex gap-2 align-items-center">
                                        <input type="number" class="form-control form-control-sm cantidad-producto" value="1" min="1" style="width: 80px;" />
                                        <button type="button" class="btn btn-sm btn-primary flex-grow-1 btn-agregar-carrito" data-producto-id="${producto.id}">
                                            <i class="bi bi-plus-circle"></i> Agregar
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                });
                $('#productosGrid').html(html);
            },
            error: function () {
                $('#productosGrid').html('<p class="text-danger">Error al cargar productos.</p>');
            }
        });
    }

    function agregarAlCarrito(productoId, cantidad) {
        $.ajax({
            url: '/Cotizacion/AgregarAlCarrito',
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
            url: '/Cotizacion/EliminarDelCarrito',
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
            url: '/Cotizacion/ObtenerCarrito',
            type: 'GET',
            success: function (data) {
                actualizarBadgeCarrito(data.cantidad);

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

    function buscarClientes(filtro) {
        $.ajax({
            url: '/Cotizacion/BuscarClientes',
            type: 'GET',
            data: { filtro: filtro },
            success: function (data) {
                if (data.length === 0) {
                    $('#resultadosClientes').html('<div class="list-group-item text-muted">Sin coincidencias. Completa los campos para registrar uno nuevo.</div>').show();
                    return;
                }

                let html = '';
                $.each(data, function (i, cliente) {
                    html += `
                        <a href="#" class="list-group-item list-group-item-action item-cliente"
                           data-nombre="${cliente.nombreCompleto}"
                           data-correo="${cliente.correo}"
                           data-telefono="${cliente.telefono || ''}">
                            <strong>${cliente.nombreCompleto}</strong>
                            <br><small class="text-muted">${cliente.correo} ${cliente.telefono ? '- ' + cliente.telefono : ''}</small>
                        </a>
                    `;
                });
                $('#resultadosClientes').html(html).show();
            },
            error: function () {
                $('#resultadosClientes').hide();
            }
        });
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

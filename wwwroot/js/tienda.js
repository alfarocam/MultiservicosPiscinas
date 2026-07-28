$(document).ready(function () {
    cargarCategorias();
    cargarRecomendaciones();
    cargarProductos();
    actualizarCarrito();

    $('#categoria').on('change', function () {
        const categoriaId = $(this).val();
        cargarProductos(categoriaId);
    });
});

function cargarCategorias() {
    $.ajax({
        url: '/Tienda/ObtenerCategorias',
        type: 'GET',
        success: function (data) {
            const select = $('#categoria');
            data.forEach(function (cat) {
                select.append(`<option value="${cat.id}">${cat.nombreCategoria}</option>`);
            });
        }
    });
}

function cargarProductos(categoriaId) {
    const url = categoriaId ? '/Tienda/ObtenerProductosPorCategoria' : '/Tienda/ObtenerProductos';

    $.ajax({
        url: url,
        type: 'GET',
        data: categoriaId ? { categoriaId: categoriaId } : {},
        success: function (data) {
            let html = '';
            data.forEach(function (prod) {
                const btnDisabled = prod.stock === 0 ? 'disabled' : '';
                const btnText = prod.stock === 0 ? 'Sin Stock' : 'Agregar al Carrito';
                html += `
                    <div class="col-md-6 col-lg-4 mb-4">
                        <div class="card h-100">
                            <div class="card-body">
                                <h5 class="card-title">${prod.nombre}</h5>
                                <p class="card-text text-muted">${prod.descripcion || 'Sin descripción'}</p>
                                <p class="card-text"><strong>Precio:</strong> ₡${prod.precio.toFixed(2)}</p>
                                <p class="card-text"><small class="text-muted">Stock: ${prod.stock}</small></p>
                                <div class="input-group mb-3">
                                    <input type="number" class="form-control" id="qty-${prod.id}" min="1" value="1" style="max-width: 80px;">
                                    <button class="btn btn-primary" type="button" onclick="agregarAlCarrito(${prod.id})" ${btnDisabled}>
                                        ${btnText}
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
            });
            $('#productosContainer').html(html);
        }
    });
}

function cargarRecomendaciones() {
    $.ajax({
        url: '/Tienda/ObtenerRecomendaciones',
        type: 'GET',
        data: { limite: 3 },
        success: function (data) {
            if (data && data.length > 0) {
                let html = '<div class="col-12 mb-3"><h4 class="text-primary"><i class="bi bi-star-fill text-warning"></i> Recomendados para ti</h4></div>';
                data.forEach(function (prod) {
                    const btnDisabled = prod.stock === 0 ? 'disabled' : '';
                    const btnText = prod.stock === 0 ? 'Sin Stock' : 'Agregar al Carrito';
                    html += `
                        <div class="col-md-6 col-lg-4 mb-4">
                            <div class="card h-100 border-warning shadow-sm">
                                <div class="card-header bg-warning text-dark fw-bold text-center py-1">
                                    <small><i class="bi bi-stars"></i> Recomendación</small>
                                </div>
                                <div class="card-body">
                                    <h5 class="card-title">${prod.nombre}</h5>
                                    <p class="card-text text-muted">${prod.descripcion || 'Sin descripción'}</p>
                                    <p class="card-text"><strong>Precio:</strong> ₡${prod.precio.toFixed(2)}</p>
                                    <p class="card-text"><small class="text-muted">Stock: ${prod.stock}</small></p>
                                    <div class="input-group mb-3">
                                        <input type="number" class="form-control" id="qty-${prod.id}" min="1" value="1" style="max-width: 80px;">
                                        <button class="btn btn-primary" type="button" onclick="agregarAlCarrito(${prod.id})" ${btnDisabled}>
                                            ${btnText}
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                });
                $('#recomendacionesContainer').html(html);
            } else {
                $('#recomendacionesContainer').empty();
            }
        }
    });
}

function agregarAlCarrito(productoId) {
    const cantidad = parseInt($(`#qty-${productoId}`).val());

    $.ajax({
        url: '/Tienda/AgregarAlCarrito',
        type: 'POST',
        data: {
            productoId: productoId,
            cantidad: cantidad,
            __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            if (result.success) {
                mostrarNotificacion(result.mensaje, 'success');
                actualizarCarrito();
                $(`#qty-${productoId}`).val(1);
            } else {
                mostrarNotificacion(result.mensaje, 'warning');
            }
        },
        error: function () {
            mostrarNotificacion('Error al procesar la solicitud', 'error');
        }
    });
}

function eliminarDelCarrito(productoId) {
    $.ajax({
        url: '/Tienda/EliminarDelCarrito',
        type: 'POST',
        data: {
            productoId: productoId,
            __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            if (result.success) {
                location.reload();
            }
        }
    });
}

function actualizarCarrito() {
    $.ajax({
        url: '/Tienda/ObtenerCarrito',
        type: 'GET',
        success: function (result) {
            $('#badgeCarrito').text(result.cantidad);

            const contenedor = $('#listaCarritoOffcanvas');
            if (contenedor.length === 0) return;

            if (result.items.length === 0) {
                contenedor.html('<p class="text-muted text-center mb-0">Tu carrito está vacío.</p>');
                $('#offcanvasResumen').addClass('d-none');
                return;
            }

            let html = '';
            result.items.forEach(function (item) {
                html += `
                    <div class="d-flex justify-content-between align-items-center border-bottom py-2">
                        <div>
                            <strong>${item.nombre}</strong><br/>
                            <small class="a-text-muted">${item.cantidad} x ₡${item.precioUnitario.toFixed(2)}</small>
                        </div>
                        <div class="text-end d-flex align-items-center gap-2">
                            <strong>₡${item.lineaTotal.toFixed(2)}</strong>
                            <button type="button" class="btn btn-sm btn-danger" onclick="eliminarDelCarrito(${item.productoId})">
                                <i class="bi bi-trash"></i>
                            </button>
                        </div>
                    </div>
                `;
            });
            contenedor.html(html);
            $('#offcanvasTotal').text('₡' + result.total.toFixed(2));
            $('#offcanvasResumen').removeClass('d-none');
        }
    });
}

function mostrarNotificacion(mensaje, tipo) {
    const alertClass = tipo === 'success' ? 'alert-success' : tipo === 'warning' ? 'alert-warning' : 'alert-danger';
    const alerta = `<div class="alert ${alertClass} alert-dismissible fade show" role="alert">
        ${mensaje}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>`;

    $('main').prepend(alerta);

    setTimeout(function () {
        $('.alert').not(':has(button)').fadeOut();
    }, 5000);
}

$(document).ready(function () {
    cargarCategorias();
    actualizarBadgeCarrito();

    $('#categoria').on('change', function () {
        const categoriaId = $(this).val();
        if (categoriaId) {
            cargarProductos(categoriaId);
        } else {
            $('#productosContainer').empty();
        }
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
    $.ajax({
        url: '/Tienda/ObtenerProductosPorCategoria',
        type: 'GET',
        data: { categoriaId: categoriaId },
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
                actualizarBadgeCarrito();
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

function actualizarBadgeCarrito() {
    $.ajax({
        url: '/Tienda/ObtenerCarrito',
        type: 'GET',
        success: function (result) {
            $('#badgeCarrito').text(result.cantidad);
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

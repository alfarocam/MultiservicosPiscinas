document.addEventListener('DOMContentLoaded', function () {
    var contenedor = document.getElementById('correoConfirmacionData');
    if (!contenedor) return;

    var mensaje = contenedor.getAttribute('data-mensaje');
    if (!mensaje) return;

    var tipo = (contenedor.getAttribute('data-tipo') || 'success').toLowerCase();
    var icono = tipo === 'success' ? 'success' : (tipo === 'warning' ? 'warning' : 'info');
    var titulo = tipo === 'success' ? 'Correo enviado' : 'Aviso de envío de correo';

    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: titulo,
            text: mensaje,
            icon: icono,
            confirmButtonColor: '#0a9396',
            confirmButtonText: 'Entendido'
        });
    }
});

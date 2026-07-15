$(document).ready(function () {
    //Agregar reglas personalizadas de validación
    $.validator.addMethod('extensionValida', function (value, element) {
        if (!value) return true; // El campo es requerido, lo valida [Required]
        const ext = value.split('.').pop().toLowerCase();
        return ['jpg', 'jpeg', 'png'].includes(ext);
    }, 'Solo se permiten imágenes JPG, JPEG o PNG.');

    $.validator.addMethod('tamanioValido', function (value, element) {
        if (!element.files || element.files.length === 0) return true;
        const maxSize = 5 * 1024 * 1024; // 5MB en bytes
        return element.files[0].size <= maxSize;
    }, 'El archivo no debe superar 5MB.');

    $('#formComprar').validate({
        errorClass: 'text-danger',
        rules: {
            Comprobante: {
                required: true,
                extensionValida: true,
                tamanioValido: true
            }
        },
        messages: {
            Comprobante: {
                required: 'Debe subir el comprobante de pago.'
            }
        }
    });
});

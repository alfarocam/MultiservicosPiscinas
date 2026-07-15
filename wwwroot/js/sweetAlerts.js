
function FinalizarServicio(event, form) {
    event.preventDefault(); // Detiene el submit inmediato

    Swal.fire({
        title: '¿Finalizar servicio?',
        text: 'El servicio quedará marcado como finalizado y no podrá reabrirse.',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sí, finalizar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#0d6efd'
    }).then((result) => {
        if (result.isConfirmed) {
            form.submit(); // Recién aquí se envía de verdad
        }
    });

    return false;
}
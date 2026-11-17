function FinalizarServicio(event, form) {
    event.preventDefault(); // Detiene el submit inmediato

    var tieneTareas = form.getAttribute('data-tiene-tareas');
    if (tieneTareas === 'false') {
        var mensajeSinTareas = 'No se puede finalizar el servicio porque no tiene tareas registradas. Debe detallar primero el trabajo realizado.';

        if (typeof Swal === 'undefined') {
            alert(mensajeSinTareas);
            return false;
        }

        Swal.fire({
            title: 'Trabajo realizado requerido',
            text: mensajeSinTareas,
            icon: 'warning',
            confirmButtonText: 'Entendido',
            confirmButtonColor: '#0d6efd'
        });

        return false;
    }

    if (typeof Swal === 'undefined') {
        if (confirm('¿Seguro que desea dar por finalizado este servicio?')) {
            form.submit();
        }
        return false;
    }

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

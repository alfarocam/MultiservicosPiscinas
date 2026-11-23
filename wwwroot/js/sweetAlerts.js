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

function confirmarFormularioConSwal(event, form, defaults) {
    event.preventDefault();

    var titulo = form.getAttribute('data-titulo') || defaults.titulo;
    var texto = form.getAttribute('data-texto') || defaults.texto;
    var confirmText = form.getAttribute('data-confirm') || defaults.confirmText;
    var confirmColor = form.getAttribute('data-color') || defaults.confirmColor;
    var icono = form.getAttribute('data-icon') || defaults.icon;

    if (typeof Swal === 'undefined') {
        if (confirm(texto)) {
            form.submit();
        }
        return false;
    }

    Swal.fire({
        title: titulo,
        text: texto,
        icon: icono,
        showCancelButton: true,
        confirmButtonText: confirmText,
        cancelButtonText: 'Cancelar',
        confirmButtonColor: confirmColor,
        cancelButtonColor: '#6c757d'
    }).then((result) => {
        if (result.isConfirmed) {
            form.submit();
        }
    });

    return false;
}

function ConfirmarEliminacion(event, form) {
    return confirmarFormularioConSwal(event, form, {
        titulo: '¿Confirmar eliminación?',
        texto: 'Esta acción no se puede deshacer.',
        confirmText: 'Sí, eliminar',
        confirmColor: '#dc3545',
        icon: 'warning'
    });
}

function ConfirmarAccion(event, form) {
    return confirmarFormularioConSwal(event, form, {
        titulo: '¿Confirmar acción?',
        texto: '¿Desea continuar con esta acción?',
        confirmText: 'Sí, continuar',
        confirmColor: '#0d6efd',
        icon: 'question'
    });
}

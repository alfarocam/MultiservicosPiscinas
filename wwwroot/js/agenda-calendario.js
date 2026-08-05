document.addEventListener('DOMContentLoaded', function () {
    var contenedor = document.getElementById('calendarioAgenda');
    var datosScript = document.getElementById('datosAgendaCalendario');

    if (!contenedor || !datosScript || typeof FullCalendar === 'undefined') {
        return;
    }

    // Las citas ya vienen armadas desde el servidor (Views/Agenda/Index.cshtml),
    // a partir de los mismos datos que carga AgendaController.Index() para la
    // tabla. Acá solo se leen, no se inventa ninguna cita.
    var citas = JSON.parse(datosScript.textContent || '[]');

    // Mismos colores que ya usan los badges de la tabla (clases c-badge-*), para
    // que la vista de calendario y la vista de tabla se vean consistentes.
    var coloresPorEstado = {
        'Pendiente': '#64748b',
        'Confirmada': '#2563eb',
        'En camino': '#2563eb',
        'En progreso': '#d97706',
        'Completada': '#059669',
        'Cancelada': '#dc2626'
    };

    var eventos = citas.map(function (cita) {
        var color = coloresPorEstado[cita.estado] || '#64748b';
        return {
            id: cita.id,
            title: cita.title,
            start: cita.start,
            url: cita.url,
            backgroundColor: color,
            borderColor: color,
            extendedProps: {
                estado: cita.estado,
                tecnico: cita.tecnico
            }
        };
    });

    var calendario = new FullCalendar.Calendar(contenedor, {
        locale: 'es',
        initialView: 'dayGridMonth',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        // Los nombres de días/meses ya salen en español porque FullCalendar los
        // formatea con el Intl del navegador según el "locale" configurado, pero
        // el texto de los botones ("today", "month", "week", "day") depende de que
        // el plugin de idioma se haya registrado a tiempo, y no siempre pasa. Para
        // no depender de ese orden de carga, se define acá explícitamente.
        buttonText: {
            today: 'Hoy',
            month: 'Mes',
            week: 'Semana',
            day: 'Día'
        },
        height: 'auto',
        events: eventos,
        eventDidMount: function (info) {
            // Tooltip simple con el técnico asignado, para no tener que abrir la
            // cita solo para ver quién la atiende.
            info.el.title = info.event.title + ' — Técnico: ' + info.event.extendedProps.tecnico;
        }
    });

    // El calendario recién se puede dibujar cuando su contenedor está visible;
    // como arranca dentro de una pestaña oculta (Bootstrap tabs), lo renderizamos
    // cuando el usuario hace clic en "Vista de Calendario" por primera vez.
    var tabCalendario = document.querySelector('a[href="#tabCalendario"]');
    var yaRenderizado = false;

    if (tabCalendario) {
        tabCalendario.addEventListener('shown.bs.tab', function () {
            if (!yaRenderizado) {
                calendario.render();
                yaRenderizado = true;
            } else {
                calendario.updateSize();
            }
        });
    } else {
        calendario.render();
    }
});
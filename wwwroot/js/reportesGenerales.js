document.addEventListener("DOMContentLoaded", function () {
    const tipoReporte = document.getElementById("tipoReporte");
    const filtrosFechas = document.getElementById("filtrosFechas");
    const filtrosAnio = document.getElementById("filtrosAnio");
    const filtroTecnicoContainer = document.getElementById("filtroTecnicoContainer");
    const estadoGeneral = document.getElementById("estadoGeneral");
    const contenedorTabla = document.getElementById("contenedorTabla");

    function actualizarFiltros() {
        const tipo = tipoReporte.value;
        
        if (tipo === "rentabilidad") {
            filtrosFechas.classList.add("d-none");
            filtrosAnio.classList.remove("d-none");
        } else {
            filtrosFechas.classList.remove("d-none");
            filtrosAnio.classList.add("d-none");
            
            if (tipo === "servicios") {
                filtroTecnicoContainer.classList.remove("d-none");
                estadoGeneral.innerHTML = `
                    <option value="">Todos</option>
                    <option value="Pendiente">Pendiente</option>
                    <option value="En progreso">En progreso</option>
                    <option value="Completado">Completado</option>
                    <option value="Cancelado">Cancelado</option>
                `;
            } else if (tipo === "proyectos") {
                filtroTecnicoContainer.classList.add("d-none");
                estadoGeneral.innerHTML = `
                    <option value="">Todos</option>
                    <option value="Planificación">Planificación</option>
                    <option value="En Curso">En Curso</option>
                    <option value="Finalizado">Finalizado</option>
                `;
            }
        }
        contenedorTabla.innerHTML = '<p class="text-muted text-center py-4">Seleccione los filtros y presione "Generar Reporte" para ver los resultados.</p>';
    }

    tipoReporte.addEventListener("change", actualizarFiltros);
    actualizarFiltros(); // inicializar

    document.getElementById("btnGenerar").addEventListener("click", function () {
        const tipo = tipoReporte.value;
        let url = "";
        let params = new URLSearchParams();

        if (tipo === "rentabilidad") {
            url = window.reportesUrls.obtenerRentabilidad;
            params.append("anio", document.getElementById("anioRentabilidad").value);
        } else {
            if (tipo === "servicios") {
                url = window.reportesUrls.obtenerServicios;
                const tec = document.getElementById("tecnicoId").value;
                if (tec) params.append("tecnicoId", tec);
            } else if (tipo === "proyectos") {
                url = window.reportesUrls.obtenerProyectos;
            }
            
            const fd = document.getElementById("fechaDesde").value;
            const fh = document.getElementById("fechaHasta").value;
            const est = document.getElementById("estadoGeneral").value;

            if (fd) params.append("fechaDesde", fd);
            if (fh) params.append("fechaHasta", fh);
            if (est) params.append("estado", est);
        }

        contenedorTabla.innerHTML = '<div class="text-center py-4"><div class="spinner-border text-primary" role="status"></div><p class="mt-2">Cargando datos...</p></div>';

        fetch(`${url}?${params.toString()}`)
            .then(response => response.text())
            .then(html => {
                contenedorTabla.innerHTML = html;
            })
            .catch(error => {
                contenedorTabla.innerHTML = '<p class="text-danger text-center py-4">Error al cargar el reporte.</p>';
            });
    });

    document.getElementById("btnExportar").addEventListener("click", function () {
        const tipo = tipoReporte.value;
        let url = "";
        let params = new URLSearchParams();

        if (tipo === "rentabilidad") {
            url = window.reportesUrls.exportarRentabilidad;
            params.append("anio", document.getElementById("anioRentabilidad").value);
        } else {
            if (tipo === "servicios") {
                url = window.reportesUrls.exportarServicios;
                const tec = document.getElementById("tecnicoId").value;
                if (tec) params.append("tecnicoId", tec);
            } else if (tipo === "proyectos") {
                url = window.reportesUrls.exportarProyectos;
            }
            
            const fd = document.getElementById("fechaDesde").value;
            const fh = document.getElementById("fechaHasta").value;
            const est = document.getElementById("estadoGeneral").value;

            if (fd) params.append("fechaDesde", fd);
            if (fh) params.append("fechaHasta", fh);
            if (est) params.append("estado", est);
        }

        window.location.href = `${url}?${params.toString()}`;
    });
});

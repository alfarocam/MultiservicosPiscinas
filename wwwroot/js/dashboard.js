//Gráficos del Dashboard General del Administrador
document.addEventListener('DOMContentLoaded', function () {

    // Paleta de colores consistente con el sistema
    const COLORES = [
        '#3B82F6', '#10B981', '#F59E0B', '#EF4444',
        '#8B5CF6', '#EC4899', '#06B6D4', '#6366F1',
        '#14B8A6', '#F97316'
    ];

    function leerJSON(elementId) {
        try {
            const el = document.getElementById(elementId);
            if (!el) return null;
            const data = JSON.parse(el.textContent);
            return data && data.length > 0 ? data : null;
        } catch (e) {
            console.error('Error al leer datos JSON (' + elementId + '):', e);
            return null;
        }
    }

    //GRÁFICO 1: Servicios por mes (últimos 6 meses) — Line chart
    (function () {
        const datos = leerJSON('datosServiciosPorMes');
        const canvas = document.getElementById('graficoServiciosMes');
        if (!datos || !canvas) return;

        new Chart(canvas.getContext('2d'), {
            type: 'bar',
            data: {
                labels: datos.map(d => d.mes),
                datasets: [{
                    label: 'Servicios',
                    data: datos.map(d => d.cantidad),
                    backgroundColor: 'rgba(59, 130, 246, 0.7)',
                    borderColor: '#3B82F6',
                    borderWidth: 2,
                    borderRadius: 6,
                    borderSkipped: false
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: ctx => ' ' + ctx.parsed.y + ' servicio(s)'
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: { precision: 0 },
                        grid: { color: 'rgba(0,0,0,0.05)' }
                    },
                    x: { grid: { display: false } }
                }
            }
        });
    })();

    //GRÁFICO 2: Estado de proyectos, Doughnut chart
    (function () {
        const datos = leerJSON('datosEstadosProyecto');
        const canvas = document.getElementById('graficoEstadosProyecto');
        if (!datos || !canvas) return;

        new Chart(canvas.getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: datos.map(d => d.estado),
                datasets: [{
                    data: datos.map(d => d.cantidad),
                    backgroundColor: COLORES.slice(0, datos.length),
                    borderColor: '#fff',
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 14,
                            font: { size: 12, family: "'Segoe UI', sans-serif" },
                            color: '#444'
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function (ctx) {
                                const total = ctx.dataset.data.reduce((a, b) => a + b, 0);
                                const pct = ((ctx.parsed / total) * 100).toFixed(1);
                                return ` ${ctx.label}: ${ctx.parsed} (${pct}%)`;
                            }
                        }
                    }
                }
            }
        });
    })();

    //GRÁFICO 3: Visitas técnicas por técnico, Bar horizontal
    (function () {
        const datos = leerJSON('datosVisitasTecnico');
        const canvas = document.getElementById('graficoVisitasTecnico');
        if (!datos || !canvas) return;

        new Chart(canvas.getContext('2d'), {
            type: 'bar',
            data: {
                labels: datos.map(d => d.nombreTecnico),
                datasets: [{
                    label: 'Visitas',
                    data: datos.map(d => d.cantidad),
                    backgroundColor: COLORES.slice(0, datos.length),
                    borderRadius: 6,
                    borderSkipped: false
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: ctx => ' ' + ctx.parsed.x + ' visita(s)'
                        }
                    }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        ticks: { precision: 0 },
                        grid: { color: 'rgba(0,0,0,0.05)' }
                    },
                    y: { grid: { display: false } }
                }
            }
        });
    })();
});

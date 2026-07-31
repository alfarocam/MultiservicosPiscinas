// Inicializar gráfico de distribución de gastos operativos
document.addEventListener('DOMContentLoaded', function () {
    const datosGraficoElement = document.getElementById('datosGrafico');

    if (!datosGraficoElement) {
        return; // No hay datos, salir
    }

    try {
        const datos = JSON.parse(datosGraficoElement.textContent);

        if (!datos || datos.length === 0) {
            return; // Sin datos
        }

        const canvas = document.getElementById('graficoDistribucion');
        if (!canvas) {
            return;
        }

        const etiquetas = datos.map(d => d.nombreCategoria);
        const valores = datos.map(d => d.total);

        // Paleta de colores consistente
        const colores = [
            '#3B82F6', // Azul
            '#10B981', // Verde
            '#F59E0B', // Ámbar
            '#EF4444', // Rojo
            '#8B5CF6', // Púrpura
            '#EC4899', // Rosa
            '#06B6D4', // Cian
            '#6366F1', // Indigo
            '#14B8A6', // Teal
            '#F97316'  // Naranja
        ];

        // Extender colores si hay más categorías que colores disponibles
        while (colores.length < etiquetas.length) {
            colores.push('#' + Math.floor(Math.random()*16777215).toString(16));
        }

        const ctx = canvas.getContext('2d');
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: etiquetas,
                datasets: [{
                    data: valores,
                    backgroundColor: colores.slice(0, etiquetas.length),
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
                            padding: 15,
                            font: {
                                size: 12,
                                family: "'Plus Jakarta Sans', sans-serif"
                            },
                            color: '#333'
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const valor = context.parsed;
                                const porcentaje = ((valor / total) * 100).toFixed(2);
                                return context.label + ': ₡' + valor.toLocaleString('es-CR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' (' + porcentaje + '%)';
                            }
                        }
                    }
                }
            }
        });
    } catch (e) {
        console.error('Error al renderizar gráfico:', e);
    }
});

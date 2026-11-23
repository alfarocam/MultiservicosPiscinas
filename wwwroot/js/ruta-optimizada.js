(function () {
    var datosEl = document.getElementById('datosRutaAdmin');
    if (!datosEl) return;

    var config = JSON.parse(datosEl.textContent || '{}');
    var paradas = config.paradas || [];
    var ordenFinal = [];
    var ultimaRutaPayload = null;
    var guardando = false;

    function construirEnlaceGoogleMaps(listaOrdenada) {
        if (!listaOrdenada || listaOrdenada.length < 2) return '';

        var origen = listaOrdenada[0];
        var destino = listaOrdenada[listaOrdenada.length - 1];
        var intermedias = listaOrdenada.slice(1, listaOrdenada.length - 1);

        var waypointsTexto = intermedias
            .map(function (p) { return p.lat + ',' + p.lng; })
            .join('|');

        var url = 'https://www.google.com/maps/dir/?api=1'
            + '&origin=' + origen.lat + ',' + origen.lng
            + '&destination=' + destino.lat + ',' + destino.lng
            + '&travelmode=driving';

        if (waypointsTexto) {
            url += '&waypoints=' + waypointsTexto;
        }

        return url;
    }

    function actualizarEstadoGuardado(mensaje, clase) {
        var el = document.getElementById('estadoGuardadoRuta');
        if (!el) return;
        el.className = 'small text-center mb-3 ' + clase;
        el.innerHTML = mensaje;
    }

    function guardarRutaEnServidor(silencioso) {
        if (!ultimaRutaPayload || !config.guardarUrl || guardando) return;

        var tokenInput = document.querySelector('#formTokenRuta input[name="__RequestVerificationToken"]');
        var token = tokenInput ? tokenInput.value : '';

        guardando = true;
        var btnGuardar = document.getElementById('btnGuardarRuta');
        if (btnGuardar) btnGuardar.disabled = true;

        actualizarEstadoGuardado(
            '<span class="spinner-border spinner-border-sm me-1"></span>Guardando ruta...',
            'text-muted'
        );

        fetch(config.guardarUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(ultimaRutaPayload)
        })
            .then(function (resp) {
                return resp.json().then(function (data) {
                    return { ok: resp.ok, data: data };
                });
            })
            .then(function (res) {
                if (res.ok && res.data && res.data.success) {
                    actualizarEstadoGuardado(
                        '<i class="bi bi-check-circle-fill me-1"></i>Ruta guardada y disponible para el técnico y reportes.',
                        'text-success fw-semibold'
                    );
                    if (!silencioso && typeof Swal !== 'undefined') {
                        Swal.fire({
                            title: 'Ruta guardada',
                            text: res.data.mensaje || 'Ruta optimizada guardada correctamente.',
                            icon: 'success',
                            confirmButtonText: 'Entendido',
                            confirmButtonColor: '#198754'
                        });
                    }
                } else {
                    var msgError = (res.data && res.data.mensaje) || 'No se pudo guardar la ruta.';
                    actualizarEstadoGuardado(
                        '<i class="bi bi-exclamation-triangle-fill me-1"></i>' + msgError,
                        'text-danger fw-semibold'
                    );
                    if (!silencioso && typeof Swal !== 'undefined') {
                        Swal.fire({
                            title: 'Error al guardar',
                            text: msgError,
                            icon: 'error',
                            confirmButtonText: 'Entendido',
                            confirmButtonColor: '#dc3545'
                        });
                    }
                }
            })
            .catch(function () {
                actualizarEstadoGuardado(
                    '<i class="bi bi-exclamation-triangle-fill me-1"></i>Error de red al guardar la ruta.',
                    'text-danger fw-semibold'
                );
            })
            .finally(function () {
                guardando = false;
                if (btnGuardar) btnGuardar.disabled = false;
            });
    }

    function procesarResultado(resultado) {
        var ruta = resultado.routes[0];
        var ordenWaypoints = ruta.waypoint_order || [];

        var intermedias = paradas.slice(1, paradas.length - 1);
        ordenFinal = [paradas[0]];
        ordenWaypoints.forEach(function (indice) {
            ordenFinal.push(intermedias[indice]);
        });
        ordenFinal.push(paradas[paradas.length - 1]);

        var distanciaTotalMetros = 0;
        var duracionTotalSegundos = 0;
        var legs = ruta.legs || [];

        legs.forEach(function (tramo) {
            distanciaTotalMetros += tramo.distance.value;
            duracionTotalSegundos += tramo.duration.value;
        });

        var distanciaKm = Number((distanciaTotalMetros / 1000).toFixed(2));
        var duracionMin = Math.round(duracionTotalSegundos / 60);

        document.getElementById('resumenRuta').innerText =
            'Distancia total: ' + distanciaKm.toFixed(1) + ' km — Tiempo estimado: ' + duracionMin + ' min';

        var lista = document.getElementById('listaParadas');
        lista.innerHTML = '';

        var paradasPayload = ordenFinal.map(function (parada, idx) {
            var tramoKm = 0;
            var tramoMin = 0;

            if (idx > 0 && legs[idx - 1]) {
                tramoKm = Number((legs[idx - 1].distance.value / 1000).toFixed(2));
                tramoMin = Math.round(legs[idx - 1].duration.value / 60);
            }

            var item = document.createElement('li');
            item.className = 'mb-2';
            var detalleTramo = idx === 0
                ? 'Punto de partida'
                : ('Tramo: ' + tramoKm.toFixed(1) + ' km · ' + tramoMin + ' min');

            item.innerHTML =
                '<strong>' + parada.cliente + '</strong><br>' +
                '<span class="text-muted small">' + parada.direccion + ' — ' + parada.hora + '</span><br>' +
                '<span class="text-primary small">' + detalleTramo + '</span>';
            lista.appendChild(item);

            return {
                citaId: parada.citaId,
                ordenVisita: idx + 1,
                distanciaTramoKm: tramoKm,
                duracionTramoMin: tramoMin
            };
        });

        var enlace = construirEnlaceGoogleMaps(ordenFinal);

        ultimaRutaPayload = {
            tecnicoId: config.tecnicoId,
            fecha: config.fecha,
            distanciaTotalKm: distanciaKm,
            duracionTotalMin: duracionMin,
            enlaceGoogleMaps: enlace,
            paradas: paradasPayload
        };

        document.getElementById('btnGuardarRuta').disabled = false;
        document.getElementById('btnExportar').disabled = false;

        guardarRutaEnServidor(true);
    }

    window.iniciarMapa = function () {
        if (paradas.length < 2) return;

        var mapaEl = document.getElementById('mapaRuta');
        if (!mapaEl) return;

        var map = new google.maps.Map(mapaEl, {
            zoom: 12,
            center: { lat: paradas[0].lat, lng: paradas[0].lng }
        });

        var directionsService = new google.maps.DirectionsService();
        var directionsRenderer = new google.maps.DirectionsRenderer({ map: map });

        var origen = paradas[0];
        var destino = paradas[paradas.length - 1];
        var paradasIntermedias = paradas.slice(1, paradas.length - 1);

        var waypoints = paradasIntermedias.map(function (p) {
            return {
                location: { lat: p.lat, lng: p.lng },
                stopover: true
            };
        });

        directionsService.route({
            origin: { lat: origen.lat, lng: origen.lng },
            destination: { lat: destino.lat, lng: destino.lng },
            waypoints: waypoints,
            optimizeWaypoints: true,
            travelMode: google.maps.TravelMode.DRIVING
        }, function (resultado, estado) {
            if (estado === 'OK') {
                directionsRenderer.setDirections(resultado);
                procesarResultado(resultado);
            } else {
                document.getElementById('resumenRuta').innerText =
                    'No se pudo calcular la ruta: ' + estado;
            }
        });
    };

    var btnGuardar = document.getElementById('btnGuardarRuta');
    if (btnGuardar) {
        btnGuardar.addEventListener('click', function () {
            guardarRutaEnServidor(false);
        });
    }

    var btnExportar = document.getElementById('btnExportar');
    if (btnExportar) {
        btnExportar.addEventListener('click', function () {
            var url = construirEnlaceGoogleMaps(ordenFinal);
            if (url) {
                window.open(url, '_blank');
            }
        });
    }
})();

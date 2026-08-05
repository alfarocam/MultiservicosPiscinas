// Selector de ubicación con Google Maps para "Nuevo Cliente" (Views/Clientes/Crear.cshtml).
// Reemplaza el tipeo manual de Latitud/Longitud: el admin marca un pin en el mapa
// (haciendo clic o arrastrando el marcador), y este script:
//   1) llena Latitud/Longitud (de solo lectura en la vista)
//   2) hace geocodificación inversa con Google y trata de auto-seleccionar
//      Provincia / Cantón / Distrito en los combos que ya existen (ubicaciones.js
//      es el que sabe cómo pedirle los cantones/distritos al servidor; este
//      script no duplica esa lógica, solo dispara los mismos combos y espera a
//      que se llenen para elegir la opción que coincide).
//
// Importante: la detección automática es "mejor esfuerzo". Google no siempre
// devuelve el distrito con la misma precisión con la que está cargada la base
// de datos de Costa Rica, así que los combos quedan siempre editables por si
// el admin necesita corregir el nivel que no se detectó bien.
//
// La función iniciarMapaCliente() es el callback que Google Maps JS API llama
// automáticamente una vez que termina de cargar su script (ver el <script> con
// "callback=iniciarMapaCliente" en Crear.cshtml). Se define en window para que
// Google pueda encontrarla, igual que "iniciarMapa" en Ruta Optimizada.
function iniciarMapaCliente() {
    var contenedor = document.getElementById('mapaCliente');
    var campoLatitud = document.getElementById('Latitud');
    var campoLongitud = document.getElementById('Longitud');
    var selectProvincia = document.getElementById('provinciaSelect');
    var selectCanton = document.getElementById('cantonSelect');
    var selectDistrito = document.getElementById('distritoSelect');

    if (!contenedor || !campoLatitud || !campoLongitud) {
        return;
    }

    // Centro por defecto: San José, Costa Rica (todos los clientes de la empresa
    // están en el país, así que arrancar ahí evita que el admin tenga que buscar
    // el mapa mundial para encontrar la zona).
    var centroInicial = { lat: 9.9281, lng: -84.0907 };

    var valorLatInicial = parseFloat(campoLatitud.value);
    var valorLngInicial = parseFloat(campoLongitud.value);
    var tieneCoordenadasPrevias = !isNaN(valorLatInicial) && !isNaN(valorLngInicial);

    var mapa = new google.maps.Map(contenedor, {
        center: tieneCoordenadasPrevias ? { lat: valorLatInicial, lng: valorLngInicial } : centroInicial,
        zoom: tieneCoordenadasPrevias ? 15 : 8
    });

    var geocoder = new google.maps.Geocoder();
    var marcador = null;

    // Quita acentos y pasa a minúsculas, para poder comparar "San José" con
    // "san jose" sin que la tilde arruine la coincidencia.
    function normalizar(texto) {
        return (texto || '')
            .toString()
            .normalize('NFD')
            .replace(/[̀-ͯ]/g, '')
            .trim()
            .toLowerCase();
    }

    // Busca, dentro de las <option> de un <select>, la que coincida por nombre
    // normalizado. Devuelve el value de esa opción, o null si no encontró nada.
    function buscarOpcionPorNombre(select, nombreBuscado) {
        var nombreNormalizado = normalizar(nombreBuscado);
        var opciones = select.querySelectorAll('option');
        for (var i = 0; i < opciones.length; i++) {
            if (normalizar(opciones[i].textContent) === nombreNormalizado) {
                return opciones[i].value;
            }
        }
        return null;
    }

    // Espera a que un <select> reciba nuevas <option> (las agrega ubicaciones.js
    // vía AJAX cuando cambia el combo anterior) y ejecuta un callback una sola vez.
    function esperarOpciones(select, callback) {
        var observer = new MutationObserver(function () {
            if (!select.disabled && select.querySelectorAll('option').length > 1) {
                observer.disconnect();
                callback();
            }
        });
        observer.observe(select, { childList: true });

        // Por si las opciones ya estaban puestas antes de empezar a observar.
        if (!select.disabled && select.querySelectorAll('option').length > 1) {
            observer.disconnect();
            callback();
        }
    }

    function extraerComponente(componentes, tipoBuscado) {
        for (var i = 0; i < componentes.length; i++) {
            if (componentes[i].types.indexOf(tipoBuscado) !== -1) {
                return componentes[i].long_name;
            }
        }
        return null;
    }

    function autocompletarUbicacion(posicion) {
        if (!selectProvincia || !selectCanton || !selectDistrito) {
            return;
        }

        geocoder.geocode({ location: posicion }, function (resultados, estado) {
            if (estado !== 'OK' || !resultados || !resultados.length) {
                return;
            }

            var componentes = resultados[0].address_components;

            // En Google, para Costa Rica: la provincia suele venir como
            // "administrative_area_level_1" y el cantón como
            // "administrative_area_level_2". El distrito no siempre viene
            // marcado de forma consistente, así que se intenta con varios
            // tipos posibles antes de rendirse.
            var nombreProvincia = extraerComponente(componentes, 'administrative_area_level_1');
            var nombreCanton = extraerComponente(componentes, 'administrative_area_level_2');
            var nombreDistrito = extraerComponente(componentes, 'locality')
                || extraerComponente(componentes, 'administrative_area_level_3')
                || extraerComponente(componentes, 'sublocality_level_1');

            if (!nombreProvincia) {
                return;
            }

            var idProvincia = buscarOpcionPorNombre(selectProvincia, nombreProvincia);
            if (!idProvincia) {
                // La provincia detectada no está en la base de datos (por ejemplo,
                // si todavía falta terminar de cargar la división territorial
                // completa de Costa Rica). No se fuerza nada: el admin completa
                // los combos a mano.
                return;
            }

            selectProvincia.value = idProvincia;
            selectProvincia.dispatchEvent(new Event('change'));

            if (!nombreCanton) {
                return;
            }

            esperarOpciones(selectCanton, function () {
                var idCanton = buscarOpcionPorNombre(selectCanton, nombreCanton);
                if (!idCanton) {
                    return;
                }

                selectCanton.value = idCanton;
                selectCanton.dispatchEvent(new Event('change'));

                if (!nombreDistrito) {
                    return;
                }

                esperarOpciones(selectDistrito, function () {
                    var idDistrito = buscarOpcionPorNombre(selectDistrito, nombreDistrito);
                    if (idDistrito) {
                        selectDistrito.value = idDistrito;
                    }
                });
            });
        });
    }

    function colocarMarcador(posicion) {
        campoLatitud.value = posicion.lat().toFixed(6);
        campoLongitud.value = posicion.lng().toFixed(6);

        if (marcador) {
            marcador.setPosition(posicion);
        } else {
            marcador = new google.maps.Marker({
                position: posicion,
                map: mapa,
                draggable: true
            });

            marcador.addListener('dragend', function () {
                colocarMarcador(marcador.getPosition());
            });
        }

        autocompletarUbicacion(posicion);
    }

    if (tieneCoordenadasPrevias) {
        marcador = new google.maps.Marker({
            position: { lat: valorLatInicial, lng: valorLngInicial },
            map: mapa,
            draggable: true
        });
        marcador.addListener('dragend', function () {
            colocarMarcador(marcador.getPosition());
        });
    }

    mapa.addListener('click', function (evento) {
        colocarMarcador(evento.latLng);
    });
}

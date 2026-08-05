// Selector de ubicación con Google Maps para "Nuevo Cliente" (Views/Clientes/Crear.cshtml).
// Reemplaza el tipeo manual de Latitud/Longitud: el admin marca un pin en el mapa
// (haciendo clic o arrastrando el marcador) y este script llena esos dos campos,
// que ahora son de solo lectura (readonly) en la vista.
//
// La función iniciarMapaCliente() es el callback que Google Maps JS API llama
// automáticamente una vez que termina de cargar su script (ver el <script> con
// "callback=iniciarMapaCliente" en Crear.cshtml). Se define en window para que
// Google pueda encontrarla, igual que "iniciarMapa" en Ruta Optimizada.
function iniciarMapaCliente() {
    var contenedor = document.getElementById('mapaCliente');
    var campoLatitud = document.getElementById('Latitud');
    var campoLongitud = document.getElementById('Longitud');

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

    var marcador = null;

    function colocarMarcador(posicion) {
        campoLatitud.value = posicion.lat().toFixed(6);
        campoLongitud.value = posicion.lng().toFixed(6);

        if (marcador) {
            marcador.setPosition(posicion);
            return;
        }

        marcador = new google.maps.Marker({
            position: posicion,
            map: mapa,
            draggable: true
        });

        marcador.addListener('dragend', function () {
            colocarMarcador(marcador.getPosition());
        });
    }

    if (tieneCoordenadasPrevias) {
        colocarMarcador(new google.maps.LatLng(valorLatInicial, valorLngInicial));
    }

    mapa.addListener('click', function (evento) {
        colocarMarcador(evento.latLng);
    });
}

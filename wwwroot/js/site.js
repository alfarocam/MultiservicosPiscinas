// ==========================================================================
// Lógica para colapsar y expandir la barra lateral (Sidebar / Drawer en Móvil)
// ==========================================================================
document.addEventListener("DOMContentLoaded", function () {
    const sidebarToggle = document.getElementById("sidebarToggle");
    const appContainer = document.querySelector(".t-app-container");
    const mobileQuery = window.matchMedia("(max-width: 768px)");

    // Crear o recuperar el backdrop para dispositivos móviles
    let backdrop = document.querySelector(".t-sidebar-backdrop");
    if (!backdrop) {
        backdrop = document.createElement("div");
        backdrop.className = "t-sidebar-backdrop";
        backdrop.style.display = "none";
        document.body.appendChild(backdrop);
    }

    function setSidebarCollapsed(collapsed) {
        if (!appContainer) return;
        if (collapsed) {
            appContainer.classList.add("t-sidebar-collapsed");
            if (backdrop) {
                backdrop.classList.remove("show");
                setTimeout(() => {
                    if (appContainer.classList.contains("t-sidebar-collapsed")) {
                        backdrop.style.display = "none";
                    }
                }, 250);
            }
        } else {
            appContainer.classList.remove("t-sidebar-collapsed");
            if (backdrop && mobileQuery.matches) {
                backdrop.style.display = "block";
                requestAnimationFrame(() => {
                    backdrop.classList.add("show");
                });
            }
        }
    }

    // En móvil el sidebar inicia escondido para no tapar el contenido
    if (appContainer && mobileQuery.matches) {
        setSidebarCollapsed(true);
    }

    if (sidebarToggle && appContainer) {
        sidebarToggle.addEventListener("click", function () {
            const isCurrentlyCollapsed = appContainer.classList.contains("t-sidebar-collapsed");
            setSidebarCollapsed(!isCurrentlyCollapsed);
        });
    }

    // Cerrar sidebar al tocar el backdrop oscuro en móvil
    if (backdrop) {
        backdrop.addEventListener("click", function () {
            setSidebarCollapsed(true);
        });
    }

    // Lógica para marcar como activa la opción del menú al darle click y cerrar en móvil
    const navLinks = document.querySelectorAll(".t-sidebar .nav-link");

    navLinks.forEach(link => {
        link.addEventListener("click", function () {
            if (mobileQuery.matches) {
                setSidebarCollapsed(true);
            }
        });
    });

    // Recuperar la opción activa almacenada previamente, o usar la ruta actual si no hay ninguna
    let activeLinkHref = localStorage.getItem("activeSidebarLink");
    if (!activeLinkHref) {
        activeLinkHref = window.location.pathname;
    }

    if (activeLinkHref) {
        navLinks.forEach(link => {
            if (link.getAttribute("href") === activeLinkHref) {
                link.classList.add("active");
            }
        });
    }

    // Agregar evento click a los enlaces
    navLinks.forEach(link => {
        link.addEventListener("click", function () {
            navLinks.forEach(l => l.classList.remove("active"));
            this.classList.add("active");
            localStorage.setItem("activeSidebarLink", this.getAttribute("href"));
        });
    });
});

// ==========================================================================
// Configuración Global y Normalización de DataTables
// ==========================================================================
if (window.jQuery && $.fn.dataTable) {
    // 1. Opciones por defecto para todas las instancias de DataTables
    $.extend(true, $.fn.dataTable.defaults, {
        responsive: false,
        autoWidth: false,
        language: {
            search: "",
            searchPlaceholder: "Buscar...",
            lengthMenu: "Mostrar _MENU_",
            info: "_START_ a _END_ de _TOTAL_",
            infoEmpty: "0 a 0 de 0",
            infoFiltered: "(de _MAX_)",
            zeroRecords: "No se encontraron resultados",
            emptyTable: "No hay datos disponibles",
            paginate: {
                first: '<i class="bi bi-chevron-double-left"></i>',
                previous: '<i class="bi bi-chevron-left"></i>',
                next: '<i class="bi bi-chevron-right"></i>',
                last: '<i class="bi bi-chevron-double-right"></i>'
            }
        }
    });

    // 2. Normalización de contenedores para evitar que los controles se desplacen con el scroll
    $(document).on('init.dt', function (e, settings) {
        var api = new $.fn.dataTable.Api(settings);
        var $table = $(api.table().node());
        var $wrapper = $(api.table().container());
        var $parent = $wrapper.parent();

        // Si la tabla estaba envuelta externamente en .table-responsive,
        // quitamos ese contenedor exterior para que el buscador y paginación ocupen el 100% de la tarjeta,
        // y hacemos que la columna contenedora de la tabla tenga scroll táctil independiente.
        if ($parent.hasClass('table-responsive')) {
            $wrapper.unwrap();
        }
        $table.parent().addClass('table-responsive w-100 u-table-inner-scroll');

        // Asegurar placeholder en el campo de búsqueda si la API remota de idioma lo sobrescribió
        var $filterInput = $wrapper.find('.dataTables_filter input');
        if ($filterInput.length && !$filterInput.attr('placeholder')) {
            $filterInput.attr('placeholder', 'Buscar en la tabla...');
        }
    });
}

// ==========================================================================
// Prevención global para menús desplegables (Dropdowns) en tablas y tarjetas
// Evita que el overflow corte los menús usando Popper fixed strategy
// ==========================================================================
(function () {
    if (typeof bootstrap !== 'undefined' && bootstrap.Dropdown) {
        bootstrap.Dropdown.Default.boundary = 'viewport';
        bootstrap.Dropdown.Default.popperConfig = function (defaultBsPopperConfig) {
            return {
                ...defaultBsPopperConfig,
                strategy: 'fixed'
            };
        };
    }

    // Solo en desktop expandir overflow de forma temporal si es necesario;
    // en móvil evitamos cambiar overflow: visible para no desbordar el ancho de pantalla.
    document.addEventListener('show.bs.dropdown', function (e) {
        if (window.innerWidth > 768) {
            var tableResp = e.target.closest('.table-responsive');
            if (tableResp) {
                tableResp.classList.add('u-overflow-visible');
                tableResp.style.overflow = 'visible';
            }
            var card = e.target.closest('.c-data-card, .card');
            if (card) {
                card.classList.add('u-overflow-visible');
                card.style.overflow = 'visible';
            }
        }
    });

    document.addEventListener('hidden.bs.dropdown', function (e) {
        var tableResp = e.target.closest('.table-responsive');
        if (tableResp) {
            tableResp.classList.remove('u-overflow-visible');
            tableResp.style.overflow = '';
        }
        var card = e.target.closest('.c-data-card, .card');
        if (card) {
            card.classList.remove('u-overflow-visible');
            card.style.overflow = '';
        }
    });
})();

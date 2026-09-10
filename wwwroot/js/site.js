// ==========================================================================
// Lógica para barra lateral (Desktop Collapse & Mobile Drawer)
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
        document.body.appendChild(backdrop);
    }

    function closeMobileSidebar() {
        if (!appContainer) return;
        appContainer.classList.remove("t-sidebar-mobile-open");
        if (sidebarToggle) {
            sidebarToggle.classList.remove("is-active");
            sidebarToggle.setAttribute("aria-expanded", "false");
        }
        if (backdrop) {
            backdrop.classList.remove("show");
        }
    }

    function openMobileSidebar() {
        if (!appContainer) return;
        appContainer.classList.add("t-sidebar-mobile-open");
        if (sidebarToggle) {
            sidebarToggle.classList.add("is-active");
            sidebarToggle.setAttribute("aria-expanded", "true");
        }
        if (backdrop) {
            backdrop.classList.add("show");
        }
    }

    function toggleSidebar() {
        if (!appContainer) return;
        if (mobileQuery.matches) {
            if (appContainer.classList.contains("t-sidebar-mobile-open")) {
                closeMobileSidebar();
            } else {
                openMobileSidebar();
            }
        } else {
            appContainer.classList.toggle("t-sidebar-collapsed");
            const isCollapsed = appContainer.classList.contains("t-sidebar-collapsed");
            if (sidebarToggle) {
                sidebarToggle.setAttribute("aria-expanded", !isCollapsed ? "true" : "false");
            }
        }
    }

    if (sidebarToggle) {
        sidebarToggle.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            toggleSidebar();
        });
    }

    // Cerrar sidebar al tocar el backdrop oscuro en móvil
    if (backdrop) {
        backdrop.addEventListener("click", function () {
            closeMobileSidebar();
        });
    }

    // Cerrar sidebar al hacer click en cualquier link del menú en móvil
    const navLinks = document.querySelectorAll(".t-sidebar .nav-link");
    navLinks.forEach(link => {
        link.addEventListener("click", function () {
            if (mobileQuery.matches) {
                closeMobileSidebar();
            }
        });
    });

    // Cerrar sidebar con la tecla Escape en móvil
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && mobileQuery.matches && appContainer && appContainer.classList.contains("t-sidebar-mobile-open")) {
            closeMobileSidebar();
        }
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

    // Marcar como activa la opción seleccionada
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
        responsive: true,
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
        var $wrapper = $(api.table().container());
        var $parent = $wrapper.parent();

        // Si la tabla estaba envuelta externamente en .table-responsive,
        // quitamos ese contenedor exterior para que el buscador y paginación ocupen el 100% de la tarjeta,
        // y para que DataTables Responsive gestione el colapso directamente sin interferencias.
        if ($parent.hasClass('table-responsive')) {
            $wrapper.unwrap();
        }

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
})();


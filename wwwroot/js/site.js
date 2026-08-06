// Lógica para colapsar y expandir la barra lateral (Sidebar)
document.addEventListener("DOMContentLoaded", function () {
    const sidebarToggle = document.getElementById("sidebarToggle");
    const appContainer = document.querySelector(".t-app-container");
    const mobileQuery = window.matchMedia("(max-width: 768px)");

    // En móvil el sidebar inicia escondido para no tapar el contenido
    if (appContainer && mobileQuery.matches) {
        appContainer.classList.add("t-sidebar-collapsed");
    }

    if (sidebarToggle && appContainer) {
        sidebarToggle.addEventListener("click", function () {
            appContainer.classList.toggle("t-sidebar-collapsed");
        });
    }

    // Lógica para marcar como activa la opción del menú al darle click
    const navLinks = document.querySelectorAll(".t-sidebar .nav-link");

    // En móvil, cerrar el sidebar al seleccionar una opción del menú
    navLinks.forEach(link => {
        link.addEventListener("click", function () {
            if (appContainer && mobileQuery.matches) {
                appContainer.classList.add("t-sidebar-collapsed");
            }
        });
    });

    // Recuperar la opción activa almacenada previamente, o usar la ruta actual si no hay ninguna
    let activeLinkHref = localStorage.getItem("activeSidebarLink");
    
    if (!activeLinkHref) {
        activeLinkHref = window.location.pathname;
    }

    if (activeLinkHref) {
        let found = false;
        navLinks.forEach(link => {
            if (link.getAttribute("href") === activeLinkHref) {
                link.classList.add("active");
                found = true;
            }
        });
        
        // Si no se encontró coincidencia exacta y la URL es solo la raíz u otro,
        // al menos intentar asegurar que se marque algo, pero dejaremos que el click actúe
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

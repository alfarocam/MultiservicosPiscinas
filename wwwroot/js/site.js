// Lógica para colapsar y expandir la barra lateral (Sidebar)
document.addEventListener("DOMContentLoaded", function () {
    const sidebarToggle = document.getElementById("sidebarToggle");
    const appContainer = document.querySelector(".t-app-container");

    if (sidebarToggle && appContainer) {
        sidebarToggle.addEventListener("click", function () {
            appContainer.classList.toggle("t-sidebar-collapsed");
        });
    }
});

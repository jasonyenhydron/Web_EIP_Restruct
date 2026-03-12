function toggleMisMenu() {
    const submenu = document.getElementById('mis-submenu');
    const arrow = document.getElementById('mis-arrow');
    if (!submenu || !arrow) return;

    if (submenu.classList.contains('hidden')) {
        submenu.classList.remove('hidden');
        arrow.style.transform = 'rotate(180deg)';
    } else {
        submenu.classList.add('hidden');
        arrow.style.transform = 'rotate(0deg)';
    }
}

document.addEventListener('DOMContentLoaded', () => {
    if (window.__sidebarRuntimeInited) return;
    window.__sidebarRuntimeInited = true;

    const sidebar = document.getElementById('sidebar');
    const toggleBtns = document.querySelectorAll('.sidebar-toggle-btn, #sidebar-toggle');
    if (!sidebar || toggleBtns.length === 0) return;

    function updateIcons() {
        const isCollapsed = sidebar.classList.contains('collapsed');
        toggleBtns.forEach(btn => {
            if (isCollapsed) {
                btn.classList.remove('active');
            } else {
                btn.classList.add('active');
            }
        });
    }

    if (localStorage.getItem('sidebarCollapsed') === 'true') {
        sidebar.classList.add('collapsed');
    }

    updateIcons();

    toggleBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            sidebar.classList.toggle('collapsed');
            localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
            updateIcons();
        });
    });
});


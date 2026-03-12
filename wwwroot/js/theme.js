(function () {
    const KEY = "eip.theme";
    const DEFAULT_THEME = "blue-white";
    const THEMES = [
        { id: "blue-white", name: "藍白經典" },
        { id: "ocean-blue", name: "海洋藍" },
        { id: "indigo-white", name: "靛藍白" }
    ];

    function normalizeTheme(themeId) {
        const id = String(themeId || "").trim().toLowerCase();
        return THEMES.some(t => t.id === id) ? id : DEFAULT_THEME;
    }

    function getSavedTheme() {
        try {
            return normalizeTheme(localStorage.getItem(KEY));
        } catch {
            return DEFAULT_THEME;
        }
    }

    function applyTheme(themeId, persist) {
        const id = normalizeTheme(themeId);
        document.documentElement.setAttribute("data-theme", id);

        if (persist !== false) {
            try {
                localStorage.setItem(KEY, id);
            } catch {
            }
        }

        window.dispatchEvent(new CustomEvent("eip-theme-change", { detail: { theme: id } }));
        return id;
    }

    function initTheme() {
        const current = document.documentElement.getAttribute("data-theme");
        if (current) {
            applyTheme(current, false);
            return;
        }
        applyTheme(getSavedTheme(), false);
    }

    window.eipTheme = {
        key: KEY,
        defaultTheme: DEFAULT_THEME,
        list: THEMES,
        get: () => normalizeTheme(document.documentElement.getAttribute("data-theme") || getSavedTheme()),
        set: (themeId) => applyTheme(themeId, true),
        apply: (themeId) => applyTheme(themeId, false),
        init: initTheme
    };

    initTheme();
})();


let sidebarSimpleBarInstance = null;

window.reinitUi = function () {
    try {
        // ==========================================
        // 1. DÉTRUIRE l'ancienne instance SimpleBar
        // ==========================================
        if (sidebarSimpleBarInstance) {
            try { sidebarSimpleBarInstance.unMount(); } catch (_) { }
            sidebarSimpleBarInstance = null;
        }

        // ==========================================
        // 2. CRÉER une NOUVELLE instance SimpleBar
        // ==========================================
        var sidebarScroll = document.getElementById('sidebar-scroll');
        if (sidebarScroll && window.SimpleBar) {
            sidebarSimpleBarInstance = new SimpleBar(sidebarScroll, { autoHide: true });
        }

        // ==========================================
        // 3. RÉ-ATTACHER les événements de clic
        //    des menus déroulants de la sidebar
        //    (perdus après le render Blazor)
        // ==========================================

        // Helper pour éviter la duplication des événements
        function removeOldListeners(selector, key) {
            document.querySelectorAll(selector).forEach(function (el) {
                if (el[key]) {
                    el.removeEventListener('click', el[key]);
                    delete el[key];
                }
            });
        }
        function addListener(selector, key, handler) {
            document.querySelectorAll(selector).forEach(function (el) {
                el[key] = handler;
                el.addEventListener('click', handler);
            });
        }

        // Nettoyer les anciens listeners
        removeOldListeners('.main-menu > li.slide.has-sub > a, .nav > ul > .slide.has-sub > a', '_level1Click');

        // --- Niveau 1 : clic sur un item parent ---
        addListener('.main-menu > li.slide.has-sub > a, .nav > ul > .slide.has-sub > a', '_level1Click', function (e) {
            e.preventDefault();
            var html = document.documentElement;
            var isHover = html.getAttribute('data-nav-style') === 'menu-hover' || html.getAttribute('data-nav-style') === 'icon-hover';
            var isMobile = window.innerWidth < 992;
            if (!isHover || isMobile || (!html.getAttribute('data-toggled') && html.getAttribute('data-nav-layout') !== 'horizontal')) {
                var parentMenu = this.closest('.nav.sub-open');
                if (parentMenu) {
                    parentMenu.querySelectorAll(':scope > ul > .slide.has-sub > a').forEach(function (siblingLink) {
                        var sub = siblingLink.nextElementSibling;
                        if (sub && (sub.style.display === 'block' || window.getComputedStyle(sub).display === 'block')) {
                            slideUp(sub);
                        }
                    });
                }
                slideToggle(this.nextElementSibling);
            }
        });

        // Nettoyer les anciens listeners internes
        removeOldListeners('.slide-menu .slide.has-sub > a', '_innerLevelClick');

        // --- Niveaux internes ---
        addListener('.slide-menu .slide.has-sub > a', '_innerLevelClick', function (e) {
            var html = document.documentElement;
            var isHover = html.getAttribute('data-nav-style') === 'menu-hover' || html.getAttribute('data-nav-style') === 'icon-hover';
            var isMobile = window.innerWidth < 992;
            if (!isHover || isMobile || (!html.getAttribute('data-toggled') && html.getAttribute('data-nav-layout') !== 'horizontal')) {
                var innerMenu = this.closest('.slide-menu');
                if (innerMenu) {
                    innerMenu.querySelectorAll(':scope .slide.has-sub > a').forEach(function (siblingLink) {
                        var sub = siblingLink.nextElementSibling;
                        if (sub && sub.style.display === 'block') {
                            slideUp(sub);
                        }
                    });
                }
                slideToggle(this.nextElementSibling);
            }
        });

        // ==========================================
        // 4. NE PAS appeler setNavActive() ici !
        //    Blazor gère déjà les classes 'active'
        //    via NavLink.ActiveClass. setNavActive()
        //    écrase ces classes et les onglets actifs
        //    disparaissent.
        // ==========================================

        console.log('[reinitUi] Sidebar réinitialisée avec succès');
    } catch (e) {
        console.error('[reinitUi] Erreur :', e);
    }
};

window.initPermissionTree = (containerId, data, dotNetRef, preSelectedIds) => {
    console.log("=== initPermissionTree appelé ===");
    console.log("containerId:", containerId);
    console.log("data brute reçue:", data);
    console.log("preSelectedIds reçus:", preSelectedIds);

    if (typeof $ === "undefined") {
        console.error("❌ jQuery n'est pas chargé (typeof $ === 'undefined')");
        return;
    }
    console.log("✅ jQuery chargé, version:", $.fn.jquery);

    if (typeof $.jstree === "undefined") {
        console.error("❌ jsTree n'est pas chargé ($.jstree === undefined)");
        return;
    }
    console.log("✅ jsTree chargé");

    const container = document.getElementById(containerId);
    if (!container) {
        console.error(`❌ Conteneur #${containerId} introuvable dans le DOM`);
        return;
    }
    console.log("✅ Conteneur trouvé:", container);

    if (!data || data.length === 0) {
        console.error("❌ data est vide ou null");
        return;
    }

    // Détruit une instance existante
    if ($.jstree.reference(container)) {
        $(container).jstree("destroy");
    }

    const mappedData = data.map(node => ({
        id: String(node.id),
        parent: (node.parent === null || node.parent === undefined || node.parent === "" || node.parent === "0")
            ? "#"
            : String(node.parent),
        text: `${node.text} (${node.code})`
    }));

    console.log("Data mappée pour jsTree:", mappedData);

    try {
        $(container).jstree({
            core: {
                data: mappedData,
                themes: { dots: true, icons: true }
            },
            plugins: ["checkbox"],
            checkbox: {
                three_state: true,
                cascade: "up+down",
                tie_selection: false
            }
        });

        $(container).on("changed.jstree", function (e, data) {
            if (!dotNetRef) return;
            const instance = $.jstree.reference(container);
            const checkedIds = instance.get_checked(null, false);
            dotNetRef.invokeMethodAsync("OnPermissionsChecked", checkedIds);
        });

        // Cocher les permissions pré-sélectionnées après le chargement de l'arbre
        $(container).on("ready.jstree", function () {
            if (preSelectedIds && preSelectedIds.length > 0) {
                console.log("🔄 Pré-sélection des permissions:", preSelectedIds);
                const instance = $.jstree.reference(container);
                if (instance) {
                    instance.check_node(preSelectedIds);
                    console.log("✅ Permissions pré-sélectionnées appliquées");
                }
            }
        });

        console.log("✅ jsTree initialisé avec succès");
    } catch (err) {
        console.error("❌ Erreur lors de l'initialisation de jsTree:", err);
    }
};

window.getCheckedPermissions = (containerId) => {
    if (typeof $ === "undefined" || typeof $.jstree === "undefined") return [];
    const container = document.getElementById(containerId);
    if (!container) return [];
    const instance = $.jstree.reference(container);
    if (!instance) return [];
    return instance.get_checked(null, false);
};
window.initRolesSelect = function (dotnetHelper) {

    let select = $('#rolesSelect');

    if (select.hasClass('select2-hidden-accessible')) {
        select.off('change');
        select.select2('destroy');
    }

    select.select2({
        width: '100%',
        placeholder: 'Sélectionner les rôles',
        allowClear: true,
        closeOnSelect: false
    });

    select.on('change', function () {

        let values = $(this).val() || [];

        console.log("Roles sélectionnés :", values);

        dotnetHelper.invokeMethodAsync(
            "UpdateRoles",
            values
        );

    });

};
/* fichier amélioré pour ouverture/fermeture modals avec Bootstrap si disponible,
   fallback propre si Bootstrap absent */

const mapProfil = {
    0: "SUPER_ADMIN",
    1: "ADMIN",
    2: "MON_OFFICER",
    3: "MON_MANAGER",
    4: "COMPTABLE"
};

function showToast(message, type) {
    try { 
        const modal = document.getElementById("createProfileModal");
        if (modal) hideModal(modal);

        const toastBody = document.getElementById("toastBody");
        if (toastBody) toastBody.textContent = message || "";
        else console.warn("Élément #toastBody introuvable.");

        const toastHeader = document.getElementById("toastHeader");
        if (toastHeader) {
            toastHeader.classList.remove(
                "bg-success", "bg-warning", "bg-danger", "bg-info", "bg-secondary",
                "text-white", "text-dark"
            );

            switch ((type || "").toLowerCase()) {
                case "success":
                    toastHeader.classList.add("bg-success", "text-white");
                    break;
                case "warning":
                    toastHeader.classList.add("bg-warning", "text-dark");
                    break;
                case "error":
                case "danger":
                    toastHeader.classList.add("bg-danger", "text-white");
                    break;
                case "info":
                    toastHeader.classList.add("bg-info", "text-dark");
                    break;
                default:
                    toastHeader.classList.add("bg-secondary", "text-white");
                    break;
            }
        } else {
            console.warn("Élément #toastHeader introuvable.");
        }

        const toastElement = document.getElementById("liveToast");
        if (toastElement && window.bootstrap && bootstrap.Toast) {
            const toast = new bootstrap.Toast(toastElement);
            toast.show();
        } else {
            console.warn("Élément #liveToast introuvable ou Bootstrap non chargé.");
        }
    } catch (ex) {
        console.error('showToast error', ex);
    }
}



function toggleOnLoaderAndToast() { 
    const loaderOverlay = document.getElementById("loaderOverlay");
    if (loaderOverlay) loaderOverlay.classList.add("show");
}

function toggleOffLoaderAndToast() {
    const loaderOverlay = document.getElementById("loaderOverlay");
    if (loaderOverlay) loaderOverlay.classList.remove("show");
}

function showLoader(message, title) {
    const loaderOverlay = document.getElementById("loaderOverlay");
    if (loaderOverlay) loaderOverlay.classList.add("show");

    setTimeout(() => {
        if (loaderOverlay) loaderOverlay.classList.remove("show");
        showToast(message, title);
    }, 100);
}

function showContentLoader() {
    document.getElementById("contentLoader")?.classList.add("show");
}

function hideContentLoader() {
    document.getElementById("contentLoader")?.classList.remove("show");
}

const profilLink = document.getElementById("profilLink");

if (profilLink) {
    profilLink.addEventListener("click", function () {
        showContentLoader();
    });
}

/* Helpers pour modals : utilise Bootstrap si dispo sinon fallback */
function createBackdrop() {
    const backdrop = document.createElement('div');
    backdrop.className = 'modal-backdrop fade show';
    backdrop.setAttribute('data-generated', 'true');
    document.body.appendChild(backdrop);
}

function removeGeneratedBackdrops() {
    document.querySelectorAll('div.modal-backdrop[data-generated="true"]').forEach(el => el.remove());
}

function showModal(el) {
    if (!el) return;
    if (window.bootstrap && bootstrap.Modal) {
        const instance = bootstrap.Modal.getOrCreateInstance(el, {});
        instance.show();
    } else {
        // fallback minimal
        el.style.display = 'block';
        el.classList.add('show');
        el.setAttribute('aria-modal', 'true');
        el.removeAttribute('aria-hidden');
        // ajouter backdrop si aucun n'existe
        if (!document.querySelector('.modal-backdrop')) createBackdrop();
    }
}

function hideModal(el) {
    if (!el) return;
    if (window.bootstrap && bootstrap.Modal) {
        const instance = bootstrap.Modal.getOrCreateInstance(el);
        try { instance.hide(); } catch (e) { /* safe */ }
    } else {
        el.style.display = 'none';
        el.classList.remove('show');
        el.setAttribute('aria-hidden', 'true');
        removeGeneratedBackdrops();
    }
}

 
async function DownloadBkmvti(event) {
    try {
        event.preventDefault();
        toggleOnLoaderAndToast();

        const typeMag = document.getElementById("typeMag").value;

        const response = await fetch("/ManqueAGagner/DownloadBkmvti", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ typeMag: typeMag })
        });

        if (!response.ok) {
            throw new Error("Erreur lors du téléchargement");
        }

        // IMPORTANT : lire en blob (fichier)
        const blob = await response.blob();

        // créer lien de téléchargement
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;

        // nom du fichier (optionnel)
        a.download = "BKMVTI.txt";

        document.body.appendChild(a);
        a.click();
        a.remove();

        toggleOffLoaderAndToast();
        showToast("Téléchargement réussi", "success");

    } catch (error) {
        toggleOffLoaderAndToast();
        showToast("Erreur : " + error.message, "danger");
    }
}

async function SaveMag(event) {
    try {
        event.preventDefault();
        toggleOnLoaderAndToast();

        let formData = new FormData(event.target);

        const response = await fetch("/ManqueAGagner/ProcessMagFiles", {
            method: "POST",
            body: formData
        });

        if (!response.ok) {
            throw new Error("Erreur réseau");
        }

        const result = await response.json();

        if (result.success) {
            toggleOffLoaderAndToast();
            showToast(result.message, "success");
            setTimeout(() => { location.reload(); }, 1000);
        } else {
            toggleOffLoaderAndToast();
            showToast(result.message, "warning");
        }

    } catch (error) {
        toggleOffLoaderAndToast();
        showToast("Erreur : " + error.message, "danger");
    }
}

function saveProfil(event) {
    event.preventDefault();
    toggleOnLoaderAndToast();

    let formData = new FormData(event.target);
    let data = Object.fromEntries(formData.entries());

    fetch("/Profil/SaveProfil", {
        method: "POST",
        credentials: 'same-origin',
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify(data)
    })
        .then(response => {
            if (!response.ok) {
                toggleOffLoaderAndToast();
                throw new Error("Erreur réseau");
            }
            return response.json();
        })
        .then(result => {
            if (result.success) {
                toggleOffLoaderAndToast();
                showToast(result.message, "success");
                setTimeout(() => { location.reload(); }, 1000);
            } else {
                toggleOffLoaderAndToast();
                showToast(result.message, "warning");
            }
        })
        .catch(error => {
            toggleOffLoaderAndToast();
            showToast("Erreur : " + error.message, "danger");
        });
}

// DOM-ready modal logic
document.addEventListener('DOMContentLoaded', () => {
    const modal = document.getElementById("createProfileModal");
    const openBtn = document.getElementById("openProfileModalBtn");

    const magBtn = document.getElementById("openMagModalBtn");
    const openMagModal = document.getElementById("createMagModal");

    // Sélectionner tous les boutons Cancel
    const cancelButtons = document.querySelectorAll(".cancelBtn");

    // modal update
    const updateModal = document.getElementById("updateProfileModal");
    const openUpdateBtns = document.querySelectorAll(".openUpdateProfileModalBtn");

    // modal alert / suppression
    const modalAlert = document.getElementById("openAlertParameter");
    const deleteProfilBtn = document.getElementById("deleteProfilBtn");                             

    function hideAllModals() {
        if (modal) hideModal(modal);
        if (updateModal) hideModal(updateModal);
        if (modalAlert) hideModal(modalAlert);
        if (openMagModal) hideModal(openMagModal);
    }

    // Ouvrir le modal principal
    if (openBtn && modal) {
        openBtn.addEventListener('click', () => {
            if (window.bootstrap && bootstrap.Modal) {
                bootstrap.Modal.getOrCreateInstance(modal).show();
            } else {
                modal.style.display = 'block';
                modal.classList.add('show');
                if (!document.querySelector('.modal-backdrop')) {
                    const backdrop = document.createElement('div');
                    backdrop.className = 'modal-backdrop fade show';
                    document.body.appendChild(backdrop);
                }
            }
        });
    }

    if (magBtn && openMagModal) {
        magBtn.addEventListener('click', () => { showModal(openMagModal); });
    }

    // Fermer avec les boutons cancel (tous)
    if (cancelButtons && cancelButtons.length) {
        cancelButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                hideAllModals();
            });
        });
    }

    // Fermer si clic à l'extérieur (handler unique)
    window.addEventListener('click', (event) => {
        const target = event.target;
        if (modal && target === modal) hideAllModals();
        if (updateModal && target === updateModal) hideAllModals();
        if (modalAlert && target === modalAlert) hideAllModals();
        if (openMagModal && target === openMagModal) hideAllModals();
    });

    // Suppression: écouteur sur le bouton delete (si présent)
    if (deleteProfilBtn) {
        deleteProfilBtn.addEventListener("click", function (event) {
            event.preventDefault();
            const id = this.getAttribute("data-id");
            fetch(`/Profil/DeleteProfil/${id}`, {
                method: "DELETE",
                credentials: 'same-origin',
                headers: {
                    "Content-Type": "application/json",
                    "X-Requested-With": "XMLHttpRequest"
                }
            })
                .then(response => {
                    if (!response.ok) throw new Error("Erreur lors de la suppression");
                    return response.json();
                })
                .then(result => {
                    if (result.success) {
                        showToast(result.message, "success");
                        if (modalAlert) hideModal(modalAlert);
                        setTimeout(() => { location.reload(); }, 1000);
                    } else {
                        showToast(result.message, "warning");
                    }
                })
                .catch(error => {
                    showToast("Erreur : " + error.message, "danger");
                });
        });
    }

    // Attacher un événement à chaque bouton "Modifier" pour ouvrir updateModal
    if (openUpdateBtns && openUpdateBtns.length) {
        openUpdateBtns.forEach(btn => {
            btn.addEventListener("click", function () {
                const id = this.getAttribute("data-id");

                fetch(`/Profil/GetProfil/${id}`, {
                    credentials: 'same-origin',
                    headers: {
                        "X-Requested-With": "XMLHttpRequest",
                        "Content-Type": "application/json"
                    }
                })
                    .then(response => {
                        if (response.status === 440 || response.status === 401) {
                            window.location.href = "/Auth/Login";
                            return;
                        }
                        if (!response.ok) {
                            throw new Error('Erreur réseau');
                        }
                        return response.json();
                    })
                    .then(result => {
                        if (!result) return;
                        if (!result.success) {
                            alert(result.message);
                            return;
                        }

                        const profil = result.data || {};

                        const setIfExists = (id, value) => {
                            const el = document.getElementById(id);
                            if (el) el.value = value ?? '';
                        };
                        setIfExists("profileId", profil.id);
                        setIfExists("Username", profil.username);
                        setIfExists("Userag", profil.userag);
                        setIfExists("TypeProfileString", mapProfil[profil.typeProfile] || "Inconnu");
                        setIfExists("Status", profil.status);
                        setIfExists("Email", profil.email);

                        // Afficher le modal de mise à jour via helper
                        if (updateModal) showModal(updateModal);
                    })
                    .catch(err => alert("Error : " + err.message));
            });
        });
    }
});


async function UpdateProfil(event) {
    event.preventDefault();
    toggleOnLoaderAndToast();

    let formData = new FormData(event.target);
    let data = Object.fromEntries(formData.entries());

    fetch("/Profil/UpdateProfil", {
        method: "POST",
        credentials: 'same-origin',
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify(data)
    })
        .then(response => {
            if (!response.ok) {
                toggleOffLoaderAndToast();
                throw new Error("Erreur réseau");
            }

            return response.json();
        })
        .then(result => {
            if (result.success) {

                toggleOffLoaderAndToast();

                showToast(result.message, "success");

                setTimeout(() => {
                    location.reload();
                }, 1000);

            } else {

                toggleOffLoaderAndToast();

                showToast(result.message, "warning");
            }
        })
        .catch(error => {

            toggleOffLoaderAndToast();

            showToast("Erreur : " + error.message, "danger");
        });
}

// Fonction appelée depuis le bouton du tableau pour le modal de confirmation de suppression
function showDeleteProfile(id) {
    const modalAlert = document.getElementById("openAlertParameter");
    const deleteProfilBtn = document.getElementById("deleteProfilBtn");
    if (!modalAlert || !deleteProfilBtn) return;

    // Stocker l'ID dans un attribut data-id
    deleteProfilBtn.setAttribute("data-id", id);

    // Afficher le modal
    modalAlert.style.display = "block";
}

// Expose function globally (necessary if called from markup)
window.showDeleteProfile = showDeleteProfile;



$("#updateProfileModal").on("submit", (event) => {
   
    UpdateProfil(event)
})
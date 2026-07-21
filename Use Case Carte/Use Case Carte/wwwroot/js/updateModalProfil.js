window.hideUpdateProfileModal = () => {

    const modalElement = document.getElementById("updateProfileModal");

    const modal = bootstrap.Modal.getInstance(modalElement);

    if (modal) {
        modal.hide();
    }

}
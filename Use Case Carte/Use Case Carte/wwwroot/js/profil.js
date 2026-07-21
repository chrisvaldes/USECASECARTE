function initProfileModals () {
  const modal = document.getElementById('createProfileModal')
  const openBtn = document.getElementById('openProfileModalBtn')
  if (openBtn && modal) {
    openBtn.addEventListener('click', () => {
      if (window.bootstrap && bootstrap.Modal) {
        bootstrap.Modal.getOrCreateInstance(modal).show()
      } else {
        modal.style.display = 'block'
        modal.classList.add('show')
        if (!document.querySelector('.modal-backdrop')) {
          const backdrop = document.createElement('div')
          backdrop.className = 'modal-backdrop fade show'
          document.body.appendChild(backdrop)
        }
      }
    })
  }
}
window.initProfileModals = initProfileModals

window.showUpdateProfileModal = () => {
    const modal = document.getElementById("updateProfileModal");

    bootstrap.Modal.getOrCreateInstance(modal).show();
};

document.addEventListener('click', function (e) {
  if (e.target.closest('.cancelBtn')) {
    const modalElement = document.querySelector('#createProfileModal')

    if (modalElement) {
      bootstrap.Modal.getOrCreateInstance(modalElement).hide()
    }
  }
})

 
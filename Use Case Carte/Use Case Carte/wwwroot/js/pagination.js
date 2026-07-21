document.addEventListener("DOMContentLoaded", function () {

    const rowsPerPage = 8;

    const searchInput = document.getElementById("searchInput");
    const clearSearch = document.getElementById("clearSearch");
    const resultCount = document.getElementById("resultCount");

    const prevPageBtn = document.getElementById("prevPage");
    const nextPageBtn = document.getElementById("nextPage");

    const pageNumbersContainer = document.getElementById("pageNumbers");

    // ⚠️ élément manquant dans ton code original
    const paginationInfo = document.getElementById("paginationInfo");

    const tableRows = document.querySelectorAll("#usersTable tbody tr:not(.empty-row)");

    // =========================
    // STOP SI ELEMENTS CRITIQUES ABSENTS
    // =========================
    //if (
    //    !searchInput ||
    //    !clearSearch ||
    //    !resultCount ||
    //    !prevPageBtn ||
    //    !nextPageBtn ||
    //    !pageNumbersContainer
    //) {
    //    console.error("Pagination.js : un ou plusieurs éléments HTML sont introuvables.");
    //    return;
    //}

    let currentPage = 1;
    let filteredRows = [...tableRows];

    // =========================
    // AFFICHAGE TABLE
    // =========================
    function displayTable() {

        const totalPages = Math.ceil(filteredRows.length / rowsPerPage);

        if (currentPage > totalPages && totalPages > 0) {
            currentPage = totalPages;
        }

        const start = (currentPage - 1) * rowsPerPage;
        const end = start + rowsPerPage;

        tableRows.forEach(row => {
            row.style.display = "none";
        });

        filteredRows.slice(start, end).forEach(row => {
            row.style.display = "";
        });

        updatePagination(totalPages);
    }

    // =========================
    // PAGINATION
    // =========================
    function updatePagination(totalPages) {

        pageNumbersContainer.innerHTML = "";
        pageNumbersContainer.className = "d-flex gap-2 mx-1";

        if (totalPages === 0) {

            if (paginationInfo) paginationInfo.innerText = "0 / 0";
            resultCount.innerText = "0 résultat trouvé";

            prevPageBtn.disabled = true;
            nextPageBtn.disabled = true;

            return;
        }

        function createPageButton(page) {
            const btn = document.createElement("button");
            btn.innerText = page;
            btn.className = "btn btn-sm page-number-btn";

            if (page === currentPage) {
                btn.classList.add("active-page");
            }

            btn.addEventListener("click", function () {
                currentPage = page;
                displayTable();
            });

            pageNumbersContainer.appendChild(btn);
        }

        createPageButton(1);

        if (currentPage > 3) {
            const dots = document.createElement("span");
            dots.innerText = "...";
            dots.className = "px-2";
            pageNumbersContainer.appendChild(dots);
        }

        for (
            let i = Math.max(2, currentPage - 1);
            i <= Math.min(totalPages - 1, currentPage + 1);
            i++
        ) {
            createPageButton(i);
        }

        if (currentPage < totalPages - 2) {
            const dots = document.createElement("span");
            dots.innerText = "...";
            dots.className = "px-2";
            pageNumbersContainer.appendChild(dots);
        }

        if (totalPages > 1) {
            createPageButton(totalPages);
        }

        prevPageBtn.disabled = currentPage === 1;
        nextPageBtn.disabled = currentPage === totalPages;

        resultCount.innerText = `${filteredRows.length} résultat(s) trouvé(s)`;

        if (paginationInfo) {
            paginationInfo.innerText = `${currentPage} / ${totalPages}`;
        }
    }

    // =========================
    // RECHERCHE
    // =========================
    searchInput.addEventListener("keyup", function () {

        const filter = this.value.toLowerCase();

        clearSearch.style.display = filter ? "block" : "none";

        filteredRows = [...tableRows].filter(row =>
            row.textContent.toLowerCase().includes(filter)
        );

        currentPage = 1;
        displayTable();
    });

    // =========================
    // CLEAR SEARCH
    // =========================
    clearSearch.addEventListener("click", function () {

        searchInput.value = "";
        clearSearch.style.display = "none";

        filteredRows = [...tableRows];
        currentPage = 1;

        displayTable();
        searchInput.focus();
    });

    // =========================
    // NEXT PAGE
    // =========================
    nextPageBtn.addEventListener("click", function () {

        const totalPages = Math.ceil(filteredRows.length / rowsPerPage);

        if (currentPage < totalPages) {
            currentPage++;
            displayTable();
        }
    });

    // =========================
    // PREV PAGE
    // =========================
    prevPageBtn.addEventListener("click", function () {

        if (currentPage > 1) {
            currentPage--;
            displayTable();
        }
    });

    // =========================
    // INIT
    // =========================
    displayTable();
});
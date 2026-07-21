async function DownloadCarteARegulerExcel(event) {
    try {
        event.preventDefault();

        toggleOnLoaderAndToast(); 
         

        const carteAReguler = document.getElementById("carteAReguler").value; 

        const response = await fetch("/ManqueAGagner/TelechargerCarteARegulerExcel", {
            method: "POST",

            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ typeMag: carteAReguler })
        });

        if (!response.ok) {
            throw new Error("Erreur lors du téléchargement du fichier Excel");
        }

        const blob = await response.blob();

        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");

        a.href = url;
        a.download = `BKMBTI_${new Date().toISOString().split("T")[0]}.xlsx`;

        document.body.appendChild(a);
        a.click();
        a.remove();

        window.URL.revokeObjectURL(url);

        toggleOffLoaderAndToast();
        showToast("Téléchargement réussi", "success");
    }
    catch (error) {
        toggleOffLoaderAndToast();
        showToast(`Erreur : ${error.message}`, "danger");
    }
}
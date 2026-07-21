window.downloadFile = (fileName, base64Data) => {

    const link = document.createElement('a');

    link.download = fileName;

    link.href = "data:application/octet-stream;base64," + base64Data;

    document.body.appendChild(link);

    link.click();

    document.body.removeChild(link);
};

window.downloadExcelFile = (fileName, base64Data) => {

    const byteCharacters = atob(base64Data);

    const byteNumbers = new Array(byteCharacters.length);

    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }

    const byteArray = new Uint8Array(byteNumbers);

    const blob = new Blob(
        [byteArray],
        {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        });

    const url = URL.createObjectURL(blob);

    const link = document.createElement("a");

    link.href = url;

    link.download = fileName;

    document.body.appendChild(link);

    link.click();

    document.body.removeChild(link);

    URL.revokeObjectURL(url);
};

function GetDetailMagClient(event) {
    event.preventDefault();

    const formData = new FormData(event.target);
    const data = Object.fromEntries(formData.entries());

    const url = `/CartesClient/GetAllCustomerBilling?ncpf=${encodeURIComponent(data.ncpf)}&debut=${data.debut}&fin=${data.fin || ""}`;

    toggleOnLoaderAndToast();

    window.location.href = url;
}

 

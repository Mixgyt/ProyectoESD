function addListeners(){
    const checkInInput = document.getElementById("check_in");
    const checkOutInput = document.getElementById("check_out");

    checkInInput.addEventListener("change", function () {
        const selectedDate = new Date(this.value);
        if (selectedDate) {
            // Sumar un día a la fecha seleccionada
            selectedDate.setDate(selectedDate.getDate() + 1);

            // Formatear la fecha en YYYY-MM-DD para el atributo min
            const minDate = selectedDate.toISOString().split("T")[0];

            // Establecer la fecha mínima en el campo de salida
            checkOutInput.min = minDate;

            // Si la fecha de salida actual es inválida, limpiar el valor
            if (checkOutInput.value && new Date(checkOutInput.value) < new Date(minDate)) {
                checkOutInput.value = "";
            }
        }
    });   
}

document.addEventListener("DOMContentLoaded", addListeners);
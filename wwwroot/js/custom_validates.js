function addListeners() {
    const restricted = document.querySelector("#positive-numbers")
    restricted.addEventListener("keyup", event => {
        const value = event.target.value;
        if (!/^\d+(\.\d{0,2})?$/.test(value) && value !== '') {
            event.target.value = event.target.getAttribute("data-value");
        } else {
            event.target.setAttribute("data-value", value);
        }
    });
}

document.addEventListener("DOMContentLoaded", addListeners);
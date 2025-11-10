/** @param {HTMLInputElement} input*/
const leadingZeros = (input) => {
    if (!isNaN(parseInt(input.value)) && input.value.length === 1) {
        input.value = '0' + input.value;
    }
}

export {
    leadingZeros
}
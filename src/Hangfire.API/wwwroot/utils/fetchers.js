/** @param {string} stylesheet */
const fetchStyle = async (stylesheet) => {
    const response = await fetch(stylesheet);
    const cssStyle = await response.text();
    return cssStyle;
}

export {
    fetchStyle
}
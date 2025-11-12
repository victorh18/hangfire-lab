import { TimePartInput, TimeInput, DropDown, AppButton, AppNavbar } from "./components/index.js"

customElements.define('time-part-input', TimePartInput);
customElements.define('time-input', TimeInput);
customElements.define('dropdown-select', DropDown);
customElements.define('app-button', AppButton);
customElements.define('app-navbar', AppNavbar);

const SHOW_DRAWER_CLASS = 'show-drawer';

btnToggleDrawer.addEventListener('click', () => {
    //alert('hi mom!'); 
    const drawer = document.getElementById("drawer");
    const backdrop = document.getElementById('backdrop');
    if (drawer.classList.contains(SHOW_DRAWER_CLASS)) {
        drawer.classList.remove(SHOW_DRAWER_CLASS);
        backdrop.style.display = 'none'
    }
    else {
        drawer.classList.add(SHOW_DRAWER_CLASS);
        backdrop.style.display = 'block'
    }
    
})

backdrop.addEventListener('click', () => {
    const drawer = document.getElementById("drawer");
    const backdrop = document.getElementById('backdrop');
    drawer.classList.remove(SHOW_DRAWER_CLASS);
    backdrop.style.display = 'none'
})


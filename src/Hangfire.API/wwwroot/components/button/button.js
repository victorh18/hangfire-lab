const sheet = new CSSStyleSheet();
sheet.replaceSync(/*css*/`
    button {
        display: flex;
        justify-content: center;
        align-items: center;
        padding: 1rem 1rem;
        width: auto;
        height: 2rem;
        border-radius: .5rem;
        background-color: #4f46e5;
        border-color: #4f46e5;
        font-weight: bold;
        font-size: 1rem;
        font-family: 'Space Grotesk', sans-serif;
        color: white; 
        cursor: pointer;
        flex: 1 0 100%;
    }

    .button-full {
        width: 100%;
    }

    .button-secondary {
        background-color: #00b3ff;
        border-color: #00b3ff;
    }

    .button-secondary:hover {
        background-color: #38bdf8;
    }

    button:hover {
        background-color: #5b52ff;
    }

    @media (max-width: 500px) {
        .button-sm-icon-only #buttonLabel {
            display: none;
        }

        .button-sm-size-full {
            flex: 1 0 100%;
            margin-top: 1rem;
        }
    }
`);

export class AppButton extends HTMLElement {
    shadow = this.attachShadow({ mode: 'open' });

    get markup() {
        const _class = this.size === "full" ? " button-full " : "";
        const _variant = this.variant === 'secondary' ? ' button-secondary ' : "";
        const _smMode = this.smLabelMode === 'iconOnly' ? 'button-sm-icon-only' : ""
        const _smSize = this.smSize === 'full' ? 'button-sm-size-full' : ""

        const iconEl = this.getIconElement(this.icon);
        return /*html*/`
            <button id="${this.id}" class="${_class} ${_variant} ${_smMode} ${_smSize}">
                ${iconEl ?? ''}
                <span id="buttonLabel" class="truncate">${this.label}</span>
            </button>
        `;
    }

    constructor() {
        super();
        this.shadow.adoptedStyleSheets = [sheet]
        this.shadow.innerHTML = this.markup;
    }

    getIconElement(iconName) {
        if (!iconName)
            return null;

        const spanEl = document.createElement('span');
        spanEl.id = 'buttonIcon';
        spanEl.style.padding = '0rem .5rem 0rem 0.2rem' 
        spanEl.innerText = iconName;
        return spanEl.outerHTML;
    }

    get label() {
        return this.getAttribute('label') || '';
    }

    get icon() {
        return this.getAttribute('icon') || ''
    }

    get id() {
        return this.getAttribute('id') || ''
    }

    get variant() {
        return this.getAttribute('variant') || ''
    }

    get size() {
        return this.getAttribute('size') || '';
    }

    get smLabelMode() {
        return this.getAttribute("smLabelMode") || '';
    }

    get smSize() {
        return this.getAttribute("smSize") || '';
    }

}
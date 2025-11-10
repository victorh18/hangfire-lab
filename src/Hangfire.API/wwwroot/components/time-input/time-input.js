const sheet = new CSSStyleSheet();
sheet.replaceSync(/*css*/`
    .container {
        display: inline-flex;
        flex-direction: column;
        align-items: flex-start;
        /* gap: 1rem; */
    }

    .input-container {
        border: 1px solid #374151;
        border-radius: 0.5rem;
        overflow: hidden;
        display: flex;
        flex-direction: row;
        align-items: center;
        background-color: #1f2937;
        padding: .25rem .5rem;
    }

    .input-container:active,
    .input-container:focus-within {
        outline-width: 2px;
        outline-style: solid; 
        outline-offset: -1px;
        outline-color: #2563eb;
    }

    span {
        font-size: 1rem
    }
`);

class TimeInput extends HTMLElement {
    shadow = this.attachShadow({ mode: 'open' });

    get _markup() {
        const iconEl = this.getIconElement(this.iconName);

        return /*html*/`
            <label>${this.title}</label>
            <div class="input-container">
                ${iconEl ?? ''}
                <time-part-input id="mmInput"></time-part-input>:
                <time-part-input id="ssInput"></time-part-input>.
                <time-part-input id="msInput" maxValue="9" placeholder="0"></time-part-input>
            <div>
        `
    }

    get title() {
        return this.getAttribute('title') || ''
    }

    get mmInput() {
        return this.shadow.querySelector('#mmInput');
    }
    get ssInput() {
        return this.shadow.querySelector('#ssInput');
    }
    get msInput() {
        return this.shadow.querySelector('#msInput');
    }

    get timeValue() {
        return `${this.mmInput?.value}:${this.ssInput?.value}.${this.msInput?.value}`
    }

    get iconName() {
        return this.getAttribute('iconName') || '';
    }

    getIconElement(iconName) {
        if (!iconName)
            return null;

        const spanEl = document.createElement('span');
        spanEl.style.padding = '0rem .5rem 0rem 0.2rem' 
        spanEl.innerText = iconName;
        spanEl.className = 'material-symbols-outlined text-gray-400 pointer-events-none';
        return spanEl.outerHTML;
    }

    async setup() {
        this.shadow.adoptedStyleSheets = [sheet]
        const timeInputContainer = document.createElement('div');
        timeInputContainer.classList.add("container");

        timeInputContainer.addEventListener('click', () => { console.log(this.timeValue) })

        timeInputContainer.innerHTML = this._markup;
        this.shadow.appendChild(timeInputContainer);
    }

    constructor() {
        super();
        this.setup();
    }
}

export {
    TimeInput
}

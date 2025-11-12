const sheet = new CSSStyleSheet();
sheet.replaceSync(/*css*/`
        div {
            display: flex;
            flex-direction: column;
        }

        select {
            background-color: #1f2937;
            border: 1px solid #374151;
            color: white;
            font-family: 'Space Grotesk', sans-serif;
            padding: .5rem;
            border-radius: .5rem;
            padding-right: 4rem;
            width: fit-content;

            -webkit-appearance: none; /* For Webkit browsers like Chrome, Safari */
            -moz-appearance: none;    /* For Firefox */
            appearance: none;

            background-image: url('data:image/svg+xml;utf8,<svg fill="white" height="24" viewBox="0 0 24 24" width="24" xmlns="http://www.w3.org/2000/svg"><path d="M7 10l5 5 5-5z"/><path d="M0 0h24v24H0z" fill="none"/></svg>');
            background-repeat: no-repeat;
            background-position: right 0px center;
        }

        select:focus,
        select:active {
            outline-width: 2px;
            outline-style: solid; 
            outline-offset: -1px;
            outline-color: #2563eb;
        }
    `);

export class DropDown extends HTMLElement {
    shadow = this.attachShadow({ mode: 'open' });
    get markup() {
        return /*html*/`
            <label for="drpExtractionType">${this.title}</label>
            <select class="" name="extractionType" id="drpExtractionType">
                ${this.children}
            </select>
        `
    }

    constructor() {
        super();
        this.shadow.adoptedStyleSheets = [sheet]
        const containerEl = document.createElement('div');
        containerEl.innerHTML = this.markup;
        this.shadow.appendChild(containerEl);
    }

    get title() {
        return this.getAttribute("title") || "";
    };

    get children() {
        return this.innerHTML;
    }

    get className() {
        return this.getAttribute("class") || "";
    }

}
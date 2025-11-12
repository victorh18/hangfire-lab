'use strict'
const sheet = new CSSStyleSheet();
sheet.replaceSync(/*css*/`
    input::-webkit-outer-spin-button,
    input::-webkit-inner-spin-button {
        -webkit-appearance: none;
        margin: 0;
    }

    input:focus-visible {
        border: none;
        outline: none
    }

    input {
        text-align: right;
        font-size: 1rem;
        width: 1.3rem;
        border-width: 0px;
        background-color: #1f2937;
        color: white;
        font-family: 'Space Grotesk', sans-serif;

    }

    .input-single {
        text-align: left;
        width: .7rem;
    }

    .underlined {
        text-decoration: underline;
    }

`);

import { leadingZeros } from "../../utils/strings.js";

class TimePartInput extends HTMLElement {
    shadow = this.attachShadow({ mode: 'open' });
    timePartInputElement = document.createElement('input');
    constructor() {
        super();
        this.shadow.adoptedStyleSheets = [sheet];
        this.timePartInputElement = document.createElement('input');

        if (this.placeholder.length > 1){
            this.timePartInputElement.addEventListener('change', () => leadingZeros(this.timePartInputElement))
        } else {
            this.timePartInputElement.className = 'input-single';
        }

        this.handleFocus(this.timePartInputElement);
        this.setupDefaults(this.timePartInputElement);
        this.addEventListener('keydown', this.keyDownHandler)

        this.shadow.appendChild(this.timePartInputElement);
    }

    get minValue() {
        return parseInt(this.getAttribute('minValue') || "") || 0;
    }

    get maxValue() {
        return parseInt(this.getAttribute('maxValue') || "") || 59;
    }

    get placeholder() {
        return this.getAttribute('placeholder') || "00";
    }

    get value() {
        return this.timePartInputElement.value || "00";
    }

    /** @param {HTMLInputElement} inputElement */
    setupDefaults(inputElement) {
        inputElement.type = 'number'
        inputElement.maxLength = this.placeholder.length;
        inputElement.placeholder = this.placeholder
        inputElement.max = this.maxValue.toString();
        inputElement.min = this.minValue.toString();
    }

    /**
     * @param {KeyboardEvent} event
     */
    keyDownHandler(event) {
        const siblings = Array.from(event.target?.parentNode?.querySelectorAll("time-part-input"));
        const index = siblings.indexOf(event.target);

        let newIndex = 0;
        switch (event.key) {
            case "ArrowRight":
                newIndex = (index + 1) % 3;
                break;
            case "ArrowLeft":
                newIndex = ((index || 3) - 1) % 3;
                break;
            default:
                return;
        }

        const currentInputElement = siblings[index].shadowRoot.children[0];
        currentInputElement.classList.remove('underlined')

        const nextInputElement = siblings[newIndex].shadowRoot.children[0]
        nextInputElement.classList.add('underlined')
        nextInputElement.focus()
    }

    /**
     * @param {HTMLInputElement} inputElement
     */
    handleFocus(inputElement) {
        inputElement.addEventListener('focus', (event) => {
            event.target.classList.add('underlined');
        });
        inputElement.addEventListener('blur', (event) => {
            event.target.classList.remove('underlined')
            if (Number(event.target.value) > this.maxValue) {
                this.timePartInputElement.value = this.maxValue
            }

            if (Number(event.target.value) < this.minValue) {
                this.timePartInputElement.value = this.minValue
            }
        })
    }
}

export {
    TimePartInput
}



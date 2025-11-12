const styles = new CSSStyleSheet();
styles.replaceSync(/*css*/`
        nav {
            display: flex;
            justify-content: center;
            gap: 2rem;
            font-family: 'Space Grotesk', sans-serif;  
            width: 100%;
        }

        a:hover {
            color: #00b3ff;
            cursor: pointer;
            font-weight: bold;
        }
    `);

export class AppNavbar extends HTMLElement {
    shadow = this.attachShadow({ mode: 'open' });
    get markup() {
        return /*html*/`
            <nav>
               ${this.children}
            </nav>
        `;
    }

    constructor() {
        super();
        this.shadow.adoptedStyleSheets = [styles];
        this.shadow.innerHTML = this.markup;
    }

    get children() {
        return this.innerHTML;
    }
}
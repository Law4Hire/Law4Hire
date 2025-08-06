// DEBUG FLUENT DESIGN TOKENS - Run this in browser console
console.log("=== FLUENT DESIGN TOKENS DEBUG ===");

// Check current theme mode
const fluentTheme = document.querySelector('fluent-design-theme');
if (fluentTheme) {
    console.log("FluentDesignTheme mode:", fluentTheme.getAttribute('mode'));
}

// Check data-theme attributes
console.log("HTML data-theme:", document.documentElement.getAttribute('data-theme'));
console.log("Body data-theme:", document.body.getAttribute('data-theme'));

// Check computed CSS custom properties
const rootStyles = getComputedStyle(document.documentElement);
const bodyStyles = getComputedStyle(document.body);

console.log("\n--- CSS Custom Properties ---");
console.log("--primary-color:", rootStyles.getPropertyValue('--primary-color'));
console.log("--secondary-color:", rootStyles.getPropertyValue('--secondary-color'));
console.log("--accent-color:", rootStyles.getPropertyValue('--accent-color'));
console.log("--background-color:", rootStyles.getPropertyValue('--background-color'));
console.log("--text-color:", rootStyles.getPropertyValue('--text-color'));

console.log("\n--- Fluent Design Tokens ---");
console.log("--neutral-layer-1:", rootStyles.getPropertyValue('--neutral-layer-1'));
console.log("--neutral-foreground-rest:", rootStyles.getPropertyValue('--neutral-foreground-rest'));
console.log("--accent-fill-rest:", rootStyles.getPropertyValue('--accent-fill-rest'));
console.log("--accent-fill-hover:", rootStyles.getPropertyValue('--accent-fill-hover'));

// Check header element specifically
const header = document.querySelector('fluent-header');
if (header) {
    const headerStyles = getComputedStyle(header);
    console.log("\n--- FluentHeader Computed Styles ---");
    console.log("Background:", headerStyles.backgroundColor);
    console.log("Color:", headerStyles.color);
    console.log("--accent-fill-rest on header:", headerStyles.getPropertyValue('--accent-fill-rest'));
    
    // Check all CSS custom properties on the header
    console.log("\n--- All Custom Properties on Header ---");
    const allProps = [];
    for (let i = 0; i < headerStyles.length; i++) {
        const prop = headerStyles[i];
        if (prop.startsWith('--')) {
            allProps.push(`${prop}: ${headerStyles.getPropertyValue(prop)}`);
        }
    }
    console.log(allProps);
}

// Check if there are any CSS rules overriding the header
console.log("\n--- CSS Rules Analysis ---");
const sheets = Array.from(document.styleSheets);
sheets.forEach((sheet, i) => {
    try {
        const rules = Array.from(sheet.cssRules || sheet.rules || []);
        rules.forEach(rule => {
            if (rule.selectorText && (
                rule.selectorText.includes('fluent-header') ||
                rule.selectorText.includes('header') ||
                rule.selectorText.includes('--accent-fill-rest')
            )) {
                console.log(`Sheet ${i}: ${rule.selectorText} -> ${rule.cssText}`);
            }
        });
    } catch (e) {
        console.log(`Sheet ${i}: Cannot access (CORS)`);
    }
});

console.log("\n--- Manual Token Override Test ---");
console.log("Setting --accent-fill-rest to pure dark blue...");

// Try to manually override the token
document.documentElement.style.setProperty('--accent-fill-rest', '#1e3a8a');
if (header) {
    header.style.backgroundColor = '#1e3a8a';
}

setTimeout(() => {
    const newHeaderStyles = getComputedStyle(header);
    console.log("After manual override:");
    console.log("Header background:", newHeaderStyles.backgroundColor);
}, 100);
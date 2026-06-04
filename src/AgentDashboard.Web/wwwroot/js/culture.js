// Culture persistence for Blazor application
const blazorCulture = {
    set: (culture) => {
        localStorage.setItem('blazor-culture', culture);
    },
    get: () => {
        return localStorage.getItem('blazor-culture');
    },
    remove: () => {
        localStorage.removeItem('blazor-culture');
    }
};

// Export to global scope for Blazor JS interop
window.blazorCulture = blazorCulture;

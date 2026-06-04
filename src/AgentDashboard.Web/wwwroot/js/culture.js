// Culture persistence for Blazor application
// Cookie name must match CookieRequestCultureProvider.DefaultCookieName
const COOKIE_NAME = 'ASPNETCORE_CULTURE';

const blazorCulture = {
    set: (culture) => {
        // Set localStorage for client-side persistence
        localStorage.setItem('blazor-culture', culture);
        
        // Set cookie for server-side detection
        // Format: c=<culture>|uic=<culture>
        const cookieValue = `c=${encodeURIComponent(culture)}|uic=${encodeURIComponent(culture)}`;
        const expires = new Date();
        expires.setFullYear(expires.getFullYear() + 1);
        document.cookie = `${COOKIE_NAME}=${cookieValue}; expires=${expires.toUTCString()}; path=/; SameSite=Lax; Secure`;
    },
    get: () => {
        return localStorage.getItem('blazor-culture');
    },
    remove: () => {
        localStorage.removeItem('blazor-culture');
        document.cookie = `${COOKIE_NAME}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/`;
    }
};

// Export to global scope for Blazor JS interop
window.blazorCulture = blazorCulture;

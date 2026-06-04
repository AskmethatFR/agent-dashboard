---
id: "localization"
type: "technical"
owner: "architect"
status: "current"
updated: "2026-06-04"
links:
  - "architecture-overview"
answers:
  - "How is localization implemented in Agent Dashboard?"
  - "Which package is used for JSON-based localization?"
  - "How to add new translations?"
  - "How to add a new language?"
decided_in:
  - "#17"
---

# Localization (i18n) Implementation

> **One-liner**: Agent Dashboard uses AspNetCore.Localizer.Json package for JSON-based localization, supporting multiple languages with Blazor Server.
> **Links**: [[architecture-overview]]
> **Issue**: #17

## Context

Agent Dashboard is a multi-agent orchestration cockpit that needs to support multiple languages for international users. The application uses **AspNetCore.Localizer.Json** package (v1.0.4+) to provide JSON-based localization instead of traditional .resx files.

## Decision / Specification

### Package Choice

**Selected**: [AspNetCore.Localizer.Json](https://github.com/AskmethatFR/AspNetCore.Localizer.Json)

**Rationale**:
- JSON-based localization (easier to edit and version control)
- Supports both embedded resources and physical files
- Compatible with .NET 8/9/10
- Built-in Blazor Server support with `GetHtmlBlazorString()`
- Configurable caching and performance optimizations
- Missing translation collection and logging
- Two localization modes: Basic and I18n

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                      User Request                            │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                   CultureMiddleware                           │
│  - Detects culture from query string (?culture=en-US)        │
│  - Falls back to cookie                                        │
│  - Sets CultureInfo.CurrentUICulture                          │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                 RequestLocalizationMiddleware                 │
│  - Validates and sets request culture                         │
│  - Provides fallback to default culture (en-US)              │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                      Component Tree                           │
│  - Components inject IJsonStringLocalizer<T>                  │
│  - Localizer looks up translations from JSON files           │
│  - Falls back to default culture if not found                 │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                      JSON Files                                │
│  - Located in /i18n/ directory                                │
│  - Named: [FullTypeName].[culture].json                       │
│  - Example: AgentDashboard.Web.Components.Pages.Home.fr-FR.json│
└─────────────────────────────────────────────────────────────┘
```

### Supported Cultures

- **en-US** (English - United States) - Default
- **fr-FR** (French - France)

## Configuration

### 1. Service Registration (Program.cs)

```csharp
// Add JSON localization services
builder.Services.AddJsonLocalization(options =>
{
    options.ResourcesPath = "i18n";
    options.UseEmbeddedResources = false;  // Use physical files
    options.SupportedCultureInfos = new HashSet<CultureInfo>
    {
        new CultureInfo("en-US"),
        new CultureInfo("fr-FR")
    };
    options.LocalizationMode = LocalizationMode.I18n;
    options.CacheDuration = TimeSpan.FromMinutes(30);
    options.FileEncoding = Encoding.UTF8;
    options.IgnoreJsonErrors = false;  // Security requirement - fail on malformed JSON
});

// Configure request localization
var supportedCultures = new[] { "en-US", "fr-FR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// Configure culture middleware (before UseRequestLocalization)
app.UseMiddleware<CultureMiddleware>();
```

### 2. File Structure

```
src/AgentDashboard.Web/
├── i18n/
│   ├── shared.en-US.json           # Shared translations (contains keys for Home, NavMenu, TicketCard)
│   ├── shared.fr-FR.json           # Traductions partagées
│   ├── AgentDashboard.Web.Components.Pages.Error.en-US.json
│   ├── AgentDashboard.Web.Components.Pages.Error.fr-FR.json
│   ├── AgentDashboard.Web.Components.Pages.NotFound.en-US.json
│   ├── AgentDashboard.Web.Components.Pages.NotFound.fr-FR.json
│   ├── AgentDashboard.Web.Components.Shared.RetryCounter.en-US.json
│   └── AgentDashboard.Web.Components.Shared.RetryCounter.fr-FR.json
└── Middleware/
    └── CultureMiddleware.cs        # Culture detection middleware
```

> **Note**: Home, NavMenu, TicketCard, and AgeBadge components use `IJsonStringLocalizer<T>` which falls back to `shared.json` for their translation keys. Component-specific JSON files were deleted to avoid duplicate keys.
=======

### 3. CSProject Configuration

```xml
<!-- Copy JSON files to output directory -->
<ItemGroup>
    <Content Update="i18n\**\*.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
</ItemGroup>
```

## Component Integration

### Basic Usage (Typed Localizer)

```razor
@page "/"
@inject IJsonStringLocalizer<Home> Localizer

<h1>@Localizer["Welcome"]</h1>
<p>@Localizer["Description"]</p>
```

**Corresponding JSON file**: `shared.en-US.json` (Home component uses `IJsonStringLocalizer<Home>` which falls back to shared.json)

```json
{
  "Welcome": "Welcome to Agent Dashboard",
  "Description": "An orchestration cockpit for your agent team."
}
```

### With Parameters

```razor
@inject IJsonStringLocalizer<RetryCounter> Localizer

@code {
    private string GetAriaLabel()
    {
        var current = 3;
        var max = 5;
        return string.Format(Localizer["RetryCount"], current, max);
    }
}
```

**JSON**:
```json
{
  "RetryCount": "Retry count: {0} of {1}"
}
```

### Shared Translations

For translations used across multiple components, use the `shared` files:

```razor
@inject IJsonStringLocalizer<Shared> Localizer

<button>@Localizer["Filter"]</button>
```

**JSON**: `shared.en-US.json`
```json
{
  "Filter": "Filter",
  "Loading": "Loading...",
  "NoDataAvailable": "No data available"
}
```

## Culture Switching

### CultureSelector Component

The `CultureSelector` component provides a UI for users to switch between supported cultures:

```razor
@using AgentDashboard.Web.Components.Shared

<CultureSelector />
```

**Features**:
- Displays buttons for each supported culture
- Shows the current culture as active
- Persists selection in localStorage
- Reloads the page to apply culture change

### CultureMiddleware

Handles culture detection from:
1. Query string parameter (`?culture=fr-FR`)
2. Cookie (set after first selection)
3. Falls back to default culture (en-US)

**Location**: `src/AgentDashboard.Web/Middleware/CultureMiddleware.cs`

## Adding New Translations

### For an Existing Component

1. **Identify the component type**: e.g., `Home`, `NavMenu`, `TicketCard`
2. **Create or update JSON file**: 
   - File name: `[FullTypeName].[culture].json`
   - For **shared keys** (used by multiple components): Use `shared.[culture].json`
   - For **component-specific keys**: Use `[FullTypeName].[culture].json` (e.g., `AgentDashboard.Web.Components.Pages.Error.en-US.json`)
3. **Add translation keys**:
   ```json
   {
     "NewKey": "Translation in English"
   }
   ```
4. **Use in component**:
   ```razor
   @inject IJsonStringLocalizer<Home> Localizer
   <span>@Localizer["NewKey"]</span>
   ```

### For Shared Translations

1. **Edit `shared.en-US.json` and `shared.fr-FR.json`**
2. **Add the key to both files**
3. **Use in any component**:
   ```razor
   @inject IJsonStringLocalizer<Shared> Localizer
   <span>@Localizer["SharedKey"]</span>
   ```

## Adding a New Language

1. **Add to supported cultures** (Program.cs):
   ```csharp
   options.SupportedCultureInfos = new HashSet<CultureInfo>
   {
       new CultureInfo("en-US"),
       new CultureInfo("fr-FR"),
       new CultureInfo("es-ES")  // Add new language
   };
   ```

2. **Add to request localization options**:
   ```csharp
   var supportedCultures = new[] { "en-US", "fr-FR", "es-ES" };
   ```

3. **Create JSON files** for each component:
   - `shared.es-ES.json`
   - `AgentDashboard.Web.Components.Pages.Home.es-ES.json`
   - etc.

4. **Update CultureSelector** (optional):
   ```csharp
   private readonly CultureInfo[] SupportedCultures = [
       new CultureInfo("en-US"),
       new CultureInfo("fr-FR"),
       new CultureInfo("es-ES")
   ];
   ```

## Performance Considerations

- **Caching**: Translations are cached for 30 minutes by default
- **Cache size**: Configurable via `CacheMaxSize` option
- **Cache clearing**: Use `localizer.ClearMemCache()` to manually clear cache
- **Embedded vs Files**: Physical files are easier to edit but require file system access

## Troubleshooting

### Missing Translations

If a translation key is not found:
1. Verify the JSON file exists in the output directory
2. Check the file name matches the component type exactly
3. Ensure the culture is in supported cultures
4. Check `IgnoreJsonErrors = false` is set for security

### Culture Not Switching

1. Verify CultureMiddleware is registered before UseRequestLocalization
2. Check browser cookies for the culture cookie
3. Verify the culture parameter is passed correctly
4. Ensure localStorage is not blocked by browser settings

### Build Errors

- **"IJsonStringLocalizer<T> not found"**: Ensure `@using AspNetCore.Localizer.Json.Localizer` is present
- **"AddJsonLocalization not found"**: Ensure `using AspNetCore.Localizer.Json.Extensions;` is present
- **"LocalizationMode not found"**: Ensure `using AspNetCore.Localizer.Json.JsonOptions;` is present

## Best Practices

1. **Use typed localizers** (`IJsonStringLocalizer<T>`) for component-specific translations
2. **Use shared localizer** (`IJsonStringLocalizer<Shared>`) for common translations
3. **Keep JSON files synchronized** across cultures
4. **Test with missing translations** to ensure graceful fallback
5. **Use parameterized strings** for dynamic content
6. **Name translation keys consistently** (PascalCase recommended)

## Files Modified for Issue #17

- `src/AgentDashboard.Web/Program.cs` - Service configuration
- `src/AgentDashboard.Web/AgentDashboard.Web.csproj` - File copy configuration
- `src/AgentDashboard.Web/Components/_Imports.razor` - Namespace imports
- `src/AgentDashboard.Web/Components/App.razor` - Language attribute
- `src/AgentDashboard.Web/Components/Shared/LocalizedComponentBase.razor` - **DELETED** (dead code, replaced by shared.json fallback)
- `src/AgentDashboard.Web/Components/Shared/CultureSelector.razor` - Culture switching
- `src/AgentDashboard.Web/Components/Shared/RetryCounter.razor` - Localized aria-labels
- `src/AgentDashboard.Web/Components/Board/TicketCard.razor` - Localized text
- `src/AgentDashboard.Web/Components/Layout/TopBar/NavMenu.razor` - Localized menu
- `src/AgentDashboard.Web/Components/Layout/TopBar/TopBar.razor` - CultureSelector integration
- `src/AgentDashboard.Web/Components/Pages/Home.razor` - Localized UI
- `src/AgentDashboard.Web/Components/Pages/Error.razor` - Localized errors
- `src/AgentDashboard.Web/Components/Pages/NotFound.razor` - Localized not found
- `src/AgentDashboard.Web/Middleware/CultureMiddleware.cs` - Culture detection
- `src/AgentDashboard.Web/wwwroot/js/culture.js` - Culture persistence
- `src/AgentDashboard.Web/i18n/*.json` - All translation files

## References

- [AspNetCore.Localizer.Json GitHub](https://github.com/AskmethatFR/AspNetCore.Localizer.Json)
- [NuGet Package](https://www.nuget.org/packages/AspNetCore.Localizer.Json)
- [Microsoft Localization Documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization)

# WhereGass - Architectural Pointers & Cleanup Log

This document tracks the surgical cleanups and architectural decisions made by Gemini CLI to ensure a lean, robust, and high-performance codebase.

## Architectural Pointers

### 1. Unified Map Initialization
- **Problem**: The map page would previously "hang" while waiting for geolocation, leading to a poor user experience.
- **Solution**: The map and sidebar now initialize **immediately** with station data. Geolocation is triggered in parallel. Once coordinates are received, the UI updates with distance calculations and proximity sorting.
- **File**: `wwwroot/js/map.js`

### 2. Service Worker Optimization
- **Problem**: The Service Worker was attempting to cache non-HTTP requests (like Chrome extensions) and failing on navigation requests without a network fallback.
- **Solution**: Implemented protocol filtering and a robust "Network-First" strategy for page navigations to ensure reliability on unstable connections.
- **File**: `wwwroot/sw.js`

### 3. Clean Landing Page
- **Problem**: The root URL (`/`) defaulted to a generic ASP.NET welcome page.
- **Solution**: Redirected `HomeController.Index` to `StationsController.Index` to provide immediate value (the gas station list) to the user.
- **File**: `Controllers/HomeController.cs`

### 4. Shared Reporting Logic
- **Problem**: Inconsistent handling of reporting steps and tutorial logic between different views.
- **Solution**: Moved global handlers like `showTutorial` and authentication state detection to `site.js` to keep the Layout file clean and declarative.
- **File**: `wwwroot/js/site.js`, `Views/Shared/_Layout.cshtml`

### 5. Consolidated Branding
- **Problem**: Multiple CSS blocks for brand logos and inconsistent logo resolution.
- **Solution**: Standardized on `Station.ResolveLogoPath` in the backend and mapped the UI to use a unified border-ring system for availability status.
- **File**: `Models/Station.cs`, `Views/Stations/Index.cshtml`

## Cleanup Log (2026-04-15)

| Task | Action | Result |
| :--- | :--- | :--- |
| **Bloat Removal** | Removed inline scripts from `_Layout.cshtml`. | Leaner HTML, better script separation. |
| **Logic Cleanup** | Replaced complex brand CSS with status-based rings. | Improved maintainability and UI clarity. |
| **Functionality Fix** | Fixed `maxZoom` error in Leaflet initialization. | Resolved JS crash on map page. |
| **PWA Hardening** | Added protocol checks to `sw.js` fetch listener. | Eliminated console errors from browser extensions. |
| **UX Improvement** | Added "Immediate Load" to map update cycles. | Zero-latency map interaction. |

## Maintenance Notes
- **Secrets**: Avoid placing API keys or production connection strings in `appsettings.json`. Use environment variables in the deployment pipeline.
- **Logo Assets**: New brand logos should be added to `wwwroot/images/logos/` and mapped in `Station.ResolveLogoPath`.

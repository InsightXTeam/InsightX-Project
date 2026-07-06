# InsightX Architecture (Report Upload & AI Module - Isolated)

This document describes the architecture of the **Report Upload & AI Processing Module**, which has been isolated on this branch (`feature/report-upload`).

## 🧱 Backend Architecture (`/BackEnd`)
The backend is built with **.NET 8 Web API** following Clean Architecture principles.

### Layers:
1. **InsightX.API**: Presentation layer containing the `ReportsController`. Handles HTTP requests for file uploads and status checks. Auth middleware is bypassed for isolated testing.
2. **InsightX.Application**: Business logic layer containing DTOs, interfaces, and CQRS commands/queries for report processing.
3. **InsightX.Domain**: Core domain entities such as `Report`, `KPI`, and core enums.
4. **InsightX.Infrastructure**: Implementation of persistence (Entity Framework Core) and AI services. Contains the `DocumentProcessorService` for OCR, text extraction, and Semantic Kernel integrations.

## 🎛️ Frontend Architecture (`/FontEnd/InsightX`)
The frontend is a modern Angular application featuring standalone components and modular routing.

### Structure:
1. **Core Module (`src/app/core`)**: Contains mocked `auth.service.ts` to allow direct access to the Reports dashboard without needing a backend JWT token.
2. **Features Module (`src/app/features`)**:
   - **`reports/`**: The core feature of this branch. Handles the UI for uploading reports, tracking extraction status, and viewing parsed metrics.
   - *(Note: Folders for auth, departments, users, kpis, and admin are preserved as empty `.gitkeep` directories for future module integration).*
3. **Shared Module (`src/app/shared`)**:
   - **`components/sidenavbar/`**: The main responsive sidebar navigation widget, scoped down to only display the Reports navigation item.

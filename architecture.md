
  ## 🎛️ Frontend Architecture Details (`/FontEnd/InsightX`)

  The frontend is a modern Angular application featuring standalone components and modular, lazy-loaded routing structures organized by features.

  ### 1. Core Module (`src/app/core`)
  Contains global singleton configurations, security middleware, and auth guards.
  * **`interceptors/auth.interceptor.ts`**: Intercepts HTTP requests and attaches the `Authorization: Bearer <JWT_TOKEN>` header if authenticated. Handles automatic 401 token refresh.
  * **`guards/auth.guard.ts`**: Restricts route transitions based on authentication states and role permissions.
  * **`services/auth.service.ts`**: Manages logging in, logging out, refreshing tokens, and holds session state.

  ### 2. Features Module (`src/app/features`)
  Organized around dedicated functional domains containing components, styling, and child route definitions:
  * **`auth/`**: Entry logic (Login, Signup/Register, onboarding stepper flows).
  * **`departments/`**: View/add/edit/delete divisions inside the company.
  * **`kpis/`**: Configure, update, and manage target KPIs.
  * **`users/`**: View/invite/delete manager team members.
  * **`profile/`**: User account credentials and security password change forms.

  ### 3. Shared Module (`src/app/shared`)
  * **`components/sidenavbar/`**: The main responsive sidebar navigation widget wrapping all authenticated dashboard features.

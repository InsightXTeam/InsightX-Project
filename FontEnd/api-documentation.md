# InsightX API Documentation

Welcome to the InsightX Backend API documentation. This document details the routing, endpoints, request/response models, authorization requirements, and roles for the InsightX API.

---

## 🔑 Authentication & Authorization

All endpoints (except public auth endpoints) require a JSON Web Token (JWT) passed in the `Authorization` HTTP header.

### Authorization Header Format
```http
Authorization: Bearer <your_jwt_token>
```

### JWT Claims Structure
When successfully authenticated, the token payload contains the following claims:
- `sub` (or NameIdentifier): The unique identifier of the authenticated user.
- `CompanyId`: The ID of the company the user belongs to.
- `DepartmentId`: The ID of the department the user belongs to (null/empty for Owner or Super Admin).
- `role`: The user's role (e.g., `sadmin`, `Owner`, `Manager`).

### User Roles
1. **`sadmin` (Super Admin)**: System-level administrator. Manages platform owners, activation, and deactivation. Associated with the special company `"InsightX System"`.
2. **`Owner`**: The registerer and owner of a company. Has permission to set up KPIs, invite managers, create departments, and view all company users.
3. **`Manager`**: Department-level manager. Can view company profiles and list users within their assigned department only.

---

## 🗂️ Endpoint Summary Table

| Method | Route | Auth Scheme | Allowed Roles | Description |
| :--- | :--- | :--- | :--- | :--- |
| **POST** | [`/auth/register`](#post-authregister) | ❌ Public | None | Register a new company & its Owner |
| **POST** | [`/auth/login`](#post-authlogin) | ❌ Public | None | Login to get access & refresh tokens |
| **POST** | [`/auth/refresh`](#post-authrefresh) | ❌ Public | None | Refresh an expired access token |
| **POST** | [`/auth/logout`](#post-authlogout) | ✅ Bearer JWT | Any Role | Log out and revoke refresh tokens |
| **GET** | [`/companies/me`](#get-companiesme) | ✅ Bearer JWT | Any Role | Get the current user's company profile |
| **PUT** | [`/companies/setup`](#put-companiessetup) | ✅ Bearer JWT | `Owner` | Configure company KPIs & thresholds |
| **POST** | [`/departments`](#post-departments) | ✅ Bearer JWT | `Owner` | Create a new company department |
| **GET** | [`/departments/{id}`](#get-departmentsid) | ✅ Bearer JWT | Any Role | Retrieve a specific department by ID |
| **GET** | [`/departments`](#get-departments) | ✅ Bearer JWT | Any Role | List all departments in the company |
| **PUT** | [`/departments/{id}`](#put-departmentsid) | ✅ Bearer JWT | `Owner` | Edit/Update a department's name |
| **DELETE** | [`/departments/{id}`](#delete-departmentsid) | ✅ Bearer JWT | `Owner` | Delete a department |
| **POST** | [`/kpis`](#post-kpis) | ✅ Bearer JWT | `Owner` | Create a new company KPI |
| **GET** | [`/kpis/{id}`](#get-kpisid) | ✅ Bearer JWT | Any Role | Retrieve a specific KPI by ID |
| **GET** | [`/kpis`](#get-kpis) | ✅ Bearer JWT | Any Role | List all KPIs in the company |
| **PUT** | [`/kpis/{id}`](#put-kpisid) | ✅ Bearer JWT | `Owner` | Edit/Update a KPI's details |
| **DELETE** | [`/kpis/{id}`](#delete-kpisid) | ✅ Bearer JWT | `Owner` | Delete a KPI |
| **POST** | [`/users/invite`](#post-usersinvite) | ✅ Bearer JWT | `Owner` | Invite a new Manager to a department |
| **GET** | [`/users`](#get-users) | ✅ Bearer JWT | Any Role | List company users (filtered by permissions) |
| **POST** | [`/users/change-password`](#post-userschange-password) | ✅ Bearer JWT | Any Role | Change the authenticated user's password |
| **DELETE** | [`/users/{id}`](#delete-usersid) | ✅ Bearer JWT | `Owner` | Delete a Manager user from the company |
| **GET** | [`/users/owners`](#get-usersowners) | ✅ Bearer JWT | `sadmin` | List all platform company Owners |
| **POST** | [`/users/{id}/activate`](#post-usersidactivate) | ✅ Bearer JWT | `sadmin` | Activate a user account |
| **POST** | [`/users/{id}/deactivate`](#post-usersiddeactivate) | ✅ Bearer JWT | `sadmin` | Deactivate a user account |

---

## 🛠️ Endpoints Reference

### 🔐 Authentication Module (`/auth`)

#### **POST** `/auth/register`
* **Description:** Registers a new company and seeds its `Owner` user in a single request.
* **Auth Requirement:** None (Public)
* **Request Body (`RegisterDto`):**
  ```json
  {
    "companyName": "Acme Corp",
    "ownerName": "Jane Doe",
    "email": "jane.doe@acme.com",
    "password": "SecurePassword123!"
  }
  ```
* **Success Response (`200 OK` with `AuthResponseDto`):**
  ```json
  {
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "4df9bc..."
  }
  ```
* **Error Response (`400 Bad Request` or `500 Internal Error`):**
  ```json
  "Company name already exists."
  ```

#### **POST** `/auth/login`
* **Description:** Validates credentials and returns JWT access and refresh tokens.
* **Auth Requirement:** None (Public)
* **Request Body (`LoginDto`):**
  ```json
  {
    "email": "jane.doe@acme.com",
    "password": "SecurePassword123!"
  }
  ```
* **Success Response (`200 OK` with `AuthResponseDto`):**
  ```json
  {
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "4df9bc..."
  }
  ```
* **Error Response (`401 Unauthorized`):**
  ```json
  "Invalid credentials or account is deactivated."
  ```

#### **POST** `/auth/refresh`
* **Description:** Refreshes an expired access token using a valid, active refresh token.
* **Auth Requirement:** None (Public)
* **Request Body (`RefreshDto`):**
  ```json
  {
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "4df9bc..."
  }
  ```
* **Success Response (`200 OK` with `AuthResponseDto`):**
  ```json
  {
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "newRefreshToken..."
  }
  ```
* **Error Response (`400 Bad Request`):**
  ```json
  "Invalid or expired refresh token."
  ```

#### **POST** `/auth/logout`
* **Description:** Revokes all active refresh tokens for the current authenticated user.
* **Auth Requirement:** Bearer JWT (Any Role)
* **Success Response (`200 OK`):**
  *(Empty body)*
* **Error Response (`401 Unauthorized`):**
  *(Unauthorized or expired token)*

---

### 🏢 Companies Module (`/companies`)

#### **GET** `/companies/me`
* **Description:** Retrieves the profile of the company associated with the authenticated user, including its departments and KPIs.
* **Auth Requirement:** Bearer JWT (Any Role)
* **Success Response (`200 OK` with `CompanyProfileDto`):**
  ```json
  {
    "id": 1,
    "name": "Acme Corp",
    "createdAt": "2026-06-24T20:30:00Z",
    "departments": [
      {
        "id": 5,
        "name": "Engineering"
      }
    ],
    "kpIs": [
      {
        "id": 12,
        "name": "Sprint Velocity",
        "threshold": 80.0,
        "unit": "Story Points"
      }
    ]
  }
  ```

#### **PUT** `/companies/setup`
* **Description:** Configures or overrides the KPIs and target thresholds for the company. Old KPIs are cleared and replaced by the new collection.
* **Auth Requirement:** Bearer JWT (`Owner` only)
* **Request Body (`SetupDto`):**
  ```json
  {
    "kpis": [
      {
        "name": "Customer Retention Rate",
        "threshold": 95.5,
        "unit": "%"
      },
      {
        "name": "Server Uptime",
        "threshold": 99.9,
        "unit": "%"
      }
    ]
  }
  ```
* **Success Response (`200 OK`):**
  *(Empty body)*

---

### 📂 Departments Module (`/departments`)

#### **POST** `/departments`
* **Description:** Creates a new department within the authenticated user's company.
* **Auth Requirement:** Bearer JWT (`Owner` only)
* **Request Body (`CreateDepartmentDto`):**
  ```json
  {
    "name": "Quality Assurance"
  }
  ```
* **Success Response (`201 Created` with `DepartmentResponseDto`):**
  *Headers:* `Location: /departments/15`
  ```json
  {
    "id": 15,
    "name": "Quality Assurance",
    "companyId": 1
  }
  ```

#### **GET** `/departments/{id}`
* **Description:** Retrieves details of a specific department by ID. It ensures data isolation so users cannot view departments of other companies.
* **Auth Requirement:** Bearer JWT (Any Role)
* **Route Parameter:** `id` (integer)
* **Success Response (`200 OK` with `DepartmentResponseDto`):**
  ```json
  {
    "id": 15,
    "name": "Quality Assurance",
    "companyId": 1
  }
  ```
* **Error Response (`404 Not Found`):**
  ```json
  "Department not found or belongs to another company."
  ```

#### **GET** `/departments`
* **Description:** Lists all departments in the current user's company.
* **Auth Requirement:** Bearer JWT (Any Role)
* **Success Response (`200 OK` with `List<DepartmentResponseDto>`):**
  ```json
  [
    {
      "id": 5,
      "name": "Engineering",
      "companyId": 1
    },
    {
      "id": 15,
      "name": "Quality Assurance",
      "companyId": 1
    }
  ]
  ```

#### **PUT** `/departments/{id}`
* **Description:** Updates the name of an existing department. Enforces cross-company isolation.
* **Auth Requirement:** Bearer JWT (`Owner` only)
* **Route Parameter:** `id` (integer)
* **Request Body (`CreateDepartmentDto`):**
  ```json
  {
    "name": "Research and Development"
  }
  ```
* **Success Response (`200 OK` with `DepartmentResponseDto`):**
  ```json
  {
    "id": 15,
    "name": "Research and Development",
    "companyId": 1
  }
  ```
* **Error Response (`400 Bad Request` or `404 Not Found`):**
  ```json
  "A department with this name already exists in your company."
  ```

#### **DELETE** `/departments/{id}`
* **Description:** Deletes a department by ID. It ensures data isolation. Associated users' `DepartmentId` will be set to `null`.
* **Auth Requirement:** Bearer JWT (`Owner` only)
* **Route Parameter:** `id` (integer)
* **Success Response (`204 NoContent`):**
  *(Empty body)*
* **Error Response (`404 Not Found`):**
  ```json
  "Department not found."
  ```

---

### 📈 KPIs Module (`/kpis`)

#### **POST** `/kpis`
* **Description:** Creates a new KPI within the authenticated user's company.
* **Auth Requirement:** Bearer JWT (`Owner` only)
* **Request Body (`CreateKpiDto`):**
  ```json
  {
    "name": "Revenue Growth",
    "threshold": 15000.0,
    "unit": "USD"
  }
  ```
* **Success Response (`201 Created` with `KpiResponseDto`):**
  *Headers:* `Location: /kpis/12`
  ```json
  {
    "id": 12,
    "name": "Revenue Growth",
    "threshold": 15000.0,
    "unit": "USD",
    "companyId": 1
  }
  ```
* **Error Response (`400 Bad Request`):**
  ```json
  "A KPI with this name already exists in your company."
  ```

#### **GET** `/kpis/{id}`
* **Description:** Retrieves details of a specific KPI by ID. It ensures data isolation so users cannot view KPIs of other companies.
* **Auth Requirement:** Bearer JWT (Any Role)
* **Route Parameter:** `id` (integer)
* **Success Response (`200 OK` with `KpiResponseDto`):**
  ```json
  {
    "id": 12,
    "name": "Revenue Growth",
    "threshold": 15000.0,
    "unit": "USD",
    "companyId": 1
  }
  ```
* **Error Response (`404 Not Found`):**
  ```json
  "KPI not found."
  ```

#### **GET** `/kpis`
* **Description:** Lists all KPIs in the current user's company.
* **Auth Requirement:** Bearer JWT (Any Role)
* **Success Response (`200 OK` with `List<KpiResponseDto>`):**
  ```json
  [
    {
      "id": 12,
      "name": "Revenue Growth",
      "threshold": 15000.0,
      "unit": "USD",
      "companyId": 1
    }
  ]
  ```

#### **PUT** `/kpis/{id}`
* **Description:** Updates the fields of an existing KPI. Enforces cross-company isolation.
* **Auth Requirement:** Bearer JWT (`Owner` only)
* **Route Parameter:** `id` (integer)
* **Request Body (`CreateKpiDto`):**
  ```json
  {
    "name": "Net Profit",
    "threshold": 20000.0,
    "unit": "USD"
  }
  ```
* **Success Response (`200 OK` with `KpiResponseDto`):**
  ```json
  {
    "id": 12,
    "name": "Net Profit",
    "threshold": 20000.0,
    "unit": "USD",
    "companyId": 1
  }
  ```
* **Error Response (`400 Bad Request` or `404 Not Found`):**
  ```json
  "A KPI with this name already exists in your company."
  ```

#### **DELETE** `/kpis/{id}`
* **Description:** Deletes a KPI by ID. It ensures data isolation.
* **Auth Requirement:** Bearer JWT (`Owner` only)
* **Route Parameter:** `id` (integer)
* **Success Response (`204 NoContent`):**
  *(Empty body)*
* **Error Response (`404 Not Found`):**
  ```json
  "KPI not found."
  ```

---

### 👥 User Management Module (`/users`)

#### **POST** `/users/invite`
* **Description:** Invites/creates a new employee with the `Manager` role. The invited manager will belong to the same company and must be assigned to an existing department belonging to that company.
* **Auth Requirement:** Bearer JWT (`Owner` only)
* **Request Body (`InviteUserDto`):**
  ```json
  {
    "name": "Alice Smith",
    "email": "alice.smith@acme.com",
    "password": "ManagerPassword123!",
    "departmentId": 5
  }
  ```
* **Success Response (`200 OK`):**
  *(Empty body)*
* **Error Response (`400 Bad Request`):**
  ```json
  "Invalid department or email already in use."
  ```

#### **GET** `/users`
* **Description:** Lists users in the company.
  * An **`Owner`** sees **all** users registered under their company.
  * A **`Manager`** only sees users registered under their **own department**.
* **Auth Requirement:** Bearer JWT (Any Role)
* **Success Response (`200 OK` with `List<UserResponseDto>`):**
  ```json
  [
    {
      "id": "e2f1...",
      "name": "Jane Doe",
      "email": "jane.doe@acme.com",
      "role": "Owner",
      "departmentId": null,
      "departmentName": null
    },
    {
      "id": "c8b4...",
      "name": "Alice Smith",
      "email": "alice.smith@acme.com",
      "role": "Manager",
      "departmentId": 5,
      "departmentName": "Engineering"
    }
  ]
  ```

#### **GET** `/users/owners`
* **Description:** Lists all company Owner users on the platform. Used by platform admins.
* **Auth Requirement:** Bearer JWT (`sadmin` only)
* **Success Response (`200 OK` with `List<OwnerManagementDto>`):**
  ```json
  [
    {
      "userId": "e2f1...",
      "ownerName": "Jane Doe",
      "ownerEmail": "jane.doe@acme.com",
      "isActivated": true,
      "companyId": 1,
      "companyName": "Acme Corp",
      "companyCreatedAt": "2026-06-24T20:30:00Z"
    }
  ]
  ```

#### **POST** `/users/{id}/activate`
* **Description:** Activates a user account, allowing them to login.
* **Auth Requirement:** Bearer JWT (`sadmin` only)
* **Route Parameter:** `id` (string/GUID)
* **Success Response (`200 OK`):**
  ```json
  {
    "message": "User activated successfully."
  }
  ```

#### **POST** `/users/{id}/deactivate`
* **Description:** Deactivates a user account, preventing them from logging in or using their tokens.
* **Auth Requirement:** Bearer JWT (`sadmin` only)
* **Route Parameter:** `id` (string/GUID)
* **Success Response (`200 OK`):**
  ```json
  {
    "message": "User deactivated successfully."
  }
  ```

#### **DELETE** `/users/{id}`
* **Description:** Deletes a Manager user account from the Owner's company. Cannot delete Owners or Super Admins.
* **Auth Requirement:** Bearer JWT (`Owner` only)
* **Route Parameter:** `id` (string/GUID)
* **Success Response (`204 NoContent`):**
  *(Empty body)*
* **Error Response (`400 Bad Request` or `403 Forbidden` or `404 Not Found`):**
  ```json
  "Only users with the Manager role can be deleted."
  ```

#### **POST** `/users/change-password`
* **Description:** Changes the password of the authenticated user.
* **Auth Requirement:** Bearer JWT (Any Role)
* **Request Body (`ChangePasswordDto`):**
  ```json
  {
    "currentPassword": "CurrentPassword123!",
    "newPassword": "NewSecurePassword123!"
  }
  ```
* **Success Response (`200 OK`):**
  *(Empty body)*
* **Error Response (`400 Bad Request`):**
  ```json
  "Password change failed: [Identity error details]"
  ```

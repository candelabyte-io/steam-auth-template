# Steam Auth Template

This is an Angular (frontend) and .NET (backend) project that uses Steam as an authenticator. After a successful Steam login the backend generates a JWT token which the frontend uses to authenticate requests to the API.

Short overview of how the backend and frontend work together and how to run the project (using `dotnet run` and `ng serve`).

## Project structure (important files)
- backend/SteamAuthTemplate.API
  - Controllers/AuthController.cs — Steam OpenID authentication flow, JWT creation and redirect to frontend, protected `/me` endpoint.
  - Program.cs — ASP.NET Core configuration, authentication registration (Steam, JWT).
  - appsettings.json / appsettings.Development.json — contains `Jwt:Key` and `Frontend:Url` configuration values.
- client (Angular app)
  - src/app/services/auth.service.ts — triggers login flow and calls protected API endpoints.
  - src/app/interceptors/jwt.interceptor.ts — attaches `Authorization: Bearer <token>` header to API requests.
  - src/app/components/login.component.ts — login button / link that starts the flow.
  - src/app/components/auth-callback.component.ts — reads `token` query param from backend redirect and stores it (typically in localStorage).

## How it works (flow)
1. User clicks "Login" in the Angular frontend.
2. Frontend opens the backend endpoint: GET /auth/steam/login.
3. Backend issues an authentication Challenge to Steam (OpenID). User authenticates at Steam.
4. Steam redirects back to backend at /auth/steam/callback with Steam identity info.
5. Backend extracts Steam ID from the returned identity, creates a JWT (signed with the symmetric key from `Jwt:Key`) containing the Steam ID as a claim, and redirects the browser to the frontend callback URL:
   /auth/callback?token=<url-encoded-jwt>
6. Frontend `auth-callback` component reads the `token` query parameter, stores the JWT (commonly in `localStorage`) and redirects the user to a protected page.
7. Angular `jwt.interceptor` adds `Authorization: Bearer <token>` to outgoing API requests. The backend validates the JWT for protected endpoints such as GET /auth/steam/me.

Notes:
- The JWT produced by the backend expires in 2 hours (see AuthController.cs).
- The backend returns `steamid` as a claim and in the `/me` response.

## Configuration
- Backend: set the `Jwt:Key` value in `backend/SteamAuthTemplate.API/appsettings.json` (or environment variables) to a strong secret used to sign tokens.
- Backend: set `Frontend:Url` to the running frontend base URL (e.g. `http://localhost:4200`) so the backend can redirect after successful login.
- Ensure the Steam OpenID authentication is configured in `Program.cs` (Steam/OpenID options may require a return URL matching the app registration).

## Prerequisites
- .NET 9 SDK (or matching SDK used by the project)
- Node.js and npm
- (Optional) Angular CLI installed globally for convenience: `npm i -g @angular/cli` — otherwise use the local CLI via `npx` or `npm run start`.

## Running locally

Open two terminals (backend and frontend) or use VS Code integrated terminals.

Backend (API):
```bash
# from project root
cd backend/SteamAuthTemplate.API
dotnet run
```
This runs the ASP.NET Core API. Check the console output for the URL(s) it binds to (e.g. http://localhost:5000 / https://localhost:5001 or as configured).

Frontend (Angular):
```bash
# from project root
cd client
npm install
npm run start
# or, if you prefer the CLI directly:
npx ng serve --open
```
Default Angular dev server address is `http://localhost:4200`. If you use a different port, update `backend/SteamAuthTemplate.API/appsettings.json` `Frontend:Url` value accordingly so redirects work.

## Testing the flow
1. Start backend and frontend.
2. Open the frontend (`http://localhost:4200`) and click the Login button.
3. Complete Steam login. After successful Steam auth, you should be redirected to the frontend with `?token=<jwt>`.
4. The frontend should store the token and call protected API endpoints like `/auth/steam/me`. You can also test an API call directly:
```bash
curl -H "Authorization: Bearer <jwt>" http://localhost:5000/auth/steam/me
```

## Troubleshooting
- If the frontend never receives a token: verify `Frontend:Url` is correct in backend configuration and the backend callback redirect matches the frontend route.
- If JWT validation fails: confirm `Jwt:Key` used to sign tokens matches the key used by API validation configuration.
- If Steam authentication fails: verify the Steam OpenID setup in `Program.cs` and ensure return URLs match registered/allowed URLs.

## Security notes
- Do NOT store long-lived secrets in source control. Use environment variables or a secrets store for `Jwt:Key`.
- For production, enable HTTPS and set appropriate cookie and token security settings.

## Quick summary of commands
```bash
# backend
cd backend/SteamAuthTemplate.API
dotnet run

# frontend
cd client
npm install
npm run start   # or npx ng serve --open
```

If you run into any issues, feel free to create an issue on the project's GitHub repository describing the problem and steps to reproduce it.

## License
This project is licensed under the MIT License — see the accompanying LICENSE file for details. This repository is MIT licensed on GitHub.

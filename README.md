# Home Manager
Home Manager to modularna aplikacja do sterowania logiką automatyzacji Home Assistant poza Node-RED. Home Assistant pozostaje źródłem stanów oraz wykonawcą komend, a Home Manager realizuje logikę decyzyjną, harmonogramy, override i historię decyzji.

## Stack
- Backend: .NET 8 Web API
- Frontend: Angular 15 (standalone + feature routing)
- Baza danych: MariaDB
- Integracja HA: REST API
- Testy: xUnit (logika domenowa i aplikacyjna)
- Orkiestracja: Docker Compose

## Architektura
Projekty:
- `HomeManager.Domain` — modele domenowe, enumy, reguły presetów
- `HomeManager.Application` — silnik decyzyjny, harmonogramy, override, porty
- `HomeManager.Infrastructure` — EF Core, repozytoria, klient HA REST
- `HomeManager.Api` — REST API, background services, health check, middleware błędów
- `HomeManager.Tests.Unit` — testy logiki decyzyjnej
- `HomeManager.Tests.Integration` — testy integracyjne
- `HomeManager.Frontend` — UI konfiguracyjne managera

Zakładki UI:
- Dashboard
- Energy Manager
- Rooms
- Schedules
- Overrides
- Home Assistant Entities
- Logs / Decision History
- Settings

## Wymagania
- .NET SDK 8.0+
- Node.js 14+ (zalecane 18+)
- npm
- Docker + Docker Compose (dla uruchomienia kontenerowego)

## Konfiguracja backendu
1. Skopiuj `HomeManager.Api/appsettings.example.json` do `HomeManager.Api/appsettings.Development.json` (lub ustaw zmienne środowiskowe).
2. Uzupełnij:
   - `HomeAssistant.BaseUrl`
   - `HomeAssistant.LongLivedAccessToken`
   - `ConnectionStrings.MariaDb`

## Uruchomienie lokalne
Backend:
1. `dotnet restore HomeManager.sln`
2. `dotnet run --project HomeManager.Api/HomeManager.Api.csproj`

Frontend:
1. `cd HomeManager.Frontend`
2. `npm install`
3. `npm start`

Domyślne endpointy:
- API: `http://localhost:8080`
- Health: `http://localhost:8080/health`
- Frontend: `http://localhost:4200`

## Uruchomienie Docker Compose
1. Opcjonalnie ustaw token HA:
   - PowerShell: `$env:HOME_ASSISTANT_TOKEN=\"twoj_token_ha\"`
2. Uruchom:
   - `docker compose up --build -d`
3. Sprawdź:
   - Frontend: `http://localhost:4200`
   - API health: `http://localhost:8080/health`

Compose uruchamia:
- `mariadb` (persistent volume `mariadb_data`)
- `backend` (.NET API z healthcheck)
- `frontend` (nginx + reverse proxy `/api` -> backend)

## Najważniejsze endpointy API
- `GET /api/states`
- `GET /api/states/{entityId}`
- `POST /api/services/{domain}/{service}`
- `GET/POST/PUT/DELETE /api/rooms`
- `GET/POST/PUT/DELETE /api/schedules`
- `GET/POST/DELETE /api/overrides`
- `POST /api/managers/power/evaluate`
- `GET /api/decisions/latest`
- `GET /api/decisions/history`
- `GET /api/ha/entities`
- `POST /api/ha/refresh-entities`
- `GET/PUT /api/settings`
- `GET /api/logs`
- `GET /api/dashboard`

## Testy i walidacja
Backend:
- `dotnet build HomeManager.sln --no-restore`
- `dotnet test HomeManager.sln --no-build`

Frontend:
- `cd HomeManager.Frontend`
- `npm run build`
- `npm run lint`
- `npm run test`

## Fallback heartbeat (Home Assistant)
Przykładową automatyzację fallback znajdziesz w:
- `home-assistant-heartbeat-fallback.yaml`

Cel: w razie utraty heartbeat z Home Manager wymusić bezpieczny preset (np. `eco`) dla wybranych termostatów.

# Teams Call Center

Bot de recording pentru Microsoft Teams cu dashboard live pentru call center.

## Structura Proiectului

```
teams-call-center/
├── src/
│   ├── TeamsCallCenter.Bot/      # Bot service pentru recording
│   ├── TeamsCallCenter.Api/      # API + SignalR hub
│   └── TeamsCallCenter.Shared/   # Modele comune
├── dashboard/                     # Svelte dashboard (Teams Tab)
└── teams-app/                     # Teams app manifest
```

## Cerinte

- .NET 8 SDK
- Node.js 18+
- Azure subscription
- Microsoft 365 tenant cu admin access

## Setup

### 1. Inregistrare Bot in Azure

1. Creaza un Azure Bot resource
2. Noteaza App ID si App Secret
3. Adauga permisiunile:
   - `Calls.AccessMedia.All`
   - `Calls.JoinGroupCall.All`
4. Obtine admin consent:
   ```
   https://login.microsoftonline.com/common/adminconsent?client_id=<APP_ID>
   ```

### 2. Configurare Recording Policy (PowerShell)

```powershell
# Conectare la Teams
Import-Module MicrosoftTeams
Connect-MicrosoftTeams

# Creaza application instance
New-CsOnlineApplicationInstance -UserPrincipalName bot@contoso.com -DisplayName "Call Center Bot" -ApplicationId <APP_ID>
Sync-CsOnlineApplicationInstance -ObjectId <OBJECT_ID>

# Creaza policy
New-CsTeamsComplianceRecordingPolicy -Enabled $true -Description "Call Center Recording" CallCenterRecording
Set-CsTeamsComplianceRecordingPolicy -Identity CallCenterRecording -ComplianceRecordingApplications @(New-CsTeamsComplianceRecordingApplication -Parent CallCenterRecording -Id <OBJECT_ID>)

# Atribuie policy la utilizatori
Grant-CsTeamsComplianceRecordingPolicy -Identity agent@contoso.com -PolicyName CallCenterRecording
```

### 3. Configurare Proiect

```bash
# Backend
cd src/TeamsCallCenter.Bot
# Editeaza appsettings.json cu valorile tale

# Dashboard
cd dashboard
npm install
```

### 4. Run Local (Development)

```bash
# Terminal 1 - API
cd src/TeamsCallCenter.Api
dotnet run

# Terminal 2 - Bot (necesita certificate si IP public pentru Teams)
cd src/TeamsCallCenter.Bot
dotnet run

# Terminal 3 - Dashboard
cd dashboard
npm run dev
```

### 5. Deploy pe Azure

#### API + SignalR
- Deploy pe Azure App Service
- Configureaza CORS pentru domeniul dashboard-ului

#### Bot Service
- Deploy pe Azure Cloud Service (Extended Support)
- Configureaza certificat SSL
- Configureaza firewall pentru porturi media

#### Dashboard
- Build: `npm run build`
- Deploy fisierele din `build/` pe Azure Static Web Apps

### 6. Instaleaza Teams App

1. Editeaza `teams-app/manifest.json` cu valorile tale
2. Zip-uieste folder-ul `teams-app/`
3. In Teams Admin Center, upload app-ul
4. Asigneaza app-ul la utilizatorii doriti

## Arhitectura

```
┌──────────────────────────────────────────────────────────┐
│                    TEAMS CLIENT                          │
│  ┌─────────────────┐     ┌─────────────────────────┐    │
│  │  Teams Tab      │     │  Recording Bot          │    │
│  │  (Svelte)       │     │  (auto-join calls)      │    │
│  └────────┬────────┘     └───────────┬─────────────┘    │
└───────────┼──────────────────────────┼──────────────────┘
            │                          │
            │ SignalR                  │ Graph API
            ▼                          ▼
┌──────────────────────────────────────────────────────────┐
│                 AZURE BACKEND                            │
│  ┌─────────────────┐     ┌─────────────────────────┐    │
│  │  API + SignalR  │◄────│  Recording Bot          │    │
│  │  (ASP.NET)      │     │  (processes calls)      │    │
│  └─────────────────┘     └─────────────────────────┘    │
└──────────────────────────────────────────────────────────┘
```

## Features

- [x] Receive audio streams din apeluri Teams
- [x] Dashboard live cu SignalR
- [x] Statistici call center
- [x] Teams Tab integration
- [ ] Salvare audio in Azure Blob Storage
- [ ] Transcriptie cu Azure Cognitive Services
- [ ] Istoric apeluri in database

## Limitari

- Necesita Azure Cloud Services pentru bot (nu poate rula local complet)
- Necesita certificat SSL valid
- Necesita IP public pentru media streams
- Recording policy se aplica la nivel de utilizator

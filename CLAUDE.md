# CLAUDE.md

## Project Overview

Microsoft Teams Call Center - compliance recording bot cu dashboard real-time.

## Build & Run

```bash
dotnet run --project src/TeamsCallCenter
```

Dashboard: `https://localhost:7001`
Swagger: `https://localhost:7001/swagger`

## Structure

```
TeamsCallCenter/
├── Bot/                    # Teams Bot
│   ├── Configuration/      # BotConfiguration, RecordingConfiguration
│   ├── Services/           # BotService, AuthenticationProvider
│   ├── Audio/              # AudioRecordingService, WavFileWriter
│   └── Media/              # CallHandler, BotMediaStream
├── Controllers/            # CallsController, CallingController
├── Hubs/                   # CallsHub (SignalR)
├── Models/                 # CallInfo, CallStatistics, AgentInfo, RecordingInfo
├── Services/               # ICallStateService, InMemoryCallStateService
└── wwwroot/                # Dashboard HTML
```

## Configuration (appsettings.json)

```json
{
  "Bot": {
    "AppId": "YOUR_BOT_APP_ID",
    "AppSecret": "YOUR_BOT_APP_SECRET",
    "ServiceDnsName": "your-bot.azurewebsites.net",
    "ServiceCname": "your-bot.azurewebsites.net",
    "CertificateThumbprint": "YOUR_CERT_THUMBPRINT"
  },
  "Recording": {
    "Enabled": true,
    "RecordingPath": "C:\\Recordings"
  }
}
```

## API Endpoints

- `GET /` - Dashboard
- `GET /api/calls` - Active calls
- `GET /api/calls/statistics` - Statistics
- `POST /api/calling` - Teams webhook
- `GET /health` - Health check
- SignalR Hub: `/hubs/calls`

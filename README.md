# CRM Insight - Outlook Web Plugin (Blazor)

A high-performance, lightweight Outlook web plugin interface built with .NET 9 Blazor WebAssembly. This plugin integrates with CRM systems to provide contextual data and sales engagement tools directly within Outlook.

## Features

- **Tab-based Navigation**: Related, Track, and Actions tabs with scroll position preservation
- **Contextual Data Display**: View contacts, accounts, and events related to the current email
- **Email Tracking**: Relate emails to CRM contacts and accounts with autocomplete search
- **Copilot Discovery**: Automatic detection of new contacts from email conversations
- **Sales Engagement Actions**: Quick actions for follow-ups, scheduling, and CRM sync
- **Multiple Auth Methods**: Microsoft Account, Salesforce, or credentials-based login

## Tech Stack

- **Framework**: .NET 9 Blazor WebAssembly
- **Styling**: Custom CSS (Fluent UI-inspired utility classes)
- **State Management**: Singleton AppState service with event-based updates
- **Icons**: Custom SVG icon components

## Project Structure

```
├── Components/
│   ├── Icons/                 # SVG icon components
│   ├── Layout/                # Header, Shell, TabNavigation, CopilotPanel
│   ├── Shared/                # EntityCard, StatusBadge, SearchField, Autocomplete, ButtonGroup
│   └── Tabs/                  # RelatedTab, TrackTab, ActionsTab
├── Pages/
│   ├── Auth.razor             # Authentication page
│   └── Home.razor             # Main dashboard with tab content
├── Services/
│   └── AppState.cs            # Global state management
├── wwwroot/
│   ├── css/app.css            # Custom styling
│   └── index.html             # HTML entry point
├── App.razor                  # Router configuration
├── Program.cs                 # App entry point and DI setup
└── manifest.xml               # Office Add-in manifest
```

## Getting Started

### Prerequisites

- .NET 9 SDK
- Visual Studio 2022 or VS Code with C# extension

### Installation

```bash
# Restore dependencies
dotnet restore

# Run development server
dotnet run

# Or with hot reload
dotnet watch run
```

### Development URLs

- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001 (required for Office Add-in testing)

## Office Add-in Deployment

### Sideloading for Testing

1. Start the app with HTTPS: `dotnet run --urls "https://localhost:5001"`
2. Open Outlook on the web (outlook.office.com)
3. Go to Settings → View all Outlook settings → Add-ins
4. Select "My add-ins" → "Add a custom add-in" → "Add from file"
5. Upload the `manifest.xml` file

### Production Deployment

1. Publish the app: `dotnet publish -c Release`
2. Deploy to Azure Static Web Apps, Azure App Service, or any static hosting
3. Update `manifest.xml` with production URLs
4. Submit to Microsoft AppSource or deploy via Admin Center

## Component Usage

### EntityCard

```razor
<EntityCard Type="EntityCard.EntityType.Contact"
            Title="John Smith"
            Subtitle="john@example.com"
            Description="Acme Corp"
            IsTracked="true"
            AvatarInitials="JS"
            OnTrack="() => HandleTrack(id)" />
```

### Autocomplete

```razor
<Autocomplete Label="Relate to Contacts"
              Options="contactOptions"
              SelectedIds="selectedIds"
              OnSelect="id => AddContact(id)"
              OnDeselect="id => RemoveContact(id)"
              Placeholder="Search contacts..."
              Multiple="true" />
```

### AppState

```csharp
// Inject AppState
@inject AppState AppState

// Subscribe to changes
AppState.OnChange += StateHasChanged;

// Update state
AppState.SetActiveTab(ActiveTab.Track);
AppState.TrackContact(contactId);
```

## Accessibility

- Full keyboard navigation support
- ARIA labels and roles
- High contrast mode support
- Focus visible indicators

## Performance Optimizations

- Blazor WebAssembly for client-side execution
- Minimal dependencies for small download size
- Event-based state updates (no heavy state management library)
- CSS utility classes for consistent, lightweight styling

## License

MIT

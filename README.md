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



---

# Orcas Initiative – Recurring Meeting & Sync Requirements  
**(Stand-up Call Notes – 12 Mar 2026)**

**Created by:** Seneca (drafted based on call)  
**Last Updated:** 12 March 2026  
**Labels:** orcas, outlook-plugin, salesforce-sync, recurring-meetings, ib, gm

---

## Meeting Purpose
Captured requirements from today’s stand-up call regarding the **Outlook plugin → Sync Engine → Salesforce** integration.  
Focus is on **full lifecycle support for recurring/series meetings** plus critical decisions on multi-business-line architecture (IB + GM).

---

## 1. Recurring / Series Meeting Lifecycle (Highest Priority)
The plugin must fully support recurring meetings end-to-end. All actions below must be captured from Outlook events and forwarded to the Sync Engine.

### Creation / Forwarding
- When a user creates or updates a **series** of recurring meetings in Outlook, the plugin must forward the **entire series** (including the master meeting ID) to the Sync Engine.
- Seneca’s name (or any attendee) must be sent to Salesforce.

### Attendee Add / Remove
- Detect whether the change applies to:
  - A **single instance** only, **OR**
  - The **entire series**
- Sync the correct update to Salesforce via the Sync Engine.

### Any Change Tracking
- Time, title, location, description, attendees, recurrence pattern — **any modification** must be captured and synced.

---

## 2. Delete / Purge Handling
- User deletes a **single meeting** or an **entire recurring series** in Outlook.
- Plugin must immediately capture the meeting ID(s) and notify the Sync Engine.
- Sync Engine must **purge** those meetings from Salesforce going forward.
- (Outlook handles its own deletion; we only trigger Salesforce purge.)

---

## 3. UI & Business Line Separation (IB vs GM)
**Decision:** Do **NOT** do dynamic layout switching inside one plugin (too complex).

Build and deploy **two completely separate Outlook add-ins** (same codebase OK).

| Business Line | Page Name                        | Fields Included                              | Manifest Name      | Client ID/Secret | Settings File   |
|---------------|----------------------------------|----------------------------------------------|--------------------|------------------|-----------------|
| **IB**        | Meeting Planner (existing)       | Products, Projects, Meeting Purpose, Meeting Type | `Orcas-IB.xml`    | IB-specific      | IB settings     |
| **GM**        | Event / Activity Planner (restore immediately) | NO Products, NO Projects (simplified)       | `Orcas-GM.xml`    | GM-specific      | GM settings     |

- Both instances can run on the **same IIS server** as separate websites.
- Users download the correct one:
  - IB users → **Orcas IB**
  - GM users → **Orcas GM**
- Future business lines → additional manifests (no shared reactive template).

**Code Rule (Roman):**  
Do **NOT** delete any existing work (GM page, IB page, or any code).  
Keep IB and GM in **completely separate folders/projects** for zero risk.

---

## 4. Authentication & Deployment
- Use IAC provisioning per instance.
- Each instance gets its own **Client ID + Client Secret**.
- Store in a simple `appsettings.config` (one set per instance only).
- No runtime user detection needed in the plugin.

---

## Action Items

| Owner   | Task                                                                 | Status    |
|---------|----------------------------------------------------------------------|-----------|
| Seneca  | Create this page and add all points above                            | Done      |
| Roman   | Restore the GM “Event/Activity Planner” page immediately             | Pending   |
| Roman   | Prepare two separate manifests (`Orcas-IB.xml` + `Orcas-GM.xml`)     | Pending   |
| Roman   | Create separate settings files and folders for IB/GM                 | Pending   |
| Roman   | Do NOT delete any code or pages                                      | Ongoing   |
| All     | Reach out immediately if anything is unclear                         | —         |

---

**File ready to save:**  
`Orcas_Recurring_Meeting_Requirements.md`

Just copy everything above (including the frontmatter if you use Obsidian/Notion/etc.) and paste into a new `.md` file.  
Let me know if you want a version with checkboxes, Jira links, or a separate README-style file!





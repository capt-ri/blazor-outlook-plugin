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


----

Purpose of the Meeting
The team discussed a new Outlook plugin (built as a lightweight web app) to sync calendar events (meetings) and emails from Outlook to Salesforce, replacing an older, intrusive Nomura plugin that users disliked.
Key Problems with the Old Plugin
•  Every time a user created a meeting in Outlook, a pop-up appeared asking if it should sync to Salesforce.
•  This was annoying because most meetings are internal and users didn’t want them in Salesforce.
•  Users found it disruptive, leading the team to abandon the old plugin.
New Plugin Approach (Cherry-Picking Model)
•  No automatic pop-up on every meeting creation.
•  A side panel appears on the right in Outlook when you open/create a calendar event.
•  The panel auto-populates with meeting details (subject, attendees, time, etc.).
•  It checks Salesforce in real-time as you type attendees:
	•  If the contact exists → shows the matching contact/account.
	•  If not → shows a + button to quickly create the new contact in Salesforce.
•  Users cherry-pick what to sync:
	•  Option to click “Save to Salesforce” (or a configurable checkbox).
	•  Or tie syncing to Outlook’s Send button (with possible confirmation prompt).
•  Alternative backend option (still in discussion): A sync engine that polls for events marked with a “sync” flag, so users don’t need to click anything extra.
Customization & Flexibility
•  Highly configurable by group (IB, GM, Instant, etc.):
	•  Show/hide fields.
	•  Default activity types/subtypes (e.g., default to “Sales Meeting” for Instant users).
	•  Required field validation (pulls from Salesforce).
	•  Different behavior per user type (e.g., simpler experience for Instant/traders).
•  Panel is responsive (can be resized) and runs as a web view (not a heavy desktop add-in).
Performance & User Experience Concerns
•  The new plugin is designed to be lightweight (modeled after Riva, which has had few complaints).
•  Old plugin caused slowdowns/crashes; this one shouldn’t, but they will thoroughly test in a QA M365 environment.
•  Panel always appears when editing a meeting (takes about 1/3 of the screen).
•  Focus on making it simple and fast for salespeople/traders who work quickly and don’t want extra clicks or clutter.
Current Status & Next Steps
•  Currently only a web demo (test mode shown).
•  Cannot yet deploy/test in actual Outlook because they need MTA/NTI approval from Infosec (no SAS or EI components, so it should be straightforward).
•  Once approved → deploy to QA, test with users (including potential Instant users), check performance, crashes, loading time, different Outlook versions, etc.
•  Feedback from the meeting (especially from LK on Instant user behavior) will be used to refine UI, button placement, defaults, and options (e.g., checkbox vs. button, panel visibility).
Overall tone: Positive and collaborative. The team appreciated the detailed user-perspective feedback on making it “dummy-proof,” fast, and non-intrusive for busy salespeople while giving control over what gets synced to Salesforce. Development is on hold until security approval, with no expected rollout before summer at the earliest.





-----

Prompt for Implementing Email Sync from Outlook to Salesforce (EmailMessage Object):
You are an experienced Salesforce + Outlook/VBA/Office Add-in developer.
Business Requirements:
We need to implement manual email sync from Outlook to Salesforce using the standard EmailMessage object (not Events or Tasks).
Key points from the discussion:
•  Emails should sync as EmailMessage records in Salesforce (Out-of-the-box object).
•  We must also create related EmailMessageRelation records to link the email to the correct Account and Contact.
•  This functionality applies to EM, GM, and IB teams.
•  For NCM: Sync everything automatically.
•  For IB/GM: Provide user option via right-click context menu in Outlook to selectively sync individual emails.
Core Functionality to Implement:
1.  Right-click Context Menu in Outlook
	•  Add a custom “Sync to Salesforce” option when user right-clicks an email.
	•  (Note: Outlook may not support traditional “On Create / On Open” events reliably in Exchange Online / modern Outlook. Use available event handlers or button-based triggers.)
2.  EmailMessage Object Mapping When syncing an email, populate the following standard fields on the EmailMessage object:
	•  Subject (Subject)
	•  From Address (FromAddress)
	•  From Name (FromName)
	•  To Address (ToAddress)
	•  Cc Address (CcAddress)
	•  Bcc Address (BccAddress)
	•  Message Body / HTML Body (TextBody or HtmlBody)
	•  Sent Date (MessageDate)
	•  Incoming / Outgoing flag (Incoming)
	•  Has Attachment (HasAttachment)
	•  Any other relevant standard fields
3.  EmailMessageRelation Records
	•  Create EmailMessageRelation records to link the EmailMessage to:
		•  Related Contact (based on email address matching)
		•  Related Account (via the Contact or direct lookup)
	•  Similar to how EventRelation works for calendar events.
4.  Logic Flow
	•  User selects one or more emails in Outlook → right-clicks → chooses “Sync to Salesforce”
	•  Extract all relevant email properties
	•  Map them to Salesforce EmailMessage object
	•  Create the EmailMessage record
	•  Create corresponding EmailMessageRelation records
	•  Show success/error feedback to the user
Technical Notes:
•  Do not rely on Outlook “Item Add”, “Item Open”, or similar events if they are unreliable in Exchange Online.
•  Focus on manual trigger via custom ribbon button or context menu.
•  Handle both sent and received emails correctly (set Incoming flag appropriately).
•  Properly handle attachments if required (at minimum, set HasAttachment flag).
•  Ensure email addresses are matched to existing Salesforce Contacts.
Deliverables:
Please provide:
1.  The complete code structure (VBA / Office JS / C# — whichever is most appropriate for modern Outlook add-in)
2.  Field mapping table (Outlook property → Salesforce EmailMessage field)
3.  Logic for creating EmailMessageRelation records
4.  Error handling and user feedback mechanism
5.  Any necessary Salesforce configuration notes (permissions, field access, etc.)
Start by first creating the EmailMessage mapping and EmailMessageRelation creation logic, as this is the core that needs to be tested independently of the Outlook event hook.
Once the core sync works, we can focus on the right-click integration.


# Complete Development Prompt: Outlook Web Plugin (Blazor WebAssembly)

## Objective

Build a high-performance, lightweight Outlook web plugin using .NET Blazor WebAssembly. The plugin provides CRM integration features including contextual data display, email tracking, contact discovery, and sales engagement actions. The UI follows Microsoft Fluent Design principles and is optimized for the Outlook Task Pane environment (350px width).

**CRITICAL**: Outlook add-ins run in a sandboxed iframe where browser history APIs (`history.pushState`, `history.replaceState`) are blocked. This implementation uses **state-based navigation** instead of URL-based routing to work correctly in the Outlook environment.

---

## Technical Stack

- **Framework**: .NET 8 Blazor WebAssembly (Standalone)
- **Styling**: Custom CSS with Fluent UI-inspired utility classes (no external CSS framework)
- **State Management**: Singleton `AppState` service with event-based change notifications
- **Navigation**: State-based page switching (not URL routing) for Outlook iframe compatibility
- **Icons**: Custom SVG Razor components
- **Office Integration**: Office.js API for Outlook communication
- **Architecture**: Component-based with shared reusable components

---

## Project Structure

Create the following folder structure:

```
OutlookPlugin/
├── Components/
│   ├── Icons/                    # SVG icon components
│   │   ├── SearchIcon.razor
│   │   ├── SettingsIcon.razor
│   │   ├── CalendarIcon.razor
│   │   ├── DismissIcon.razor
│   │   ├── PersonIcon.razor
│   │   ├── BuildingIcon.razor
│   │   ├── ChevronDownIcon.razor
│   │   ├── ChevronUpIcon.razor
│   │   ├── ChevronLeftIcon.razor # Back navigation icon
│   │   ├── SparkleIcon.razor
│   │   ├── MailIcon.razor
│   │   ├── PersonAddIcon.razor
│   │   ├── CheckmarkIcon.razor
│   │   ├── MoreIcon.razor
│   │   ├── SaveIcon.razor
│   │   ├── DeleteIcon.razor
│   │   ├── SendIcon.razor
│   │   ├── ClockIcon.razor
│   │   ├── DocumentIcon.razor
│   │   ├── SyncIcon.razor
│   │   ├── FilterIcon.razor
│   │   ├── CloudIcon.razor
│   │   ├── KeyIcon.razor
│   │   └── HomeIcon.razor       # Home navigation icon
│   ├── Layout/
│   │   ├── Header.razor          # Top navigation bar with home icon
│   │   ├── Shell.razor           # Main layout wrapper
│   │   ├── TabNavigation.razor   # Tab bar component
│   │   └── CopilotPanel.razor    # Contact discovery panel
│   ├── Shared/
│   │   ├── StatusBadge.razor     # Status indicator badges
│   │   ├── EntityCard.razor      # Generic entity card
│   │   ├── DiscoveryCard.razor   # Quick track contact card
│   │   ├── SearchField.razor     # Search with filters
│   │   ├── Autocomplete.razor    # Multi-select combobox
│   │   └── ButtonGroup.razor     # Action button groups
│   ├── Tabs/
│   │   ├── RelatedTab.razor      # Related entities view
│   │   ├── TrackTab.razor        # Email tracking form
│   │   ├── ActionsTab.razor      # Sales actions view
│   │   ├── ActionCard.razor      # Individual action card
│   │   └── SectionHeader.razor   # Collapsible section header
│   └── _Imports.razor            # Component-level imports
├── Pages/
│   ├── Auth.razor                # Authentication page
│   ├── Home.razor                # Main dashboard
│   └── Email.razor               # Email details page
├── Services/
│   ├── AppState.cs               # Global state management with page navigation
│   └── OutlookService.cs         # Office.js interop service
├── wwwroot/
│   ├── css/
│   │   └── app.css               # Custom styles
│   ├── index.html                # HTML entry point with Office.js & history polyfill
│   ├── icon-16.png               # Add-in icons (actual PNG files)
│   ├── icon-32.png
│   ├── icon-64.png
│   ├── icon-80.png
│   └── icon-128.png
├── App.razor                     # State-based page routing (NOT URL Router)
├── Program.cs                    # Entry point & DI
├── _Imports.razor                # Global imports
├── manifest.xml                  # Production manifest template
├── manifest.dev.xml              # Development manifest (localhost)
└── OutlookPlugin.csproj          # Project file
```

---

## Step-by-Step Implementation

### Step 1: Create Blazor WebAssembly Project

```bash
dotnet new blazorwasm -n OutlookPlugin -o OutlookPlugin --framework net8.0
cd OutlookPlugin
```

Delete the default template files:
- `Pages/Counter.razor`
- `Pages/Weather.razor`
- `Layout/MainLayout.razor`
- `Layout/MainLayout.razor.css`
- `Layout/NavMenu.razor`
- `Layout/NavMenu.razor.css`

---

### Step 2: Create AppState Service (`Services/AppState.cs`)

This is the global state management service with **page navigation support** for Outlook iframe compatibility.

```csharp
namespace OutlookPlugin.Services;

public enum AuthStatus { Unauthenticated, Authenticated, Connecting }
public enum AuthProvider { None, Microsoft, Salesforce }
public enum ActiveTab { Related, Track, Actions }
public enum FilterType { All, Contacts, Accounts, Events }

// Page enum for state-based navigation (required for Outlook iframe sandbox)
public enum AppPage { Auth, Home, Email }

public record Contact(string Id, string Name, string Email, string? Company, bool IsTracked, string? AvatarInitials);
public record Account(string Id, string Name, string Type, bool IsTracked);
public record CalendarEvent(string Id, string Title, DateTime Date, string Time, List<string> Attendees, bool IsTracked);
public record DiscoveredContact(string Id, string Email, string? Name, bool IsNew);

public class DraftState
{
    public List<string> RelatedContacts { get; set; } = new();
    public List<string> RelatedAccounts { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
    public bool IsDirty { get; set; }
}

public class AppState
{
    public event Action? OnChange;
    
    // Auth state
    public AuthStatus AuthStatus { get; private set; } = AuthStatus.Unauthenticated;
    public AuthProvider AuthProvider { get; private set; } = AuthProvider.None;
    public bool CrmConnected { get; private set; }
    
    // Navigation state - uses AppPage for state-based routing
    public AppPage CurrentPage { get; private set; } = AppPage.Auth;
    public ActiveTab ActiveTab { get; private set; } = ActiveTab.Related;
    public string SearchQuery { get; private set; } = string.Empty;
    public bool SearchVisible { get; private set; }
    
    // Data (initialize with mock data)
    public List<Contact> Contacts { get; private set; } = new();
    public List<Account> Accounts { get; private set; } = new();
    public List<CalendarEvent> Events { get; private set; } = new();
    public List<DiscoveredContact> DiscoveredContacts { get; private set; } = new();
    
    // Draft state for tracking
    public DraftState Draft { get; private set; } = new();
    
    // UI state
    public bool IsLoading { get; private set; }
    public bool CopilotExpanded { get; private set; } = true;

    public AppState() { InitializeMockData(); }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
    
    private void InitializeMockData()
    {
        Contacts = new List<Contact>
        {
            new("1", "John Smith", "john.smith@acme.com", "Acme Corp", true, "JS"),
            new("2", "Sarah Johnson", "sarah.j@techstart.io", "TechStart", false, "SJ"),
            new("3", "Michael Chen", "m.chen@globalinc.com", "Global Inc", true, "MC")
        };
        
        Accounts = new List<Account>
        {
            new("1", "Acme Corporation", "Customer", true),
            new("2", "TechStart Inc", "Prospect", false),
            new("3", "Global Industries", "Partner", true)
        };
        
        Events = new List<CalendarEvent>
        {
            new("1", "Quarterly Review Meeting", new DateTime(2026, 2, 18), "10:00 AM", 
                new List<string> { "John Smith", "Sarah Johnson" }, false),
            new("2", "Product Demo Call", new DateTime(2026, 2, 20), "2:00 PM",
                new List<string> { "Michael Chen" }, true)
        };
        
        DiscoveredContacts = new List<DiscoveredContact>
        {
            new("d1", "alex.wilson@newclient.com", "Alex Wilson", true),
            new("d2", "emma.davis@partner.org", null, true)
        };
    }

    // Auth actions - auto-navigate on auth status change
    public void SetAuthStatus(AuthStatus status)
    {
        AuthStatus = status;
        // Auto-navigate based on auth status
        if (status == AuthStatus.Authenticated)
        {
            CurrentPage = AppPage.Home;
        }
        else if (status == AuthStatus.Unauthenticated)
        {
            CurrentPage = AppPage.Auth;
        }
        NotifyStateChanged();
    }
    
    public void SetAuthProvider(AuthProvider provider)
    {
        AuthProvider = provider;
        NotifyStateChanged();
    }
    
    public async Task ConnectToCrmAsync()
    {
        IsLoading = true; NotifyStateChanged();
        await Task.Delay(1500);
        CrmConnected = true; AuthStatus = AuthStatus.Authenticated;
        CurrentPage = AppPage.Home; // Navigate to home after connecting
        IsLoading = false;
        NotifyStateChanged();
    }
    
    public void SignOut()
    {
        AuthStatus = AuthStatus.Unauthenticated;
        AuthProvider = AuthProvider.None;
        CrmConnected = false;
        CurrentPage = AppPage.Auth; // Navigate back to auth
        ActiveTab = ActiveTab.Related;
        SearchQuery = string.Empty;
        SearchVisible = false;
        Draft = new DraftState();
        InitializeMockData();
        NotifyStateChanged();
    }

    // Navigation actions
    public void NavigateToPage(AppPage page)
    {
        CurrentPage = page;
        NotifyStateChanged();
    }
    
    public void SetActiveTab(ActiveTab tab)
    {
        ActiveTab = tab;
        NotifyStateChanged();
    }
    
    public void SetSearchQuery(string query)
    {
        SearchQuery = query;
        NotifyStateChanged();
    }
    
    public void ToggleSearch()
    {
        SearchVisible = !SearchVisible;
        if (!SearchVisible) SearchQuery = "";
        NotifyStateChanged();
    }

    // Data actions
    public void TrackContact(string id)
    {
        var contact = Contacts.FirstOrDefault(c => c.Id == id);
        if (contact != null)
        {
            var index = Contacts.IndexOf(contact);
            Contacts[index] = contact with { IsTracked = !contact.IsTracked };
            NotifyStateChanged();
        }
    }
    
    public void TrackAccount(string id)
    {
        var account = Accounts.FirstOrDefault(a => a.Id == id);
        if (account != null)
        {
            var index = Accounts.IndexOf(account);
            Accounts[index] = account with { IsTracked = !account.IsTracked };
            NotifyStateChanged();
        }
    }
    
    public void TrackEvent(string id)
    {
        var evt = Events.FirstOrDefault(e => e.Id == id);
        if (evt != null)
        {
            var index = Events.IndexOf(evt);
            Events[index] = evt with { IsTracked = !evt.IsTracked };
            NotifyStateChanged();
        }
    }
    
    public void QuickTrackContact(string id)
    {
        DiscoveredContacts = DiscoveredContacts.Where(c => c.Id != id).ToList();
        NotifyStateChanged();
    }

    // Draft actions
    public void UpdateDraft(Action<DraftState> updateAction)
    {
        updateAction(Draft);
        Draft.IsDirty = true;
        NotifyStateChanged();
    }
    
    public async Task SaveDraftAsync()
    {
        IsLoading = true; NotifyStateChanged();
        await Task.Delay(1000);
        Draft = new DraftState(); IsLoading = false;
        NotifyStateChanged();
    }
    
    public void DiscardDraft() { Draft = new DraftState(); NotifyStateChanged(); }

    // UI actions
    public void SetLoading(bool loading) { IsLoading = loading; NotifyStateChanged(); }
    public void ToggleCopilot() { CopilotExpanded = !CopilotExpanded; NotifyStateChanged(); }
}
```

---

### Step 3: Update Program.cs

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OutlookPlugin;
using OutlookPlugin.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register AppState as a singleton for global state management
builder.Services.AddSingleton<AppState>();

await builder.Build().RunAsync();
```

---

### Step 4: Update _Imports.razor (Root)

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.JSInterop
@using OutlookPlugin
@using OutlookPlugin.Services
@using OutlookPlugin.Components
@using OutlookPlugin.Components.Icons
@using OutlookPlugin.Components.Shared
@using OutlookPlugin.Components.Layout
@using OutlookPlugin.Components.Tabs
```

---

### Step 5: Create Components/_Imports.razor

```razor
@using OutlookPlugin.Components.Icons
```

---

### Step 6: Create App.razor (STATE-BASED ROUTING)

**CRITICAL**: Do NOT use Blazor's `<Router>` component. Outlook's iframe sandbox blocks `history.pushState`, causing navigation errors. Use state-based page switching instead.

This component also registers for Office.js `ItemChanged` events to detect when the user switches to a different email (plugin stays open).

```razor
@using OutlookPlugin.Services
@using OutlookPlugin.Pages
@using Microsoft.JSInterop
@inject AppState AppState
@inject OutlookService OutlookService
@implements IDisposable

@* State-based routing for Outlook iframe sandbox compatibility *@
@switch (AppState.CurrentPage)
{
    case AppPage.Auth:
        <Auth />
        break;
    case AppPage.Home:
        <Home />
        break;
    case AppPage.Email:
        <Email />
        break;
}

@code {
    protected override void OnInitialized()
    {
        AppState.OnChange += StateHasChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Register for email changed events to keep plugin in sync when user switches emails
            await OutlookService.RegisterItemChangedHandlerAsync(this);
        }
    }

    /// <summary>
    /// Called from JavaScript when the user switches to a different email.
    /// This keeps the plugin open and in sync with the currently selected email.
    /// </summary>
    [JSInvokable]
    public async Task OnEmailChanged(EmailInfo? emailInfo)
    {
        Console.WriteLine($"Email changed: {emailInfo?.Subject ?? "null"}");
        
        if (emailInfo != null)
        {
            var context = new EmailContext
            {
                ItemId = emailInfo.ItemId,
                ConversationId = emailInfo.ConversationId,
                Subject = emailInfo.Subject,
                SenderName = emailInfo.Sender?.DisplayName,
                SenderEmail = emailInfo.Sender?.EmailAddress,
                Body = emailInfo.Body,
                DateReceived = emailInfo.DateTimeCreated,
                ItemType = emailInfo.ItemType,
                Recipients = emailInfo.Recipients?.Select(r => new EmailParticipant
                {
                    Name = r.DisplayName,
                    Email = r.EmailAddress,
                    Type = r.RecipientType
                }).ToList()
            };
            
            AppState.SetEmailContext(context);
        }
        else
        {
            AppState.SetEmailContext(null);
        }
        
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        AppState.OnChange -= StateHasChanged;
    }
}
```

---

### Step 7: Create Icon Components

Each icon is a separate Razor component with an SVG. Example pattern:

**Components/Icons/SearchIcon.razor**
```razor
@namespace OutlookPlugin.Components.Icons

<svg class="@Class" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 20 20" fill="currentColor">
    <path d="M8.5 3a5.5 5.5 0 0 1 4.38 8.82l4.15 4.15a.75.75 0 0 1-.98 1.13l-.08-.07-4.15-4.15A5.5 5.5 0 1 1 8.5 3Zm0 1.5a4 4 0 1 0 0 8 4 4 0 0 0 0-8Z"/>
</svg>

@code {
    [Parameter] public string Class { get; set; } = "";
}
```

**Components/Icons/HomeIcon.razor** (for header navigation)
```razor
@namespace OutlookPlugin.Components.Icons

<svg class="@Class" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
    <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
    <polyline points="9 22 9 12 15 12 15 22"></polyline>
</svg>

@code {
    [Parameter] public string Class { get; set; } = "w-5 h-5";
}
```

**Create the following icons using the same pattern** (get SVG paths from Fluent UI Icons):
- SearchIcon, SettingsIcon, CalendarIcon, DismissIcon, PersonIcon, BuildingIcon
- ChevronDownIcon, ChevronUpIcon, SparkleIcon, MailIcon, PersonAddIcon, CheckmarkIcon
- MoreIcon, SaveIcon, DeleteIcon, SendIcon, ClockIcon, DocumentIcon, SyncIcon, FilterIcon
- CloudIcon, KeyIcon, HomeIcon

---

### Step 8: Create Layout Components

**Components/Layout/Header.razor** (with Home icon instead of text)
```razor
@namespace OutlookPlugin.Components.Layout
@using OutlookPlugin.Services
@inject AppState AppState

<header class="header">
    <div class="header-content">
        @if (AppState.SearchVisible)
        {
            <div class="flex items-center flex-1 gap-2">
                <input type="text"
                       class="input flex-1"
                       placeholder="Search contacts, accounts..."
                       value="@AppState.SearchQuery"
                       @oninput="HandleSearchInput"
                       autofocus />
                <button class="btn btn-subtle text-white" @onclick="AppState.ToggleSearch">
                    <DismissIcon />
                </button>
            </div>
        }
        else
        {
            <div class="flex items-center gap-2">
                <button class="btn btn-subtle text-white" @onclick="GoHome" title="Home">
                    <HomeIcon Class="w-5 h-5" />
                </button>
            </div>
            <div class="flex items-center gap-1">
                <button class="btn btn-subtle text-white" @onclick="AppState.ToggleSearch" title="Search">
                    <SearchIcon />
                </button>
                <button class="btn btn-subtle text-white" title="My Day">
                    <CalendarIcon />
                </button>
                <button class="btn btn-subtle text-white" title="Settings">
                    <SettingsIcon />
                </button>
            </div>
        }
    </div>
</header>

@code {
    protected override void OnInitialized()
    {
        AppState.OnChange += StateHasChanged;
    }

    private void GoHome()
    {
        AppState.SetActiveTab(ActiveTab.Related);
    }

    private void HandleSearchInput(ChangeEventArgs e)
    {
        AppState.SetSearchQuery(e.Value?.ToString() ?? "");
    }

    public void Dispose()
    {
        AppState.OnChange -= StateHasChanged;
    }
}
```

**Components/Layout/TabNavigation.razor**
- Three tabs: Related, Track, Actions
- Active tab styling with bottom border
- Dirty indicator (yellow dot) on Track tab when draft has unsaved changes
- Warns user before leaving Track tab with unsaved changes

**Components/Layout/CopilotPanel.razor**
- Expandable/collapsible panel showing discovered contacts
- Header with sparkle icon, "Copilot Discovery" title, count badge
- Lists DiscoveryCard components when expanded
- Only shows if there are discovered contacts

**Components/Layout/Shell.razor**
- Main layout wrapper combining Header, CopilotPanel, TabNavigation
- Parameters: ShowTabs, ShowCopilot (for auth page)
- Conditionally renders components based on parameters

---

### Step 9: Create Pages (NO URL ROUTING)

**Pages/Auth.razor** (NO @page directive needed - uses state-based routing)
```razor
@using OutlookPlugin.Services
@using OutlookPlugin.Components.Layout
@inject AppState AppState

<Shell ShowTabs="false" ShowCopilot="false">
    <div class="auth-container">
        <div class="w-full max-w-sm">
            @* Header *@
            <div class="text-center mb-8">
                <div class="auth-logo">
                    <CloudIcon Class="w-8 h-8 text-primary" />
                </div>
                <h1 class="text-xl font-bold text-gray-900 mb-2">Welcome to CRM Insight</h1>
                <p class="text-sm text-secondary">Connect your account to get started</p>
            </div>

            @if (mode == AuthMode.Connecting)
            {
                <div class="flex flex-col items-center gap-4 py-8 animate-fade-in">
                    <span class="spinner spinner-lg text-primary"></span>
                    <p class="text-sm text-secondary">Connecting to your account...</p>
                </div>
            }

            @if (mode == AuthMode.Select)
            {
                <div class="space-y-4 animate-fade-in">
                    @* Microsoft Sign In *@
                    <button class="btn btn-primary btn-lg w-full" @onclick="HandleMicrosoftSignIn">
                        <svg class="w-5 h-5" viewBox="0 0 21 21">
                            <rect x="1" y="1" width="9" height="9" fill="#f25022" />
                            <rect x="11" y="1" width="9" height="9" fill="#7fba00" />
                            <rect x="1" y="11" width="9" height="9" fill="#00a4ef" />
                            <rect x="11" y="11" width="9" height="9" fill="#ffb900" />
                        </svg>
                        Sign in with Microsoft
                    </button>

                    <div class="divider">or</div>

                    @* Salesforce Sign In *@
                    <button class="btn btn-secondary btn-lg w-full" @onclick="HandleSalesforceSignIn">
                        <svg class="w-5 h-5" viewBox="0 0 24 24" fill="#00A1E0">
                            <path d="M10.006 5.415c.699-.716 1.663-1.159 2.735-1.159 1.392 0 2.612.755 3.27 1.87a4.296 4.296 0 011.656-.332c2.39 0 4.333 1.943 4.333 4.333 0 2.39-1.943 4.333-4.333 4.333-.278 0-.55-.027-.813-.078-.576 1.278-1.87 2.167-3.367 2.167-.532 0-1.037-.111-1.497-.312-.674 1.43-2.134 2.423-3.823 2.423-1.78 0-3.304-1.101-3.929-2.661a3.604 3.604 0 01-.572.046c-2.007 0-3.633-1.626-3.633-3.633 0-1.476.881-2.746 2.142-3.315-.183-.471-.282-.981-.282-1.511 0-2.267 1.84-4.107 4.107-4.107 1.258 0 2.383.567 3.137 1.459l-.131-.523z" />
                        </svg>
                        Connect with Salesforce
                    </button>

                    <div class="divider">or use credentials</div>

                    @* Credentials option *@
                    <button class="btn btn-outline btn-lg w-full" @onclick="() => mode = AuthMode.Credentials">
                        <KeyIcon Class="w-5 h-5" />
                        Sign in with Credentials
                    </button>
                </div>
            }

            @if (mode == AuthMode.Credentials)
            {
                <div class="space-y-4 animate-fade-in">
                    <div>
                        <label class="block text-sm font-medium text-gray-700 mb-1">Username</label>
                        <input type="text" class="input" placeholder="Enter your username"
                               @bind="username" @bind:event="oninput" />
                    </div>
                    <div>
                        <label class="block text-sm font-medium text-gray-700 mb-1">Password</label>
                        <input type="password" class="input" placeholder="Enter your password"
                               @bind="password" @bind:event="oninput" />
                    </div>
                    <button class="btn btn-primary btn-lg w-full"
                            disabled="@(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))"
                            @onclick="HandleCredentialsSubmit">
                        Sign In
                    </button>
                    <button class="btn btn-subtle w-full" @onclick="() => mode = AuthMode.Select">
                        Back to sign in options
                    </button>
                </div>
            }
        </div>
    </div>
</Shell>

@code {
    private enum AuthMode { Select, Credentials, Connecting }
    private AuthMode mode = AuthMode.Select;
    private string username = "";
    private string password = "";

    private async Task HandleMicrosoftSignIn()
    {
        AppState.SetAuthProvider(AuthProvider.Microsoft);
        mode = AuthMode.Connecting;
        StateHasChanged();
        await Task.Delay(1500);
        // SetAuthStatus auto-navigates to Home page
        AppState.SetAuthStatus(AuthStatus.Authenticated);
    }

    private async Task HandleSalesforceSignIn()
    {
        AppState.SetAuthProvider(AuthProvider.Salesforce);
        mode = AuthMode.Connecting;
        StateHasChanged();
        // ConnectToCrmAsync sets auth and auto-navigates
        await AppState.ConnectToCrmAsync();
    }

    private async Task HandleCredentialsSubmit()
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return;
        mode = AuthMode.Connecting;
        StateHasChanged();
        await Task.Delay(1500);
        // SetAuthStatus auto-navigates to Home page
        AppState.SetAuthStatus(AuthStatus.Authenticated);
    }
}
```

**Pages/Home.razor** (NO @page directive - uses state-based routing)
```razor
@using OutlookPlugin.Services
@using OutlookPlugin.Components.Layout
@using OutlookPlugin.Components.Tabs
@inject AppState AppState
@inject OutlookService OutlookService
@implements IDisposable

<Shell ShowTabs="true" ShowCopilot="true">
    <div class="animate-fade-in">
        @* Email context banner *@
        @if (AppState.CurrentEmail != null && !string.IsNullOrEmpty(AppState.CurrentEmail.Subject))
        {
            <div class="p-3 bg-primary-light border-b text-sm">
                <div class="font-medium text-primary truncate">@AppState.CurrentEmail.Subject</div>
                <div class="text-secondary text-xs mt-1">
                    From: @(AppState.CurrentEmail.SenderName ?? AppState.CurrentEmail.SenderEmail)
                </div>
            </div>
        }
        
        @switch (AppState.ActiveTab)
        {
            case ActiveTab.Related:
                <RelatedTab />
                break;
            case ActiveTab.Track:
                <TrackTab />
                break;
            case ActiveTab.Actions:
                <ActionsTab />
                break;
        }
    </div>
</Shell>

@code {
    protected override void OnInitialized()
    {
        AppState.OnChange += StateHasChanged;
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadEmailContextAsync();
        }
    }
    
    private async Task LoadEmailContextAsync()
    {
        var emailInfo = await OutlookService.GetCurrentEmailInfoAsync();
        if (emailInfo != null)
        {
            var context = new EmailContext { ... }; // Map properties
            AppState.SetEmailContext(context);
        }
    }

    public void Dispose()
    {
        AppState.OnChange -= StateHasChanged;
    }
}
```

**Pages/Email.razor** - Full email details page (NO @page directive)
```razor
@using OutlookPlugin.Services
@using OutlookPlugin.Components.Layout
@inject AppState AppState
@inject OutlookService OutlookService
@implements IDisposable

<Shell ShowTabs="false" ShowCopilot="false">
    <div class="p-4 space-y-4">
        @* Header with back button *@
        <div class="flex items-center gap-3 mb-4">
            <button class="btn btn-subtle" @onclick="GoBack" title="Back to Home">
                <ChevronLeftIcon Class="w-5 h-5" />
            </button>
            <h1 class="text-lg font-semibold">Email Details</h1>
        </div>

        @if (isLoading)
        {
            <div class="flex flex-col items-center justify-center py-8">
                <span class="spinner spinner-lg text-primary mb-4"></span>
                <p class="text-sm text-secondary">Loading email information...</p>
            </div>
        }
        else if (AppState.CurrentEmail == null)
        {
            <div class="empty-state py-8">
                <MailIcon Class="w-12 h-12 text-secondary mx-auto mb-4" />
                <p class="text-secondary">No email selected</p>
            </div>
        }
        else
        {
            <div class="space-y-4 animate-fade-in">
                @* Subject *@
                <div class="card">
                    <div class="card-body">
                        <div class="text-xs text-secondary font-medium mb-1">SUBJECT</div>
                        <div class="font-semibold">@AppState.CurrentEmail.Subject</div>
                    </div>
                </div>

                @* Sender with avatar and quick track *@
                <div class="card">
                    <div class="card-body">
                        <div class="text-xs text-secondary font-medium mb-2">FROM</div>
                        <div class="flex items-center gap-3">
                            <div class="avatar avatar-contact">@GetInitials(AppState.CurrentEmail.SenderName)</div>
                            <div class="min-w-0 flex-1">
                                <div class="font-medium truncate">@AppState.CurrentEmail.SenderName</div>
                                <div class="text-sm text-secondary truncate">@AppState.CurrentEmail.SenderEmail</div>
                            </div>
                            <button class="btn btn-sm btn-primary">
                                <PersonAddIcon Class="w-4 h-4" /> Track
                            </button>
                        </div>
                    </div>
                </div>

                @* Recipients list *@
                @if (AppState.CurrentEmail.Recipients?.Any() == true)
                {
                    <div class="card">
                        <div class="card-body">
                            <div class="text-xs text-secondary font-medium mb-2">RECIPIENTS</div>
                            @foreach (var recipient in AppState.CurrentEmail.Recipients)
                            {
                                <div class="flex items-center gap-3 py-1">
                                    <div class="avatar avatar-sm">@GetInitials(recipient.Name)</div>
                                    <div class="text-sm">@(recipient.Name ?? recipient.Email)</div>
                                </div>
                            }
                        </div>
                    </div>
                }

                @* Date and metadata *@
                <div class="card">
                    <div class="card-body space-y-2">
                        <div>
                            <div class="text-xs text-secondary font-medium">DATE RECEIVED</div>
                            <div class="text-sm">@AppState.CurrentEmail.DateReceived?.ToString("f")</div>
                        </div>
                        <div>
                            <div class="text-xs text-secondary font-medium">TYPE</div>
                            <div class="text-sm">@AppState.CurrentEmail.ItemType</div>
                        </div>
                    </div>
                </div>

                @* Body preview *@
                @if (!string.IsNullOrEmpty(AppState.CurrentEmail.Body))
                {
                    <div class="card">
                        <div class="card-body">
                            <div class="text-xs text-secondary font-medium mb-2">BODY PREVIEW</div>
                            <div class="text-sm text-secondary max-h-48 overflow-y-auto">
                                @TruncateBody(AppState.CurrentEmail.Body, 500)
                            </div>
                        </div>
                    </div>
                }

                @* Actions *@
                <div class="flex gap-2 pt-2">
                    <button class="btn btn-primary flex-1" @onclick="TrackEmail">
                        <CheckmarkIcon /> Track Email
                    </button>
                    <button class="btn btn-secondary flex-1" @onclick="RefreshEmail">
                        <SyncIcon /> Refresh
                    </button>
                </div>
            </div>
        }
    </div>
</Shell>

@code {
    private bool isLoading = true;

    protected override void OnInitialized() => AppState.OnChange += StateHasChanged;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) await LoadEmailAsync();
    }

    private async Task LoadEmailAsync()
    {
        isLoading = true;
        StateHasChanged();
        
        var emailInfo = await OutlookService.GetCurrentEmailInfoAsync();
        if (emailInfo != null)
        {
            AppState.SetEmailContext(new EmailContext { ... }); // Map properties
        }
        
        isLoading = false;
        StateHasChanged();
    }
    
    private async Task RefreshEmail() => await LoadEmailAsync();
    private async Task TrackEmail() => await OutlookService.ShowNotificationAsync("Email tracked!");
    private void GoBack() => AppState.NavigateToPage(AppPage.Home);
    private string GetInitials(string? name) => /* extract initials */;
    private string TruncateBody(string body, int max) => body.Length > max ? body[..max] + "..." : body;

    public void Dispose() => AppState.OnChange -= StateHasChanged;
}
```

---

### Step 10: Create index.html with Office.js & History API Polyfill

**CRITICAL**: The index.html must include:
1. Office.js library for Outlook communication
2. History API polyfill to prevent errors in Outlook's sandboxed iframe

**wwwroot/index.html**
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>CRM Insight - Outlook Plugin</title>
    <base href="/" />
    <link rel="stylesheet" href="css/app.css" />
    <link rel="icon" type="image/png" href="favicon.png" />
    
    <!-- Office JavaScript API - Required for Outlook Add-ins -->
    <script src="https://appsforoffice.microsoft.com/lib/1/hosted/office.js" type="text/javascript"></script>
    
    <script>
        // CRITICAL: Polyfill history API for Outlook iframe sandbox
        // Outlook add-ins run in a sandboxed iframe where history.pushState/replaceState are blocked
        (function() {
            var currentPath = '/';
            
            // Create safe no-op versions that don't throw errors
            if (!window.history.pushState || typeof window.history.pushState !== 'function') {
                window.history.pushState = function(state, title, url) {
                    currentPath = url || currentPath;
                    console.log('[Outlook Sandbox] pushState intercepted:', url);
                };
            } else {
                var originalPushState = window.history.pushState.bind(window.history);
                window.history.pushState = function(state, title, url) {
                    try {
                        originalPushState(state, title, url);
                    } catch (e) {
                        currentPath = url || currentPath;
                        console.log('[Outlook Sandbox] pushState error caught, using fallback');
                    }
                };
            }
            
            if (!window.history.replaceState || typeof window.history.replaceState !== 'function') {
                window.history.replaceState = function(state, title, url) {
                    currentPath = url || currentPath;
                    console.log('[Outlook Sandbox] replaceState intercepted:', url);
                };
            } else {
                var originalReplaceState = window.history.replaceState.bind(window.history);
                window.history.replaceState = function(state, title, url) {
                    try {
                        originalReplaceState(state, title, url);
                    } catch (e) {
                        currentPath = url || currentPath;
                        console.log('[Outlook Sandbox] replaceState error caught, using fallback');
                    }
                };
            }
            
            console.log('[Outlook Sandbox] History API polyfill installed');
        })();
    </script>
</head>
<body>
    <div id="app" class="taskpane">
        <div class="flex items-center justify-center min-h-screen">
            <div class="text-center">
                <span class="spinner spinner-lg text-primary mb-4"></span>
                <p class="text-sm text-secondary">Loading CRM Insight...</p>
            </div>
        </div>
    </div>

    <div id="blazor-error-ui" style="display: none;">
        An unhandled error has occurred.
        <a href="" class="reload">Reload</a>
        <a class="dismiss">🗙</a>
    </div>

    <script>
        // Initialize Office.js
        Office.onReady(function(info) {
            console.log("Office.js initialized. Host: " + info.host + ", Platform: " + info.platform);
        });
    </script>
    <script src="_framework/blazor.webassembly.js"></script>
</body>
</html>
```

---

### Step 11: Create Development Manifest (`manifest.dev.xml`)

Use this for local development and testing:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OfficeApp
  xmlns="http://schemas.microsoft.com/office/appforoffice/1.1"
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
  xmlns:bt="http://schemas.microsoft.com/office/officeappbasictypes/1.0"
  xmlns:mailappor="http://schemas.microsoft.com/office/mailappversionoverrides/1.0"
  xsi:type="MailApp">

  <Id>a1b2c3d4-e5f6-7890-abcd-ef1234567890</Id>
  <Version>1.0.0.0</Version>
  <ProviderName>CRM Insight</ProviderName>
  <DefaultLocale>en-US</DefaultLocale>
  <DisplayName DefaultValue="CRM Insight"/>
  <Description DefaultValue="Connect your email to your CRM with contextual data and sales engagement tools."/>

  <IconUrl DefaultValue="https://localhost:5001/icon-64.png"/>
  <HighResolutionIconUrl DefaultValue="https://localhost:5001/icon-128.png"/>
  <SupportUrl DefaultValue="https://localhost:5001"/>

  <AppDomains>
    <AppDomain>https://localhost:5001</AppDomain>
  </AppDomains>

  <Hosts><Host Name="Mailbox"/></Hosts>

  <Requirements>
    <Sets><Set Name="Mailbox" MinVersion="1.1"/></Sets>
  </Requirements>

  <FormSettings>
    <Form xsi:type="ItemRead">
      <DesktopSettings>
        <SourceLocation DefaultValue="https://localhost:5001"/>
        <RequestedHeight>450</RequestedHeight>
      </DesktopSettings>
    </Form>
  </FormSettings>

  <Permissions>ReadWriteItem</Permissions>

  <Rule xsi:type="RuleCollection" Mode="Or">
    <Rule xsi:type="ItemIs" ItemType="Message" FormType="Read"/>
    <Rule xsi:type="ItemIs" ItemType="Appointment" FormType="Read"/>
  </Rule>

  <VersionOverrides xmlns="http://schemas.microsoft.com/office/mailappversionoverrides" xsi:type="VersionOverridesV1_0">
    <Requirements>
      <bt:Sets DefaultMinVersion="1.3"><bt:Set Name="Mailbox"/></bt:Sets>
    </Requirements>

    <Hosts>
      <Host xsi:type="MailHost">
        <DesktopFormFactor>
          <ExtensionPoint xsi:type="MessageReadCommandSurface">
            <OfficeTab id="TabDefault">
              <Group id="msgReadGroup">
                <Label resid="GroupLabel"/>
                <Control xsi:type="Button" id="msgReadOpenPaneButton">
                  <Label resid="TaskpaneButton.Label"/>
                  <Supertip>
                    <Title resid="TaskpaneButton.Label"/>
                    <Description resid="TaskpaneButton.Tooltip"/>
                  </Supertip>
                  <Icon>
                    <bt:Image size="16" resid="Icon.16x16"/>
                    <bt:Image size="32" resid="Icon.32x32"/>
                    <bt:Image size="80" resid="Icon.80x80"/>
                  </Icon>
                  <Action xsi:type="ShowTaskpane">
                    <SourceLocation resid="Taskpane.Url"/>
                  </Action>
                </Control>
              </Group>
            </OfficeTab>
          </ExtensionPoint>

          <ExtensionPoint xsi:type="MessageComposeCommandSurface">
            <OfficeTab id="TabDefault">
              <Group id="msgComposeGroup">
                <Label resid="GroupLabel"/>
                <Control xsi:type="Button" id="msgComposeOpenPaneButton">
                  <Label resid="TaskpaneButton.Label"/>
                  <Supertip>
                    <Title resid="TaskpaneButton.Label"/>
                    <Description resid="TaskpaneButton.Tooltip"/>
                  </Supertip>
                  <Icon>
                    <bt:Image size="16" resid="Icon.16x16"/>
                    <bt:Image size="32" resid="Icon.32x32"/>
                    <bt:Image size="80" resid="Icon.80x80"/>
                  </Icon>
                  <Action xsi:type="ShowTaskpane">
                    <SourceLocation resid="Taskpane.Url"/>
                  </Action>
                </Control>
              </Group>
            </OfficeTab>
          </ExtensionPoint>

          <ExtensionPoint xsi:type="AppointmentOrganizerCommandSurface">
            <OfficeTab id="TabDefault">
              <Group id="apptOrganizerGroup">
                <Label resid="GroupLabel"/>
                <Control xsi:type="Button" id="apptOrganizerOpenPaneButton">
                  <Label resid="TaskpaneButton.Label"/>
                  <Supertip>
                    <Title resid="TaskpaneButton.Label"/>
                    <Description resid="TaskpaneButton.Tooltip"/>
                  </Supertip>
                  <Icon>
                    <bt:Image size="16" resid="Icon.16x16"/>
                    <bt:Image size="32" resid="Icon.32x32"/>
                    <bt:Image size="80" resid="Icon.80x80"/>
                  </Icon>
                  <Action xsi:type="ShowTaskpane">
                    <SourceLocation resid="Taskpane.Url"/>
                  </Action>
                </Control>
              </Group>
            </OfficeTab>
          </ExtensionPoint>

          <ExtensionPoint xsi:type="AppointmentAttendeeCommandSurface">
            <OfficeTab id="TabDefault">
              <Group id="apptAttendeeGroup">
                <Label resid="GroupLabel"/>
                <Control xsi:type="Button" id="apptAttendeeOpenPaneButton">
                  <Label resid="TaskpaneButton.Label"/>
                  <Supertip>
                    <Title resid="TaskpaneButton.Label"/>
                    <Description resid="TaskpaneButton.Tooltip"/>
                  </Supertip>
                  <Icon>
                    <bt:Image size="16" resid="Icon.16x16"/>
                    <bt:Image size="32" resid="Icon.32x32"/>
                    <bt:Image size="80" resid="Icon.80x80"/>
                  </Icon>
                  <Action xsi:type="ShowTaskpane">
                    <SourceLocation resid="Taskpane.Url"/>
                  </Action>
                </Control>
              </Group>
            </OfficeTab>
          </ExtensionPoint>

        </DesktopFormFactor>
      </Host>
    </Hosts>

    <Resources>
      <bt:Images>
        <bt:Image id="Icon.16x16" DefaultValue="https://localhost:5001/icon-16.png"/>
        <bt:Image id="Icon.32x32" DefaultValue="https://localhost:5001/icon-32.png"/>
        <bt:Image id="Icon.80x80" DefaultValue="https://localhost:5001/icon-80.png"/>
      </bt:Images>
      <bt:Urls>
        <bt:Url id="Taskpane.Url" DefaultValue="https://localhost:5001"/>
      </bt:Urls>
      <bt:ShortStrings>
        <bt:String id="GroupLabel" DefaultValue="CRM Insight"/>
        <bt:String id="TaskpaneButton.Label" DefaultValue="CRM Insight"/>
      </bt:ShortStrings>
      <bt:LongStrings>
        <bt:String id="TaskpaneButton.Tooltip" DefaultValue="Open CRM Insight to view contextual CRM data and track emails"/>
      </bt:LongStrings>
    </Resources>

  </VersionOverrides>

</OfficeApp>
```

---

### Step 12: Create Production Manifest Template (`manifest.xml`)

For production deployment, create a template that requires placeholder replacement:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!--
  PRODUCTION MANIFEST TEMPLATE
  
  Before deployment, replace ALL placeholder values:
  1. Generate a new GUID at https://www.guidgenerator.com/
  2. Replace YOUR_DOMAIN.com with your actual domain
  3. Ensure all icon files are uploaded to your server
  4. Update SupportUrl to a real support page
-->
<OfficeApp
  xmlns="http://schemas.microsoft.com/office/appforoffice/1.1"
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
  xmlns:bt="http://schemas.microsoft.com/office/officeappbasictypes/1.0"
  xmlns:mailappor="http://schemas.microsoft.com/office/mailappversionoverrides/1.0"
  xsi:type="MailApp">

  <!-- REQUIRED: Generate a unique GUID -->
  <Id>GENERATE-NEW-GUID-HERE</Id>
  <Version>1.0.0.0</Version>
  <ProviderName>Your Company Name</ProviderName>
  <DefaultLocale>en-US</DefaultLocale>
  <DisplayName DefaultValue="CRM Insight"/>
  <Description DefaultValue="Connect your email to your CRM with contextual data and sales engagement tools."/>

  <!-- REQUIRED: Replace with your production URLs -->
  <IconUrl DefaultValue="https://YOUR_DOMAIN.com/icon-64.png"/>
  <HighResolutionIconUrl DefaultValue="https://YOUR_DOMAIN.com/icon-128.png"/>
  <SupportUrl DefaultValue="https://YOUR_DOMAIN.com/support"/>

  <AppDomains>
    <AppDomain>https://YOUR_DOMAIN.com</AppDomain>
  </AppDomains>

  <!-- ... rest of manifest same as dev version but with YOUR_DOMAIN.com URLs ... -->

</OfficeApp>
```

---

## Running the Application

```bash
# Trust HTTPS certificate (first time only)
dotnet dev-certs https --trust

# Update launchSettings.json to use port 5001
# Set applicationUrl to "https://localhost:5001;http://localhost:5000"

# Run with HTTPS (required for Office Add-in)
dotnet run --launch-profile https

# Or specify URLs directly
dotnet run --urls "https://localhost:5001"
```

---

## Sideloading for Testing

1. Start app with HTTPS on port 5001
2. Open Outlook on the web (outlook.office.com)
3. Settings (gear) → View all Outlook settings
4. Mail → Customize actions → scroll to Add-ins
5. Custom add-ins → Add a custom add-in → Add from file
6. Upload `manifest.dev.xml`
7. Refresh Outlook, open any email, click "CRM Insight" button

---

## Key Implementation Notes

### Outlook Iframe Sandbox Compatibility

1. **NO URL-based routing**: Outlook blocks `history.pushState`. Use state-based navigation via `AppState.CurrentPage`
2. **History API Polyfill**: Include in index.html to prevent errors from any remaining history API calls
3. **Office.js required**: Load from Microsoft CDN before Blazor for proper add-in communication

### State Management

1. Use `AppState.OnChange += StateHasChanged` in `OnInitialized`
2. Always unsubscribe in `Dispose` method
3. Auth status changes auto-navigate to appropriate page
4. Tab navigation uses `SetActiveTab()`, not URL changes

### Component Patterns

1. **Icon Components**: Each icon is a separate `.razor` file with `[Parameter] public string Class`
2. **Event Callbacks**: Use `EventCallback` for parent-child communication
3. **Namespaces**: Use `@namespace` directive to organize components
4. **Records for DTOs**: Use C# records for immutable data transfer objects

---

## Feature Checklist

- [x] State-based navigation (no URL routing)
- [x] History API polyfill for Outlook sandbox
- [x] Office.js integration
- [x] **Outlook email interop** (read subject, sender, recipients, body)
- [x] **Auto-discover contacts** from email participants
- [x] Authentication page with Microsoft/Salesforce/Credentials options
- [x] Header with home icon, search, calendar, settings buttons
- [x] Tab navigation (Related, Track, Actions) with active states
- [x] Related tab with collapsible Contact/Account/Event sections
- [x] Search field with filter dropdown
- [x] Entity cards with avatar, info, status badge, track button
- [x] Track tab with autocomplete inputs and notes
- [x] Draft state management with dirty indicator
- [x] Discard/Save functionality
- [x] Actions tab with available/completed actions
- [x] Copilot discovery panel with quick track
- [x] Responsive 350px task pane layout
- [x] Office Add-in manifests (dev and production)
- [x] No Bootstrap - pure custom CSS with Fluent UI design
- [x] Email details page with full email info display
- [x] ItemChanged event handler - plugin stays open when switching emails
- [x] Auto-refresh email context on email switch

---

## Outlook Email Interop

The `OutlookService` provides methods to interact with the current email:

```csharp
@inject OutlookService OutlookService

// Get current email info
var emailInfo = await OutlookService.GetCurrentEmailInfoAsync();

// Get specific properties
var subject = await OutlookService.GetEmailSubjectAsync();
var sender = await OutlookService.GetEmailSenderAsync();
var recipients = await OutlookService.GetEmailRecipientsAsync();
var body = await OutlookService.GetEmailBodyAsync();

// Show notification in Outlook
await OutlookService.ShowNotificationAsync("Email tracked successfully!");

// Add category to email
await OutlookService.AddCategoryAsync("CRM Tracked");
```

### Available Methods

| Method | Description |
|--------|-------------|
| `GetEmailSubjectAsync()` | Get email subject line |
| `GetEmailSenderAsync()` | Get sender name and email |
| `GetEmailRecipientsAsync()` | Get all To/CC recipients |
| `GetEmailBodyAsync()` | Get email body as text |
| `GetEmailIdAsync()` | Get unique email ID |
| `GetConversationIdAsync()` | Get conversation thread ID |
| `GetEmailDateAsync()` | Get email received date |
| `GetCurrentEmailInfoAsync()` | Get all email info at once |
| `GetCurrentUserEmailAsync()` | Get logged-in user's email |
| `GetItemTypeAsync()` | Get item type (message/appointment) |
| `ShowNotificationAsync()` | Show notification in Outlook |
| `AddCategoryAsync()` | Add category/label to email |
| `RegisterItemChangedHandlerAsync()` | Register for email switch events |
| `UnregisterItemChangedHandlerAsync()` | Unregister email switch handler |

### Email Change Detection (Plugin Stays Open)

The plugin automatically detects when the user switches to a different email. This is implemented via the Office.js `ItemChanged` event:

**App.razor** - Registers the handler and receives callbacks:
```razor
@using Microsoft.JSInterop
@inject OutlookService OutlookService

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Register for email changed events
            await OutlookService.RegisterItemChangedHandlerAsync(this);
        }
    }

    /// <summary>
    /// Called from JavaScript when the user switches to a different email
    /// </summary>
    [JSInvokable]
    public async Task OnEmailChanged(EmailInfo? emailInfo)
    {
        if (emailInfo != null)
        {
            var context = new EmailContext { ... };
            AppState.SetEmailContext(context);
        }
        await InvokeAsync(StateHasChanged);
    }
}
```

**JavaScript interop** (in index.html):
```javascript
window.outlookInterop = {
    _dotNetHelper: null,
    
    registerItemChangedHandler: function(dotNetHelper) {
        this._dotNetHelper = dotNetHelper;
        Office.context.mailbox.addHandlerAsync(
            Office.EventType.ItemChanged,
            this._onItemChanged.bind(this)
        );
    },
    
    _onItemChanged: function(eventArgs) {
        if (this._dotNetHelper) {
            this.getCurrentEmailInfo().then(function(emailInfo) {
                this._dotNetHelper.invokeMethodAsync('OnEmailChanged', emailInfo);
            }.bind(this));
        }
    }
};
```

---

## Troubleshooting

### "history.pushState is not a function" Error
This error occurs if Blazor's Router is used. Solution: Use state-based routing as shown in App.razor above.

### Add-in shows blank page
1. Ensure server is running on https://localhost:5001
2. Check browser console for errors
3. Verify History API polyfill is in index.html
4. Ensure Office.js is loaded before blazor.webassembly.js

### Icons not loading
1. Ensure PNG files exist in wwwroot (not text placeholders)
2. Check manifest URLs match your server port
3. Re-sideload the add-in after changing manifest

### Port already in use
```powershell
# Find and kill process using port 5001
Get-Process -Name dotnet | Stop-Process -Force
```

---

*This prompt provides complete specifications for recreating the Outlook Web Plugin in Blazor WebAssembly with full Outlook iframe sandbox compatibility.*

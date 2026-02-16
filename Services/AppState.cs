namespace OutlookPlugin.Services;

public enum AuthStatus
{
    Unauthenticated,
    Authenticated,
    Connecting
}

public enum AuthProvider
{
    None,
    Microsoft,
    Salesforce
}

public enum ActiveTab
{
    Related,
    Track,
    Actions
}

public enum AppPage
{
    Auth,
    Home,
    Email
}

public enum FilterType
{
    All,
    Contacts,
    Accounts,
    Events
}

public record Contact(
    string Id,
    string Name,
    string Email,
    string? Company,
    bool IsTracked,
    string? AvatarInitials
);

public record Account(
    string Id,
    string Name,
    string Type,
    bool IsTracked
);

public record CalendarEvent(
    string Id,
    string Title,
    DateTime Date,
    string Time,
    List<string> Attendees,
    bool IsTracked
);

public record DiscoveredContact(
    string Id,
    string Email,
    string? Name,
    bool IsNew
);

public class DraftState
{
    public List<string> RelatedContacts { get; set; } = new();
    public List<string> RelatedAccounts { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
    public bool IsDirty { get; set; }
}

public class AppState
{
    // Events for state change notifications
    public event Action? OnChange;
    
    // Auth state
    public AuthStatus AuthStatus { get; private set; } = AuthStatus.Unauthenticated;
    public AuthProvider AuthProvider { get; private set; } = AuthProvider.None;
    public bool CrmConnected { get; private set; }
    
    // Navigation state
    public AppPage CurrentPage { get; private set; } = AppPage.Auth;
    public ActiveTab ActiveTab { get; private set; } = ActiveTab.Related;
    public string SearchQuery { get; private set; } = string.Empty;
    public bool SearchVisible { get; private set; }
    
    // Data
    public List<Contact> Contacts { get; private set; } = new();
    public List<Account> Accounts { get; private set; } = new();
    public List<CalendarEvent> Events { get; private set; } = new();
    public List<DiscoveredContact> DiscoveredContacts { get; private set; } = new();
    
    // Draft state
    public DraftState Draft { get; private set; } = new();
    
    // Current email context from Outlook
    public EmailContext? CurrentEmail { get; private set; }
    
    // UI state
    public bool IsLoading { get; private set; }
    public bool CopilotExpanded { get; private set; } = true;
    
    public AppState()
    {
        InitializeMockData();
    }
    
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
    
    private void NotifyStateChanged() => OnChange?.Invoke();
    
    // Auth actions
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
        IsLoading = true;
        NotifyStateChanged();
        
        await Task.Delay(1500); // Simulate API call
        
        CrmConnected = true;
        AuthStatus = AuthStatus.Authenticated;
        IsLoading = false;
        NotifyStateChanged();
    }
    
    public void SignOut()
    {
        AuthStatus = AuthStatus.Unauthenticated;
        AuthProvider = AuthProvider.None;
        CrmConnected = false;
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
        if (!SearchVisible) SearchQuery = string.Empty;
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
        IsLoading = true;
        NotifyStateChanged();
        
        await Task.Delay(1000); // Simulate API call
        
        Draft = new DraftState();
        IsLoading = false;
        NotifyStateChanged();
    }
    
    public void DiscardDraft()
    {
        Draft = new DraftState();
        NotifyStateChanged();
    }
    
    // UI actions
    public void SetLoading(bool loading)
    {
        IsLoading = loading;
        NotifyStateChanged();
    }
    
    public void ToggleCopilot()
    {
        CopilotExpanded = !CopilotExpanded;
        NotifyStateChanged();
    }
    
    // Email context actions
    public void SetEmailContext(EmailContext? context)
    {
        CurrentEmail = context;
        
        // Auto-discover contacts from email participants
        if (context != null)
        {
            DiscoverContactsFromEmail(context);
        }
        
        NotifyStateChanged();
    }
    
    private void DiscoverContactsFromEmail(EmailContext context)
    {
        var newDiscovered = new List<DiscoveredContact>();
        
        // Check sender
        if (!string.IsNullOrEmpty(context.SenderEmail))
        {
            var existingContact = Contacts.FirstOrDefault(c => 
                c.Email.Equals(context.SenderEmail, StringComparison.OrdinalIgnoreCase));
            
            if (existingContact == null)
            {
                newDiscovered.Add(new DiscoveredContact(
                    $"d-{Guid.NewGuid():N}",
                    context.SenderEmail,
                    context.SenderName,
                    true
                ));
            }
        }
        
        // Check recipients
        if (context.Recipients != null)
        {
            foreach (var recipient in context.Recipients)
            {
                var existingContact = Contacts.FirstOrDefault(c => 
                    c.Email.Equals(recipient.Email, StringComparison.OrdinalIgnoreCase));
                
                if (existingContact == null && 
                    !newDiscovered.Any(d => d.Email.Equals(recipient.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    newDiscovered.Add(new DiscoveredContact(
                        $"d-{Guid.NewGuid():N}",
                        recipient.Email,
                        recipient.Name,
                        true
                    ));
                }
            }
        }
        
        // Update discovered contacts (merge with existing)
        foreach (var contact in newDiscovered)
        {
            if (!DiscoveredContacts.Any(d => d.Email.Equals(contact.Email, StringComparison.OrdinalIgnoreCase)))
            {
                DiscoveredContacts.Add(contact);
            }
        }
    }
}

/// <summary>
/// Represents the current email context from Outlook
/// </summary>
public class EmailContext
{
    public string? ItemId { get; set; }
    public string? ConversationId { get; set; }
    public string? Subject { get; set; }
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    public List<EmailParticipant>? Recipients { get; set; }
    public string? Body { get; set; }
    public DateTime? DateReceived { get; set; }
    public string? ItemType { get; set; }
}

public class EmailParticipant
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Type { get; set; } // To, Cc, Bcc
}

using Microsoft.JSInterop;

namespace OutlookPlugin.Services;

/// <summary>
/// Service for interacting with Outlook via Office.js API
/// </summary>
public class OutlookService
{
    private readonly IJSRuntime _jsRuntime;

    public OutlookService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Returns true if the app is running inside Outlook as an add-in (task pane). False if opened in a normal browser tab.
    /// </summary>
    public async Task<bool> IsRunningInOutlookAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("outlookInterop.isRunningInOutlook");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error detecting Outlook host: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the current email item's subject
    /// </summary>
    public async Task<string?> GetEmailSubjectAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("outlookInterop.getSubject");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting email subject: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets the current email sender information
    /// </summary>
    public async Task<EmailSender?> GetEmailSenderAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<EmailSender>("outlookInterop.getSender");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting email sender: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets all recipients (To, CC) from the current email
    /// </summary>
    public async Task<List<EmailRecipient>> GetEmailRecipientsAsync()
    {
        try
        {
            var recipients = await _jsRuntime.InvokeAsync<EmailRecipient[]>("outlookInterop.getRecipients");
            return recipients?.ToList() ?? new List<EmailRecipient>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting email recipients: {ex.Message}");
            return new List<EmailRecipient>();
        }
    }

    /// <summary>
    /// Gets the email body as text
    /// </summary>
    public async Task<string?> GetEmailBodyAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("outlookInterop.getBody");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting email body: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets the email's unique ID
    /// </summary>
    public async Task<string?> GetEmailIdAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("outlookInterop.getItemId");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting email ID: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets the email's conversation ID
    /// </summary>
    public async Task<string?> GetConversationIdAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("outlookInterop.getConversationId");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting conversation ID: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets the email's date/time received
    /// </summary>
    public async Task<DateTime?> GetEmailDateAsync()
    {
        try
        {
            var dateString = await _jsRuntime.InvokeAsync<string>("outlookInterop.getDateTimeCreated");
            if (DateTime.TryParse(dateString, out var date))
            {
                return date;
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting email date: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets complete email information
    /// </summary>
    public async Task<EmailInfo?> GetCurrentEmailInfoAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<EmailInfo>("outlookInterop.getCurrentEmailInfo");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting email info: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Adds a category/label to the current email
    /// </summary>
    public async Task<bool> AddCategoryAsync(string category)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("outlookInterop.addCategory", category);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding category: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Shows a notification in Outlook
    /// </summary>
    public async Task ShowNotificationAsync(string message, NotificationType type = NotificationType.Informational)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("outlookInterop.showNotification", message, type.ToString().ToLower());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the current user's email address
    /// </summary>
    public async Task<string?> GetCurrentUserEmailAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("outlookInterop.getCurrentUserEmail");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting current user email: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if the current item is a message or appointment
    /// </summary>
    public async Task<string?> GetItemTypeAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("outlookInterop.getItemType");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting item type: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Registers a handler for when the user switches to a different email
    /// </summary>
    public async Task RegisterItemChangedHandlerAsync<T>(T callbackHandler) where T : class
    {
        try
        {
            var dotNetRef = DotNetObjectReference.Create(callbackHandler);
            await _jsRuntime.InvokeVoidAsync("outlookInterop.registerItemChangedHandler", dotNetRef);
            Console.WriteLine("Item changed handler registered");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering item changed handler: {ex.Message}");
        }
    }

    /// <summary>
    /// Unregisters the item changed handler
    /// </summary>
    public async Task UnregisterItemChangedHandlerAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("outlookInterop.unregisterItemChangedHandler");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error unregistering item changed handler: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the location of the current item (appointments only).
    /// </summary>
    public async Task<string?> GetLocationAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("outlookInterop.getLocation");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting location: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets attendees (organizer, required, optional) for the current appointment/meeting.
    /// </summary>
    public async Task<List<MeetingAttendee>> GetAttendeesAsync()
    {
        try
        {
            var attendees = await _jsRuntime.InvokeAsync<MeetingAttendee[]>("outlookInterop.getAttendees");
            return attendees?.ToList() ?? new List<MeetingAttendee>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting attendees: {ex.Message}");
            return new List<MeetingAttendee>();
        }
    }

    /// <summary>
    /// Gets meeting series recurrence info for the current appointment/meeting (if it is a recurring series). Returns null for single appointments or emails. Requires Mailbox 1.7.
    /// </summary>
    public async Task<MeetingSeriesInfo?> GetMeetingSeriesInfoAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<MeetingSeriesInfo?>("outlookInterop.getMeetingSeriesInfo");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting meeting series info: {ex.Message}");
            return null;
        }
    }
}

// DTOs for email data
public record EmailSender(string DisplayName, string EmailAddress);

public record EmailRecipient(string DisplayName, string EmailAddress, string RecipientType);

public record EmailInfo(
    string? ItemId,
    string? ConversationId,
    string? Subject,
    EmailSender? Sender,
    List<EmailRecipient>? Recipients,
    string? Body,
    DateTime? DateTimeCreated,
    string? ItemType
);

public enum NotificationType
{
    Informational,
    Progress,
    Error
}

// DTOs for meeting series (recurrence)
public record MeetingSeriesInfo(
    string? Subject,
    string? Location,
    string? RecurrenceType,
    string? StartDate,
    string? EndDate,
    string? StartTime,
    string? EndTime,
    int? DurationMinutes,
    RecurrencePropertiesDto? RecurrenceProperties,
    RecurrenceTimeZoneDto? RecurrenceTimeZone
);

public record RecurrencePropertiesDto(
    int? Interval,
    string[]? Days,
    string? DayOfWeek,
    int? DayOfMonth,
    string? Month,
    string? WeekNumber,
    string? FirstDayOfWeek
);

public record RecurrenceTimeZoneDto(string? Name, int? Offset);

public record MeetingAttendee(string? DisplayName, string? EmailAddress, string? Type);

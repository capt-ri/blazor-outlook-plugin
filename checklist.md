# Plugin & Recurring Meeting — Consolidated Notes

---

## Plugin Overview

- The plugin lives inside **Outlook** and is **always on** — it cannot be selectively turned off.
- It is always connected to Salesforce regardless of which account the user is logged into.
- When a user opens a meeting in Outlook, the plugin panel activates automatically and captures all changes.
- The **optionality to sync** applies to **emails only** — for meetings, the plugin captures everything automatically.

---

## GM vs IB Plugin

| | IB Plugin | GM Plugin |
|---|---|---|
| **Validation** | ✅ Working, validated | ❌ Pending — event validation Apex fix needed |
| **Delete behavior** | Soft delete | Hard delete |
| **ORCAS API payload** | Different form | Different form |
| **Approach** | Validated | Will follow same pattern as IB (update + delete) |

### Delete Standardization
- The inconsistency between GM (hard delete) and IB (soft delete) is proving **cumbersome**.
- Goal is to **standardize the delete behavior** across both plugins.
- This is the **only thing being standardized** — the ORCAS API payload/form will remain different between GM and IB.

---

## Plugin Deployment

- The plugin was **deployed** at the time of the original meeting.
- Roman to confirm with pilot user that the plugin is visible and functioning.
- Once confirmed → **"we are in business"**.

---

## How the Plugin Handles Errors

- If a meeting contains a contact that **does not exist in Salesforce**, the plugin:
  - Immediately **flashes the error** on the Outlook panel.
  - Gives the user two options:
    - **Create the contact** in Salesforce, or
    - **Cancel the meeting update** entirely.
- This is a **fail-safe** — errors are surfaced in real time before anything is pushed to Salesforce.
- The plugin shows this error even if the user did not explicitly open the plugin panel — because it is always running.
- For recurring meetings synced by the sync engine, only the **specific failing occurrence** is affected — not the entire series.

---

## Single Event Flow (Confirmed by Vipin)

1. Plugin syncs the event **directly to Salesforce**
2. Salesforce returns an **SF ID**
3. Plugin sends the SF ID (+ user email, Exchange ID, etc.) to **ORCAS database**
4. Next time the same meeting is opened → plugin **fetches the SF ID from ORCAS** to pre-populate existing data
5. Plugin uses that SF ID to **update the exact record** in Salesforce

> ⚠️ **New requirement for Roman** — previously the plugin never tracked the SF ID. Now it must capture it and pass it to ORCAS on every Salesforce push.

---

## Recurring Meeting Flow (Confirmed by Vipin)

### Step 1 — Master Event
- Plugin detects the **recurring flag** on the meeting
- Plugin sends the **master event to ORCAS only** — never to Salesforce directly
- ORCAS stores the master event in its database

### Step 2 — Occurrences (ORCAS handles)
- On its next cycle, ORCAS connects to **Exchange**
- Retrieves all occurrences based on the recurrence frequency/pattern
- Pushes all occurrences **directly to Salesforce**
- Stores the **SF ID** for each occurrence in the database

### Step 3 — Amendments & Modifications
- Any amendments to a recurring meeting are handled by the **plugin**
- Plugin pushes the change to Salesforce
- Plugin **must notify ORCAS** and record it as "synced" in the database
- This applies to all meetings — not just recurring

### Modified Occurrences (Exceptions)
- If a single occurrence is modified, plugin sends **full details to ORCAS**
- ORCAS pushes the change to Salesforce and stores the SF ID

### Occurrence Deletions
- Plugin sends a **delete notification to ORCAS**
- ORCAS deletes it from Salesforce and marks it as deleted in the database
- Plugin must never delete directly without telling ORCAS — the sync engine would never know the event was deleted

---

## Why the Plugin Must Always Notify ORCAS

- ORCAS runs a **background sync cycle** checking Outlook for changes since the last run
- If the plugin modifies or syncs something **without notifying ORCAS**, the sync engine will detect the change in its next cycle and try to sync it again → **double-sync conflict**
- Anything the plugin syncs must be **recorded in ORCAS as "synced"** so the sync engine skips it
- Most of the time, the sync engine will do nothing — its primary job is handling **new recurring master events**

---

## What ORCAS / Sync Engine Does

| Responsibility | Detail |
|---|---|
| **Recurring master** | Receives from plugin, stores in DB, retrieves occurrences from Exchange, pushes to SF |
| **Single events** | Receives SF ID from plugin, records as "synced", skips on next cycle |
| **Exceptions** | Receives full details from plugin, pushes to SF, stores SF ID |
| **Deletions** | Receives delete notification, deletes from SF, marks deleted in DB |
| **Background cycle** | Checks Outlook for changes since last run — skips anything already marked synced |

---

## Recurring Flag — Roman's Responsibility

- Roman must **explicitly track the recurring flag** in the plugin
- This determines whether the master event is held back from Salesforce and sent to ORCAS instead
- Considered a **minor change** but critical to correct routing

---

## What to Pull from Outlook for Recurring Meetings (POC Scope)

### Master Event
- Exchange ID of the master
- Recurring flag
- Meeting subject / title
- Organizer email
- Attendees list
- Recurrence pattern (daily, weekly, monthly etc.)
- Series start & end date

### Each Occurrence
- Exchange ID of the occurrence
- Parent/Master Exchange ID
- Occurrence date & time
- Occurrence index (1st, 2nd etc. in the series)

### Modified Occurrences (Exceptions)
- Exchange ID of the modified occurrence
- What changed (time, attendees, subject etc.)
- Original scheduled date
- New date/time after modification

### POC Goals
1. Detect the **recurring flag** from an Outlook calendar event
2. Distinguish the **master event** from its occurrences
3. Get the **Exchange ID** for both master and individual occurrences
4. Detect **exceptions** — occurrences modified from the original pattern

### Suggested API Approach
- `GET /me/events` — list calendar events
- `GET /me/events/{id}/instances` — get all occurrences of a recurring event
- `GET /me/events/{id}` — master event with recurrence details

---

## Event Routing Summary

| Event Type | Plugin does | ORCAS does |
|---|---|---|
| **Single create/update/delete** | Pushes to SF directly, sends SF ID to ORCAS | Stores record, skips on background cycle |
| **Recurring master** | Sends to ORCAS only | Stores master, retrieves occurrences from Exchange, pushes to SF |
| **Exception (modified occurrence)** | Sends full details to ORCAS | Pushes to SF, stores SF ID |
| **Occurrence deletion** | Sends delete notification to ORCAS | Deletes from SF, marks deleted |

---

## Open Items & Next Steps

| Who | What |
|---|---|
| **Roman** | Fix GM event validation — Apex update needed |
| **Roman** | Track recurring flag explicitly in plugin |
| **Roman** | Capture SF ID after every Salesforce push and send to ORCAS |
| **Roman** | Fetch SF ID from ORCAS when opening existing meetings to pre-populate data |
| **Roman** | Confirm pilot user can see deployed plugin |
| **Roman & Sanika** | Run mock exercise — single event sync + recurring master to ORCAS only |
| **Sanika** | Continue test case coverage (target: 80%) |
| **Team** | Align on delete standardization (hard vs soft) across GM and IB |

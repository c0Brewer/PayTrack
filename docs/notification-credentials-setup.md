# Notification Credentials Setup

The notification dispatch service (`/api/v1/notify/email` and `/api/v1/notify/slack`) requires
SMTP and Slack credentials to deliver messages. This document covers setup for both local
development and production deployment.

---

## Configuration structure

The backend reads credentials from `appsettings.json` (committed, values left empty) and
overrides from `appsettings.Development.json` (not committed — add to `.gitignore` if missing).

```
backend/PayTrack/
  appsettings.json              ← committed, keys present, values empty
  appsettings.Development.json  ← local only, never commit, holds real dev credentials
```

For production the same keys are supplied via environment variables (see [Production](#production)).

---

## Local development

Create `backend/PayTrack/appsettings.Development.json` if it does not exist:

```json
{
  "Email": {
    "SmtpHost": "...",
    "SmtpPort": 587,
    "SmtpUser": "...",
    "SmtpPassword": "...",
    "FromAddress": "..."
  },
  "Slack": {
    "BotToken": "xoxb-..."
  }
}
```

Restart the backend after any change to this file.

---

### Email — Mailtrap (recommended for testing)

Mailtrap is a free fake inbox: emails are captured and shown in the browser, never delivered.

1. Sign up at [mailtrap.io](https://mailtrap.io) (free tier is sufficient)
2. Go to **Email Testing → Inboxes → your inbox → SMTP/POP3** tab
3. Copy the credentials into `appsettings.Development.json`:

```json
"Email": {
  "SmtpHost": "sandbox.smtp.mailtrap.io",
  "SmtpPort": 587,
  "SmtpUser": "<mailtrap user>",
  "SmtpPassword": "<mailtrap password>",
  "FromAddress": "paytrack@test.local"
}
```

Sent emails appear under **Email Testing → Inboxes → your inbox**.

---

### Email — Gmail (alternative)

Requires a personal Gmail account with 2-Step Verification enabled.
> Not available for Google Workspace accounts managed by an organisation.

1. Go to [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)
2. Click **Create app password**, give it a name (e.g. "PayTrack"), copy the 16-character password
3. Fill in `appsettings.Development.json`:

```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUser": "you@gmail.com",
  "SmtpPassword": "<16-char app password>",
  "FromAddress": "you@gmail.com"
}
```

---

### Slack

#### One-time app setup

1. Go to [api.slack.com/apps](https://api.slack.com/apps) → **Create New App** → **From scratch**
2. Name it (e.g. "PayTrack") and choose your workspace
3. Left sidebar → **OAuth & Permissions** → **Bot Token Scopes** → add:
   - `users:read`
   - `users:read.email` ← required for email-based user lookup
   - `chat:write`
4. Scroll up on the same page → **Install to Workspace** → Allow
5. Copy the **Bot User OAuth Token** (starts with `xoxb-`)

#### Configure the backend

```json
"Slack": {
  "BotToken": "xoxb-..."
}
```

---

## Production

For production deployments, supply credentials as **environment variables** rather than config
files. ASP.NET Core maps environment variables with `__` as a section separator:

| Config key | Environment variable |
|---|---|
| `Email:SmtpHost` | `Email__SmtpHost` |
| `Email:SmtpPort` | `Email__SmtpPort` |
| `Email:SmtpUser` | `Email__SmtpUser` |
| `Email:SmtpPassword` | `Email__SmtpPassword` |
| `Email:FromAddress` | `Email__FromAddress` |
| `Slack:BotToken` | `Slack__BotToken` |

Set these in your hosting environment (Docker Compose `environment:` block, CI/CD secrets,
or the server's system environment).

### Recommended SMTP providers for production

| Provider | Notes |
|---|---|
| **SendGrid** | Generous free tier (100 emails/day), reliable deliverability |
| **Mailgun** | Pay-as-you-go, good EU data-residency options |
| **Gmail / Google Workspace** | Works, but has rate limits and requires App Password or OAuth |
| **Self-hosted (Postfix etc.)** | Full control, requires SPF/DKIM/DMARC configuration for deliverability |

### Slack for production

The same Slack app used for development can serve production — just keep the `xoxb-` token
secure (environment variable, not committed to source control). If the workspace has multiple
environments, create a separate Slack app per environment.

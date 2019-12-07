# Day 6 - Durable Pattern

https://25daysofserverless.com/calendar/6

Chat integration for a chat service that lets you schedule tasks using natural language (e.g. /schedule volunteer at the senior citizens' center tomorrow at 11:00). You should be able to get a confirmation that your event has been scheduled, and then get a notification at the correct time.

## Solution

Azure HTTP triggered durable function with calls to Azure Cognitive Services LUIS and integrated with Microsoft Teams webhooks (in/out)

* /api/SetDurableReminder
  * \[POST\] - Accepts an incoming webhook from Microsoft Teams, validates the HMAC and then triggers a reminder based on the LUIS intent and time entity discovered.
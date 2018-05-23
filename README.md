# A Scanner Sharply

Desktop application which collects and analyzes runtime logs from Android apps.
Written in C# and utilizes the Android Debug Bridge (ADB).

## TODO:
[X] Parse runtime log specifically for all `Harvester` logs:

```
I/com.newrelic.android( 9604): Harvester: connected
I/com.newrelic.android( 9604): Harvester: Sending 0 HTTP transactions.
I/com.newrelic.android( 9604): Harvester: Sending 0 HTTP errors.
I/com.newrelic.android( 9604): Harvester: Sending 0 activity traces.
I/com.newrelic.android( 9604): Harvester: Sending 0 analytics events.
```
[X] Parse the log file to track the session.
[ ] Mark when a new session is started within the log.
[ ] Manually log `sessionId` from app.
[X] Create totals for the 4 data sets sent.
[X] Report the 4 totals to server to then be reported to Insights.

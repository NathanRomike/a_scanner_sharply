# A Scanner Sharply

Desktop application which collects and analyzes runtime logs from Android apps.
Written in C# and utilizes the Android Debug Bridge (ADB).

## TODO:
+ Parse runtime log specifically for all `Harvester` logs:

```
I/com.newrelic.android( 9604): Harvester: connected
I/com.newrelic.android( 9604): Harvester: Sending 0 HTTP transactions.
I/com.newrelic.android( 9604): Harvester: Sending 0 HTTP errors.
I/com.newrelic.android( 9604): Harvester: Sending 0 activity traces.
I/com.newrelic.android( 9604): Harvester: Sending 0 analytics events.
```
+ Parse the log file for the `sessionId`.
+ Mark when a new session is started within the log.
+ Create totals for the 4 data sets sent.
+ Report the 4 totals, along with `sessionId` to Insights via the insert API.

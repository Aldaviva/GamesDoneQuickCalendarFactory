<img src="https://raw.githubusercontent.com/Aldaviva/GamesDoneQuickCalendarFactory/master/GamesDoneQuickCalendarFactory/gdq.ico" height="24" alt="Games Done Quick logo" /> GamesDoneQuickCalendarFactory
===

[![GitHub Actions workflow status](https://img.shields.io/github/actions/workflow/status/Aldaviva/GamesDoneQuickCalendarFactory/compile.yml?branch=master&logo=github&label=build)](https://github.com/Aldaviva/GamesDoneQuickCalendarFactory/actions/workflows/compile.yml) [![Testspace](https://img.shields.io/testspace/tests/Aldaviva/Aldaviva:GamesDoneQuickCalendarFactory/master?passed_label=passing&failed_label=failing&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA4NTkgODYxIj48cGF0aCBkPSJtNTk4IDUxMy05NCA5NCAyOCAyNyA5NC05NC0yOC0yN3pNMzA2IDIyNmwtOTQgOTQgMjggMjggOTQtOTQtMjgtMjh6bS00NiAyODctMjcgMjcgOTQgOTQgMjctMjctOTQtOTR6bTI5My0yODctMjcgMjggOTQgOTQgMjctMjgtOTQtOTR6TTQzMiA4NjFjNDEuMzMgMCA3Ni44My0xNC42NyAxMDYuNS00NFM1ODMgNzUyIDU4MyA3MTBjMC00MS4zMy0xNC44My03Ni44My00NC41LTEwNi41UzQ3My4zMyA1NTkgNDMyIDU1OWMtNDIgMC03Ny42NyAxNC44My0xMDcgNDQuNXMtNDQgNjUuMTctNDQgMTA2LjVjMCA0MiAxNC42NyA3Ny42NyA0NCAxMDdzNjUgNDQgMTA3IDQ0em0wLTU1OWM0MS4zMyAwIDc2LjgzLTE0LjgzIDEwNi41LTQ0LjVTNTgzIDE5Mi4zMyA1ODMgMTUxYzAtNDItMTQuODMtNzcuNjctNDQuNS0xMDdTNDczLjMzIDAgNDMyIDBjLTQyIDAtNzcuNjcgMTQuNjctMTA3IDQ0cy00NCA2NS00NCAxMDdjMCA0MS4zMyAxNC42NyA3Ni44MyA0NCAxMDYuNVMzOTAgMzAyIDQzMiAzMDJ6bTI3NiAyODJjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjY3IDE0LjY3LTEwNiA0NHMtNDQgNjUtNDQgMTA3YzAgNDEuMzMgMTQuNjcgNzYuODMgNDQgMTA2LjVTNjY2LjY3IDU4NCA3MDggNTg0em0tNTU3IDBjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjgzIDE0LjY3LTEwNi41IDQ0UzAgMzkxIDAgNDMzYzAgNDEuMzMgMTQuODMgNzYuODMgNDQuNSAxMDYuNVMxMDkuNjcgNTg0IDE1MSA1ODR6IiBmaWxsPSIjZmZmIi8%2BPC9zdmc%2B)](https://aldaviva.testspace.com/spaces/211585) [![Coveralls](https://img.shields.io/coveralls/github/Aldaviva/GamesDoneQuickCalendarFactory?logo=coveralls)](https://coveralls.io/github/Aldaviva/GamesDoneQuickCalendarFactory?branch=master) [![Uptime for the past 90 days](https://img.shields.io/endpoint?url=https%3A%2F%2Fwest.aldaviva.com%2Ffreshbadge%2F1096308)](https://statuspage.freshping.io/69680-GamesDoneQuickCalendarFactory) [![Current event](https://img.shields.io/endpoint?url=https%3A%2F%2Faldaviva.com%2Fgamesdonequickcalendar%2Fbadge.json)](https://aldaviva.com/docs/gdq.ics)

Converts the official [Games Done Quick schedule](https://gamesdonequick.com/schedule) into an automatically updated [iCalendar ICS file](https://aldaviva.com/docs/gdq.ics) and [public Google calendar](https://calendar.google.com/calendar/u/0?cid=ZThmNDEwMmU3MjFlMmExNDAwZjc4NmExMWU2MjlkZGQ4YTlhOGY5ODRiZjFmMjg4NGI5NmFmYjBhYWM2Zjk0N0Bncm91cC5jYWxlbmRhci5nb29nbGUuY29t), to which your calendar program can subscribe.

Each run in the current GDQ event will appear as a calendar appointment comprising the run's name, estimated start and end time, category, the game's hardware platform and release year, runners, commentators, and host.

Every type of GDQ event with a published schedule is included: the mainline events Awesome Games Done Quick and Summer Games Done Quick; community events [Frost Fatales, Flame Fatales](https://gamesdonequick.com/site/framefatales), and [Back to Black](https://gamesdonequick.com/site/blackinaflash); and small tie-in events like PAX, TwitchCon, and Disaster Relief. The [Hotfix](https://gamesdonequick.com/hotfix) series of shows is excluded because it runs year-round and its schedule is a messy, unparsable spreadsheet.

All future GDQ events will also appear in this exact same ICS file when their schedules are revealed &mdash; you don't have to resubscribe to different ICS files every few months, just subscribe once to this single calendar URL and let it update forever. This is especially helpful if you forget when GDQ starts and it sneaks up on you, causing you to miss the beginning of the event.

The calendar is updated once per minute while a GDQ event is running, and less frequently the rest of the year.

![AGDQ 2023 schedule in Vivaldi](https://pbs.twimg.com/media/Fl2xjasaEAATpQC?format=jpg&name=large)

<!-- MarkdownTOC autolink="true" bracket="round" autoanchor="false" levels="1,2,3,4" bullets="-" -->

- [Subscribing](#subscribing)
    - [Any iCalendar client](#any-icalendar-client)
    - [Vivaldi](#vivaldi)
    - [Google Calendar](#google-calendar)
    - [Microsoft Outlook](#microsoft-outlook)
        - [Outlook for Windows](#outlook-for-windows)
        - [Outlook on the web](#outlook-on-the-web)
        - [Outlook for Android](#outlook-for-android)
        - [Outlook for Mac](#outlook-for-mac)
    - [Mozilla Thunderbird](#mozilla-thunderbird)
    - [Apple Calendar](#apple-calendar)

<!-- /MarkdownTOC -->

## Subscribing

### Any iCalendar client

1. Subscribe to the iCalendar URL [`https://aldaviva.com/docs/gdq.ics`](https://aldaviva.com/docs/gdq.ics). No authentication is required.

### Vivaldi
1. Open [Vivaldi Settings](https://help.vivaldi.com/article/settings/)
1. Go to the 📅 Calendar section
1. In the Calendar Accounts section, click the `+` button
1. Select [**Web Calendar**](https://help.vivaldi.com/calendar/calendar-get-started/add-and-manage-calendar-accounts/#Online_accounts) and click Continue
1. Choose a Name, such as `Games Done Quick` or `GDQ`
1. Set the Address to [`https://aldaviva.com/docs/gdq.ics`](https://aldaviva.com/docs/gdq.ics)
1. Click the **Add Account** button
1. Click the **Save Account** button

### Google Calendar
1. Go to this project's [shared public calendar](https://calendar.google.com/calendar/u/0?cid=ZThmNDEwMmU3MjFlMmExNDAwZjc4NmExMWU2MjlkZGQ4YTlhOGY5ODRiZjFmMjg4NGI5NmFmYjBhYWM2Zjk0N0Bncm91cC5jYWxlbmRhci5nb29nbGUuY29t)
1. Click the **Add** button in the Add Calendar dialog box that appears

> [!NOTE]  
> This public calendar is recommended for Google Calendar users because it enables lower latency updates (about one minute) when a run changes during an event, as opposed to subscribing to the [ICS URL](#any-icalendar-client) which Google polls very infrequently (once every few hours).

> [!TIP]
> To prevent runs from appearing in an alternating two-column checkerboard pattern, you can disable "Display shorter events the same size as 30 minute events" in ⚙ › [Settings](https://calendar.google.com/calendar/r/settings) › General › View options *(web)* or ☰ › ⚙ Settings › General *(Android)*.

### Microsoft Outlook

#### Outlook for Windows
1. Open Outlook
1. Go to the 📅 Calendar section
1. Open the **+ Add** or **+ Open Calendar** menu
1. Select 🌐 **From Internet…**
1. Set the URL to [`https://aldaviva.com/docs/gdq.ics`](https://aldaviva.com/docs/gdq.ics) and click OK
1. Optionally, click Advanced and choose a Folder Name, such as `Games Done Quick` or `GDQ`, then click OK
1. Click the **Yes** button

#### Outlook on the web
1. Sign in to [Outlook on the web](https://outlook.office.com/calendar)
1. Go to the 📅 Calendar tab
1. In the left pane, click [**+ Add calendar**](https://outlook.office.com/calendar/addcalendar)
1. Go to the **⚇ Subscribe from web** navigation section
1. Set the URL to [`https://aldaviva.com/docs/gdq.ics`](https://aldaviva.com/docs/gdq.ics)
1. Choose a calendar name, such as `Games Done Quick` or `GDQ`
1. Click the **Import** button

#### Outlook for Android
1. Sign in to [Outlook on the web](https://outlook.office.com/calendar) with the same Office 365, Exchange, or Outlook.com&#47;Hotmail account with which you are signed into Outlook for Android
1. Follow the [Outlook on the web subscription instructions](#outlook-on-the-web)

#### Outlook for Mac
1. Sign in to [Outlook on the web](https://outlook.office.com/calendar) with the same Office 365, Exchange, or Outlook.com&#47;Hotmail account with which you are signed into Outlook for Mac
1. Follow the [Outlook on the web subscription instructions](#outlook-on-the-web)

### Mozilla Thunderbird
1. Open Thunderbird
1. Go to the 📅 Calendar space
1. Click ☰ (Thunderbird Menu) › + New › Calendar…
1. Choose **On the Network**, then click Next
1. Set the Location to [`https://aldaviva.com/docs/gdq.ics`](https://aldaviva.com/docs/gdq.ics)
1. Check **This location doesn't require credentials**
1. Click **Find Calendars**
1. Click **Subscribe**

### Apple Calendar
1. Open 📅 Calendar
1. Go to File › New Calendar Subscription…
1. Set the Calendar URL to [`https://aldaviva.com/docs/gdq.ics`](https://aldaviva.com/docs/gdq.ics) and click Subscribe
1. Choose a name, such as `Games Done Quick` or `GDQ`
1. To optionally preserve the default reminders, uncheck Remove Alerts
1. To change the update frequency, check Auto-refresh. GDQ schedules shift during events.

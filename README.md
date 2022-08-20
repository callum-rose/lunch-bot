
<img src="/Images/prof_iterole.jpg" title="Prof. Iterole" height="100" width="100">

# Lunch Bot

A program to generate and message groups of people on Microsoft Teams to go for lunch! 

In my current workplace we're all a friendly bunch, but as the company grows it gets harder to keep up with old and new people, esspecially with those not in your team. So we started a new initiative to go for mixed-team lunches once a month. The new problem is how do we create the groups for these lunches quickly and in a way that ensures new people meet each other?

![Logo](/Images/screenshot.png)

Introducing: **LunchBot**! A command line program that automates the whole process. All the user has to do is:

* Run the program
* Run the **create** command
* Run the **deliver** command

## Tech

* Azure
* Microsoft.Graph
* .NET Core

#### Packages Used

* Azure.Identity
* CsvHelper
* McMaster.Extensions.CommandLineUtils
* McMaster.Extensions.Hosting.CommandLine
* Microsoft.Azure.WebJobs
* Microsoft.Azure.WebJobs.Extensions.Http
* Microsoft.Extensions.Configuration
* Microsoft.Extensions.Configuration.FileExtensions
* Microsoft.Extensions.Configuration.Json
* Microsoft.Extensions.DependencyInjection
* Microsoft.Extensions.Hosting
* Microsoft.Graph
* NanoXLSX
* Serilog
* Serilog.Extensions.Logging
* Serilog.Sinks.Console
* Serilog.Sinks.File
* ShellProgressBar
* TextCopy

## Prerequisites

You'll need to create an [Azure App Registration](https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationsListBlade), this'll give you your app id. The app needs these permissions:

```
ChatMessage.Send
Chat.Create
User.Read.All
Chat.ReadWrite
ChatMessage.Read
User.Read
```

You'll also need your tenant id which can be found [here](https://docs.microsoft.com/en-us/azure/active-directory/fundamentals/active-directory-how-to-find-tenant).
## Run Locally

Using the key values in appsettings.json for *TenantIdEnvKey* and *ClientIdEnvKey*, add your app / client id and tenant id to your user environment variables e.g.

| Key | Value     |
| :-------- | :------- |
| `LunchBotTenantId` | `62f37a04-...-00d69cb9aea8` |
| `LunchBotClientId` | `411cf5c2-...-bab72db5fe4e` |

Clone the project. Open "LunchBot\LunchBot.sln" in your IDE, and build it. Run the output .exe file
## Commands Reference

```bash
setup           Create the .appdata config file
create          Run the party generator to create the groups for a party
deliver         Deliver the lunch from a generated party
partydatapaths  Display the paths of all the party data files
remind          Send a reminder to groups that haven't said anything yet
stats           Show stats for the system
```
## How The Algorithm Works

Since this app doesn't need to run very often a brute force approach to find the perfect party would work great, but the number of combinations is defined by:

```
N!/(n! * k * k!) 

where: N is number of people, n is max group size, k is (N/n)
```

This is an O(N!) operation which as you can imagine gets unwieldy almost immediately.

We're trying to optimise for people who haven't met before, and people who are in different departments. We can use gradient descent to find an optimal solution. The app stores matrices of who has already met in previous lunches, and we input peoples' department. 

We start with everyone randomly organised into groups. We can score each group based on:
* The number of people who have met before, and how many time above the average meet count
* The number of people in the same department


We can now search for the best and worst scoring groups in the party. Once we have this can swap a member between the groups to improve the overall score. This is brute forced because it's easier, but is quick to check every possible swap which is just:

```
group_size_0 * group_size_1
```

We then calculate the aggregate score of the party.

We find the new best and worse group, swap, and score party: until the overall party score improvement plateaus.

The whole process is then looped X times to see if we can find a better group. There's a fair amount of brute force involved in all this but for the use case it's not a bad thing being efficient with dev time.
## How To Launch a Lunch

1. Run the **setup** command first. This will create an .appdata file which you'll need to populate with key data that'll allow the app to function properly. Conductor display name is the teams display name of the signed in user. Name mappings helps find users where the input name isn't exactly the same as their teams display name, perhaps because of a nickname or apostrophe. Venues is the list of venues that lunches can be in

    ```json
    {
    "ConductorDisplayName": "Prof. Iterole",
    "NameMappings": {
        "Name1, Surname1": "Name2, Surname2",
        "Kimothy, O'Test": "Kim, O Test"
    },
    "Venues": [
        "Shoreditch",
        "Victoria",
        "Bloomsbury",
        "Canary Wharf",
        "London Bridge"
    ]
    }
    ```

1. Run the **create** command. This will intially ask you for an .xlsx file containing the list of names of everyone you want to include. The format of the file should be:

    | Employee | Department     |
    | :-------- | :------- |
    | `Surname, Name` | `Marketing` |
    | `Test, Kimothy` | `Dev` |
    | `...` | `...` |

    The first time this is run you'll probably find some users that can't be found because their name isn't entirely the same. This is what name mappings is more in *.appdata*. Update this until all users can be found.

    The groups will be created and saved to file.

1. Run the **deliver** command. This will default to doing a dry-run unless the flag is set, run **deliver --help** to see the flag. If doing a "wet-run" the teams chats will now all be created and the message sent to the chat.
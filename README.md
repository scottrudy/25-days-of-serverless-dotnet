# 25-days-of-serverless-dotnet

Dotnet Core with C# implementations of [25 days of serverless](https://25daysofserverless.com/).

## [Day 1 - A Basic Function](./day01/README.md)

REST API endpoint that spins a dreidel and randomly returns נ (Nun), ג (Gimmel), ה (Hay), or ש (Shin).

### Solution

Azure HTTP triggered Function that accepts a request for a spin result.

## [Day 2 - Task Scheduler](./day02/README.md)

Task scheduler that will tell Lucy exactly when she should relight candles, pour coffee into cups, and deliver batches of coffee.

### Solution

Azure Timer triggered Durable Function that runs annually on a specifed date and time and uses Durable Timers in between tasks to control flow.

## [Day 3 - Storage Trigger](./day03/README.md)

Web service that lets users upload photos, but then verifies that each uploaded photo/file is a png. If it is not, delete the photo. (storage trigger)

### Solution

Azure HTTP triggered function with Blob output binding that exposes an endpoint for upload and an Azure Blob Storage triggered function that deletes the blob if it doesn't have a PNG header.

## [Day 4 - API Endpoint](./day04/README.md)

An HTTP API that lets Ezra's friends add food dishes they want to bring to the potluck, change or remove them if they need to (plans change!), and see a list of what everybody's committed to bring.

### Solution

Azure HTTP triggered functions with a table storage back end.

## [Day 5 - Smart Apps](./day05/README.md)

A serverless application that helps Santa figure out if a given child is being naughty or nice based on what they've said. You'll likely need to detect the language of the correspondence, translate it, and then perform sentiment analysis to determine whether it's naughty or nice.

### Solution

Azure HTTP triggered function with calls to Azure Cognitive Services Translator Text API for language detection and language translation, as well as the Text Analytics API for sentiment analysis.

## [Day 6 - Durable Pattern](./day06/README.md)

Chat integration for a chat service that lets you schedule tasks using natural language (e.g. /schedule volunteer at the senior citizens' center tomorrow at 11:00). You should be able to get a confirmation that your event has been scheduled, and then get a notification at the correct time.

### Solution

Azure HTTP triggered function with calls to Azure Cognitive Services LUIS.

## [Day 7 - Picture Challenge](./day07/README.md)

A web API that lets you search for images of things you want to get rid of. The application (e.g. a cloud function with a single endpoint) takes text as an input and returns an image found on unsplash or another image platform.

### Solution

Azure HTTP triggered function with search API call to [Unsplash](https://api.unsplash.com) that redirects to the first image hit.

## [Day 8 - Incident Status](./day08/README.md)

A simple solution that helps inform elves and helpers all over the world when there is a problem with Santa's Reindeer Guidance System.

### Solution

N/A

## [Day 9 - Thank you to Github issue creators](./day09/README.md)

Say thank you to everyone who takes the time to create an issue in your Github repository, using serverless technology.

### Solution

N/A

## [Day 10 - Timer Trigger](./day10/README.md)

A tool that finds deals of the day from Twitter and adds them to a static web page.

### Solution

Azure time triggered function with API call to [Twitter API](https://api.twitter.com) using [Tweetinvi](https://github.com/linvi/tweetinvi). Also used input/output bindings only to find that table storage doesn't support LINQ in .net core and the blob binding doesn't support container names that begin with a `$`, like `$web`.

## [Day 11 - Database Trigger](./day11/README.md)

a system that takes in childrens' requests and stores them for later processing. Additionally, a notification is sent every time a new gift wish arrives.

Bonus: A webpage with a form that submits data in the previous format!

### Solution

Azure Functions using .Net Core 3.1 with C#. HTTP triggered functions leveraging output bindings using Azure Queue Storage. A queue storage triggered function that saves the message to Azure Table Storage and sends a message to Teams when a new row is added.

## [Day 12 - Caching](./day12/README.md)

A service that gets the readme from a GitHub repo, parses the Markdown to HTML, and returns the HTML to the client.

Bonus: Reading and parsing Markdown is a lot of work and may eat up Gigabyte Seconds! To optimize, HTML converted Markdown responses are cached and returned when requested.

### Solution

Azure Functions 3.0 using .Net Core 3.1 with C#. HTTP triggered function leveraging input binding for Azure Table Storage for caching. Leverages the [Github Content API](https://developer.github.com/v3/repos/contents) to get `README` files for a repo and the [MarkdownDeep library from Topten Software](https://www.toptensoftware.com/markdowndeep/). The Github ETag is used to check for cache invalidation.
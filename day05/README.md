# Day 5 - Smart Apps

https://25daysofserverless.com/calendar/5

A serverless application that helps Santa figure out if a given child is being naughty or nice based on what they've said. You'll likely need to detect the language of the correspondence, translate it, and then perform sentiment analysis to determine whether it's naughty or nice.

## Solution

Azure HTTP triggered function with calls to Azure Cognitive Services Translator Text API for language detection and language translation, as well as the Text Analytics API for sentiment analysis. Azure offers a Free (F0) tier to try this out if you have an Azure account, or you can sign up for a 7 day trial. To run the function locally rename the `sample.settings.json` file to `local.settings.json` and fill in the appropriate values.

This solution only accepts a single sentence, although the Cognitive Services APIs allow for full documents to be broken up into sentences.

* /api/naughtyornice
  * \[POST\] - Returns a message indicating if the child is on the "naughty" list or "nice" list. A 400 Bad Request will be generated if the language cannot be detected.

## Test data

The `test.json` contains some language examples.
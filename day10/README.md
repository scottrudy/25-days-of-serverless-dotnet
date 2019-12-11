# Day 10 - Timer Trigger

A tool that finds deals of the day from Twitter and adds them to a static web page.

## Solution

Azure time triggered function with API call to [Twitter API](https://api.twitter.com) using [Tweetinvi](https://github.com/linvi/tweetinvi). Also used input/output bindings only to find that table storage doesn't support LINQ in .net core and the blob binding doesn't support container names that begin with a `$`, like `$web`.
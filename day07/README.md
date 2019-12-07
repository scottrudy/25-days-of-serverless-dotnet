# Day 7 - Picture Challenge

https://25daysofserverless.com/calendar/7

A web API that lets you search for images of things you want to get rid of. The application (e.g. a cloud function with a single endpoint) takes text as an input and returns an image found on unsplash or another image platform.

## Solution

Azure HTTP triggered function with search API call to [Unsplash](https://api.unsplash.com) that redirects to the first image hit.

* /api/GetPictures?text={value}
  * \[GET\] - Accepts a query string text value to search on.
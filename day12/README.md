# Day 12 - Caching

A service that gets the readme from a GitHub repo, parses the Markdown to HTML, and returns the HTML to the client.

Bonus: Reading and parsing Markdown is a lot of work and may eat up Gigabyte Seconds! To optimize, HTML converted Markdown responses are cached and returned when requested.

## Solution

Azure Functions 3.0 using .Net Core 3.1 with C#. HTTP triggered function leveraging input binding for Azure Table Storage for caching. Leverages the [Github Content API](https://developer.github.com/v3/repos/contents) to get `README` files for a repo and the [MarkdownDeep library from Topten Software](https://www.toptensoftware.com/markdowndeep/). The Github ETag is used to check for cache invalidation.

\[GET\] /api/repos/{user}/{repo}

### Cheat Mode

Including the `X-Cheat-Mode` header proxies HTML content directly from Github and bypasses the conversion and cache using a [Github Media Type](https://developer.github.com/v3/repos/contents/#custom-media-types) header `application/vnd.github.VERSION.html`.

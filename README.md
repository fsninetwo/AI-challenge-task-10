# AI Console Product Finder

A console application that leverages OpenAI Function-Calling to translate natural-language product requests into structured filters, search a local JSON catalog (`products.json`), and return neatly formatted results. Each session is logged to a markdown report file.

---

## Prerequisites

1. **.NET 8 SDK** – Install from https://dotnet.microsoft.com/download
2. **OpenAI API key** – Create one at https://platform.openai.com and keep it safe.

## Getting Started

```bash
# clone or copy the repo
cd AI-challenge-task-10

# restore & build
 dotnet build 
```

### Configure Your Key

Set the key as an environment variable _or_ in `appsettings.json`.

```bash
# PowerShell
$Env:OPENAI_API_KEY="sk-..."
```

`appsettings.json` (alternative):
```json
"OpenAI": {
  "ApiKey": "sk-..."
}
```

## Running

```bash
 dotnet run 
```

When prompted, type a request such as:

```
Show me the most expensive thing for kitchen that's in stock
```

or

```
Find the cheapest electronics under $50
```

Exit any time with `exit`.

## Output & Reports

* Matching products are printed in a numbered, aligned list.
* Every request & result pair is appended to the markdown report defined in `Logging:SampleOutputPath` (`sample_outputs.md` by default). A timestamp is included for each entry.

## Typical Session

```
=== Welcome to the AI Product Finder ===
> Show me fitness equipment under $50 that's in stock
Searching, please wait...
Found 2 products:

Filtered Products:
 1. Yoga Mat            - $   19.99, Rating:  4.3, In Stock 
 2. Jump Rope           - $    9.99, Rating:  4.0, In Stock 

> exit
Thank you for using the AI Product Finder. Goodbye!
```

## Features

* Natural-language product search powered by OpenAI function-calling (`filter_products`).
* Supports common filters: **category**, **maximum price**, **minimum rating**, **in-stock constraint**, and **keywords**.
* NEW ➜ **Superlatives**: understand phrases such as "most expensive", "cheapest", "least expensive", or "lowest price" and automatically return the single top matching product.
* Automatic markdown logging of every request & result to `sample_outputs.md` with timestamps.

## Notes

* `products.json` is copied to the build output automatically; modify it to extend the catalog.
* The app uses OpenAI function-calling (`filter_products`) for NL-to-JSON conversion and merges in local heuristics for superlatives (most/least expensive).
* Ensure your network allows outbound HTTPS to `api.openai.com`. 
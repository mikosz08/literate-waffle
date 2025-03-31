# StackOverflow Tags REST API

Literate Waffle is a .NET application that integrates with the Stack Overflow API to fetch and process tags. It supports pagination, filtering, and saving data to a local JSON file. This project is designed to demonstrate how to interact with external APIs and process large datasets efficiently.

---

## Features

- Fetch tags from the Stack Overflow API with real-time data.
- Support for pagination with customizable page size and sorting options (e.g., by name or count).
- Calculate tag popularity as a percentage of the total occurrences in the dataset.
- Save fetched data to a local JSON file (`tags.json`) for caching and offline access.
- Docker support for easy deployment and scalability.
- Built-in Swagger UI for testing and exploring API endpoints.

---

## Requirements

- [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0) or newer
- [Docker](https://www.docker.com/) (optional)
- A Stack Overflow API key OR you can use this one: ```bSk)uQvp0j0Bw*p*DlEesw((``` (This is not considered a secret, and may be safely embed in client side code or distributed binaries. )

---

## Getting Started


* ```git clone https://github.com/mikosz08/literate-waffle.git```
* ```cd literate-waffle/StackAPI```
* ```Set your StackOverflow API key within appsettings.Development.json```
* ```docker-compose build```
* ```docker-compose up```
* ```http://localhost:8000/swagger```






---
![image](https://github.com/user-attachments/assets/76b629bd-f0f8-411c-9ba4-0f6706bfbf11)
![image](https://github.com/user-attachments/assets/1137c2e8-86d2-484c-a061-698b9b7718d7)


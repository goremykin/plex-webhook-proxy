## About
Plex Webhook Proxy is a proxy service that allows you to filter webhooks from Plex. You can define rules to forward specific events from specific users.

## Simkl use case
If you have several "managed" users in Plex and want to send only your personal viewing events to Simkl (or each user has their own Simkl account), Plex doesn’t support webhooks per managed user. Additionally, Plex sends too many events and unnecessary data (like username, IP address), while Simkl only requires the `media.scrobble` event with minimal data.

The Plex Webhook Proxy filters events and sends only the required data to Simkl.

## Installation
To get started with Plex Webhook Proxy, you can use Docker Compose.
```yaml
services:
    plex-webhook-proxy:
        image: ghcr.io/goremykin/plex-webhook-proxy:latest
        environment:
        PROXY_RULES: |
            [
                {
                    "UserName": "John",
                    "Events": ["media.scrobble"], 
                    "Sanitizer": "simkl",
                    "WebHookUrl": "destination_address"
                }
            ]
        ports:
        - "5050:5050"
```

After setting up the proxy with Docker Compose, you will need to add the webhook to your Plex server and point it to the proxy.

If Plex is running on the same machine outside of Docker or is using Docker with the `host` network mode, you should use http://localhost:5050 as the webhook address.

### Configuration notes
| Field Name   | Required / Optional | Description                                                                                                                                                                                             |
|--------------|---------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `UserId`     | Optional            | The Plex user ID to filter events for. If not specified, no user filtering will be applied unless `UserName` is provided.                                                                               |
| `UserName`   | Optional            | The Plex username to filter events for. If not specified, no user filtering will be applied unless `UserId` is provided. If neither `UserId` nor `UserName` is specified, all events will be processed. |
| `Events`     | Optional            | A list of events to filter (e.g., `media.scrobble`). If not specified, all events will be routed. [List of Plex events](https://support.plex.tv/articles/115002267687-webhooks/#toc-1).                 |
| `Sanitizer`  | Optional            | Only one sanitizer, "simkl", is currently implemented. This field is used to clean or transform the webhook data before forwarding it. If not specified, data will be forwarded as-is.                  |

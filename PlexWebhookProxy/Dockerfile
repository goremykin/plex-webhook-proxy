FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /source

RUN apk update && apk add --no-cache clang build-base zlib-dev

COPY . .
RUN dotnet publish -c Release -o /app

FROM alpine:latest AS final
EXPOSE 5050

WORKDIR /app
COPY --from=build /app/ .

ENTRYPOINT ["./PlexWebhookProxy"]

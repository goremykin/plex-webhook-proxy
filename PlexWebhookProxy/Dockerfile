FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

RUN apk update && apk add --no-cache clang build-base zlib-dev

COPY . .
RUN dotnet publish PlexWebhookProxy/PlexWebhookProxy.csproj -c Release -o /app
RUN rm -f /app/*.dbg # dotnet always generates debug symbols for native aot apps. DebugType / DebugSymbols don't work

FROM alpine:latest AS final
EXPOSE 5050

WORKDIR /app
COPY --from=build /app/ .

ENTRYPOINT ["./PlexWebhookProxy"]

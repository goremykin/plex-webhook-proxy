FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

RUN apk update && apk add --no-cache clang build-base zlib-dev

COPY . .
RUN dotnet publish PlexWebhookProxy/PlexWebhookProxy.csproj \
    -c Release \
    -o /app \
    -r linux-musl-$(uname -m | sed 's/x86_64/x64/' | sed 's/aarch64/arm64/') \
    # QEMU emulation has issues with multi-threaded AOT compilation on ARM64
    /p:Parallelism=1

# Strip debug symbols (dotnet always generates debug symbols for native aot apps)
RUN rm -f /app/*.dbg

FROM alpine:latest AS final
EXPOSE 5050
WORKDIR /app
COPY --from=build /app/ .
ENTRYPOINT ["./PlexWebhookProxy"]


FROM mcr.microsoft.com/dotnet/core/runtime:2.2-alpine
RUN apk update && \
    apk add librdkafka librdkafka-dev
COPY bin/release/netcoreapp2.1/publish /app
ENTRYPOINT dotnet /app/Extensions.Generic.Kafka.Hosting.Sample.dll
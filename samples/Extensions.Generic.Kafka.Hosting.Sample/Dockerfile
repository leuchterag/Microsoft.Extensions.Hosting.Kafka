FROM mcr.microsoft.com/dotnet/core/runtime:2.1-alpine
RUN apk update && \
    apk add librdkafka librdkafka-dev
COPY bin/Release/net6.0/publish /app
ENTRYPOINT dotnet /app/Extensions.Generic.Kafka.Hosting.Sample.dll
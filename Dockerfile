FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build
ARG tcbuildtype
ARG tcbuildnumber
LABEL stage=builder
LABEL tcbuildtype=$tcbuildtype
LABEL tcbuildnumber=$tcbuildnumber
WORKDIR /opt
COPY PrometheusMqttBridge .
RUN ["dotnet", "publish", "PrometheusMqttBridge.csproj"]

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine
ARG tcbuildnumber
ARG tcbuildrev
RUN mkdir -p /opt/prometheus-mqtt-exporter
WORKDIR /opt/prometheus-mqtt-exporter
EXPOSE 9100
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
LABEL org.opencontainers.image.authors="Simon Walker <github@stwalkerster.co.uk>" org.opencontainers.image.licenses="MIT" org.opencontainers.image.source="https://github.com/stwalkerster/prometheus-mqtt-exporter"
LABEL org.opencontainers.image.revision=$tcbuildrev org.opencontainers.image.version=$tcbuildnumber
COPY --from=build /opt/bin/Debug/net6.0/publish/ ./
RUN mkdir /config && \
    cp config.yml /config/config.yml
ENTRYPOINT ["dotnet", "PrometheusMqttBridge.dll"]
CMD ["/config/config.yml"]


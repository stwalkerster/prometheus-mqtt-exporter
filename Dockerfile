ARG tcbuildtype
ARG tcbuildnumber
ARG tcbuildrev
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine as build
LABEL stage=builder
LABEL tcbuildtype=$tcbuildtype
LABEL tcbuildnumber=$tcbuildnumber
WORKDIR /opt
COPY PrometheusMqttBridge .
RUN ["dotnet", "publish", "PrometheusMqttBridge.csproj"]

FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine
WORKDIR /opt
EXPOSE 9100
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
LABEL org.opencontainers.image.authors="Simon Walker <github@stwalkerster.co.uk>" org.opencontainers.image.licenses="MIT" org.opencontainers.image.source="https://github.com/stwalkerster/prometheus-mqtt-exporter"
LABEL org.opencontainers.image.revision=$tcbuildrev org.opencontainers.image.version=$tcbuildnumber
COPY --from=build /opt/bin/Debug/net5.0/publish/ ./
RUN mkdir /config && \
    cp config.yml /config/config.yml
VOLUME /config
ENTRYPOINT ["dotnet", "PrometheusMqttBridge.dll"]
CMD ["/config/config.yml"]


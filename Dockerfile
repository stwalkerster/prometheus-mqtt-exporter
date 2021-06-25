FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine as build
ARG tcbuildtype
ARG tcbuildnumber
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
COPY --from=build /opt/bin/Debug/net5.0/publish/ ./
RUN mkdir /config && \
    cp config.yml /config/config.yml
VOLUME /config
ENTRYPOINT ["dotnet", "PrometheusMqttBridge.dll"]
CMD ["/config/config.yml"]

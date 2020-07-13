FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as build
WORKDIR /opt
COPY PrometheusMqttBridge .
RUN ["dotnet", "publish", "PrometheusMqttBridge.csproj"]

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-alpine
WORKDIR /opt
EXPOSE 9100
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
COPY --from=build /opt/bin/Debug/netcoreapp3.1/publish/ ./
RUN mkdir /config && \
    cp config.yml /config/config.yml
VOLUME /config
ENTRYPOINT ["dotnet", "PrometheusMqttBridge.dll"]
CMD ["/config/config.yml"]

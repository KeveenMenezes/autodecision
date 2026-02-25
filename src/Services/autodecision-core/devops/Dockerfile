#######################
## Stage 1 - Builder ##
#######################
FROM us.gcr.io/try-apis-152423/dotnet/sdk-with-sonarscanner:8.0 AS build
WORKDIR /app

## Arguments for setting the Azure Artifacts Feed and Sonarqube Project Report
ARG FEED_URL
ARG FEED_USER
ARG FEED_ACCESSTOKEN
ARG BRANCH_NAME
ARG IS_MAIN_BRANCH
ARG SONAR_HOST_URL
ARG SONAR_PROJECT_KEY
ARG SONAR_TOKEN
ARG FT_ENABLE_SONAR_REPORT

## Copy the applications and everything else
COPY ./autodecision_core autodecision_core

## Start the Sonar Scanner if the feature toggle is enabled 
RUN ([ $FT_ENABLE_SONAR_REPORT = true ] && [ ! -z $SONAR_PROJECT_KEY ] && [ $IS_MAIN_BRANCH = 'True' ])  \
	&& dotnet sonarscanner begin /k:"${SONAR_PROJECT_KEY}" /d:sonar.host.url="${SONAR_HOST_URL}" /d:sonar.login="${SONAR_TOKEN}" /d:sonar.branch.name="${BRANCH_NAME}"  \
	|| echo $SONAR_SCANNER_ERR_MSG

## Setting the NuGet source
RUN dotnet nuget add source ${FEED_URL} --name bmgmoney --username ${FEED_USER} --password ${FEED_ACCESSTOKEN} --store-password-in-clear-text

## Restore packages
RUN dotnet restore "autodecision_core/Api/AutodecisionCore.csproj"

## Build the app
RUN dotnet build "autodecision_core/Api/AutodecisionCore.csproj" -c Release -o build    

## Publish the app
RUN dotnet publish "autodecision_core/Api/AutodecisionCore.csproj" -c Release -o publish

## Stop the Sonar Scanner and generate a report if the feature toggle is enabled
RUN ([ $FT_ENABLE_SONAR_REPORT = true ] && [ ! -z $SONAR_PROJECT_KEY ] && [ $IS_MAIN_BRANCH = 'True' ])  \
	&& dotnet sonarscanner end /d:sonar.login="${SONAR_TOKEN}"  \
	|| echo $SONAR_SCANNER_ERR_MSG

#######################
## Stage 2 - Runner  ##
#######################
FROM us.gcr.io/try-apis-152423/dotnet/aspnet-with-timezone:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AutodecisionCore.dll"]
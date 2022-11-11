# NLog.Targets.Firestore
This project provides a custom target for the [NLog](https://nlog-project.org/) framework to allow a user to send log messages to own Firestore.

## Nuget
This package also available on Nuget and it can install from Nuget:

    Install-Package NLog.Targets.Firestore

## Parameters
- ProjectId(Required): A user-assigned unique identifier for the FirebaseProject.
- PrivateKeyId(Required): The [keyString](https://cloud.google.com/api-keys/docs/reference/rest/v2/projects.locations.keys#Key.FIELDS.key_string) of the API key associated with the WebApp.
- PrivateKey(Required): To authenticate a service account and authorize it to access Firebase services, you must generate a private key file in JSON format.
- ClientEmail(Required): Client E-Mail address
- AuthUri(Optional): Authentication url
- TokenUri(Optional): Token URL
- AuthProviderX509CertUrl(Optional): Provider's X509 Certification URL
- ClientX509CertUrl(Optional): Client's X509 Certification URL
- Collection(Required): Firestore Collection Name
- ExcludeLoggers(Optional): Exclude loggers

## Config
```
<?xml version="1.0" encoding="utf-8" ?>
<!-- XSD manual extracted from package NLog.Schema: https://www.nuget.org/packages/NLog.Schema-->
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xsi:schemaLocation="NLog NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogFile="c:\temp\console-example-internal.log"
      internalLogLevel="Info" >

	<extensions>
		<add assembly="NLog.Targets.Firestore" />
	</extensions>

	<!-- the targets to write to -->
	<targets>
		<!-- write logs to file -->
		<target xsi:type="Console" name="logconsole"
				layout="${longdate}|${level}|${message} |${all-event-properties} ${exception:format=tostring}" />

		<target xsi:name="firestore"
				type= "Firestore"
				FirestoreType= "<Fire Store Type> Ex: service_account"
				ProjectId= "<Project Id>"
				PrivateKeyId="<Private Key Id>"
				PrivateKey="<Private Key>"
				ClientEmail="<Client Email>"
				ClientId= "<Client Id>"
				Collection="<Collection Name>"
				Document="<Document Name>"
				ExcludeLoggers="<ExcludeLoggers> Ex:Program,Microsoft"
				/>
	</targets>

	<!-- rules to map from logger name to target -->
	<rules>
		<logger name="*" minlevel="Trace" writeTo="logconsole,firestore" />
		
		<!--Output hosting lifetime messages to console target for faster startup detection -->
		<logger name="Microsoft.Hosting.Lifetime" minlevel="Info" writeTo="lifetimeConsole, ownFile-web" final="true" />

		<!--Skip non-critical Microsoft logs and so log only own logs (BlackHole) -->
		<logger name="Microsoft.*" maxlevel="Info" final="true" />
		<logger name="System.Net.Http.*" maxlevel="Info" final="true" />

		<logger name="*" minlevel="Trace" writeTo="ownFile-web" />
	</rules>
</nlog>
```

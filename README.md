# Real Time Taxi Flight Visualization w/ Azure Stream Analytics, Azure Functions, and SignalR

## What we will cover

In this tutorial, we will be visualizing data on a serverless web app utilizing several Azure technologies.

- We will be using Azure Maps to draw the map canvas in our web app.
- To get the flight information, we will create a VM from the provisioned image. The flight data will then be persisted in an Event Hub.
- The Stream Analytics job will then process and query the data recieved from the Event Hub.
- We will then create an Azure Function that listens to the Stream Analytics job and updates an Azure SignalR hub with all changes to the taxi data. 
- Finally the web app will be configured with a SignalR client to handle the data changes in real-time.

## What you will need

- Microsoft Azure Account
- Visual Studio Code
- Azure Function CLI Tools *(Will be downloaded automatically when debugging functions locally)*

# Creating the Azure Resources

## Azure Maps

[**Azure Maps from the docs**](https://docs.microsoft.com/en-us/azure/azure-maps/about-azure-maps)

### Create a new Azure Maps resource

1. In the upper left corner of the portal, click on **Create a resource***
2. Type in **Maps** in the search bar and select **Maps** in the dropdown.
3. Click the **Create** button that appears on the Maps resource page
4. Enter the following information into the **Create Maps Account** template

    | Name              | Value |
    | ---               | ---   |
    | Subscription      | Select your subscription
    | Resource Group    | Select the resource group created above
    | Name              | Give your maps account a meaningful name
    | Pricing Tier      | Select **Standard S0** [See Pricing Info](https://azure.microsoft.com/en-us/pricing/details/azure-maps/)

5. Read the **License and Privacy Statement** and select the checkbox.
6. Once the new Azure Maps resource has been provision, navigate to the newly deployed resource and locate the **Authentication** tab under the **Settings** subheading. You will need to grab the key later on.

---

## Azure SignalR

[**Azure SignalR from the docs**](https://docs.microsoft.com/en-us/azure/azure-signalr/signalr-overview)

### Create a new Azure SignalR resource

1. In the upper left corner of the portal, click on **Create a resource***
2. Type in **SignalR** in the search bar and select **SignalR Service** in the dropdown.
3. Click the **Create** button that appears on the SignalR Service resource page
4. Enter the following information into the **Create SignalR Service** template

    | Name              | Value |
    | ---               | ---   |
    | Resource Name     | Give your SignalR Service a meaningful name
    | Subscription      | Select your subscription
    | Resource Group    | Select the resource group created above
    | Location          | Select a location to deploy your SignalR Service too
    | Pricing Tier      | Select the **Free** tier [See Pricing Info](https://azure.microsoft.com/en-us/pricing/details/signalr-service/)

5. Once the new SignalR Service has been provision, navigate to the newly deployed resource and locate the **Keys** tab under the **Settings** subheading. You will need to grab the connection string later on.

---

## Azure Stream Analytics

[**Azure Stream Analytics from the docs**](https://docs.microsoft.com/en-us/azure/stream-analytics/stream-analytics-introduction)

### Create a new Stream Analytics Job

1. In the upper left corner of the portal, click on **Create a resource***
2. Type in **Stream** in the search bar and select **Stream Analytics Job** in the dropdown.
3. Click the **Create** button that appears on the Stream Analytics job page
4. Enter the following information into the **Create Stream Analytics Job** template

    | Name              | Value |
    | ---               | ---   |
    | Job Name          | Give your Stream Analytics Job a meaningful name
    | Subscription      | Select your subscription
    | Resource Group    | Select the resource group created above
    | Location          | Select a location to deploy your Stream Analytics job too
    | Hosting           | Select **Cloud** hosting option 
   
5. Click **Create** button that appears on the Stream Analytics job page

---

## Azure Function App

[**Azure Function App from the docs**](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview)

### Create a new Function App

1. In the upper left corner of the portal, click on **Create a resource***
2. Type in **Function** in the search bar and select **Function App** in the dropdown.
3. Click the **Create** button that appears on the Function App page
4. Enter the following information into the **Create Function App** template

    | Name              | Value |
    | ---               | ---   |
    | Subscription      | Select your subscription
    | Resource Group    | Select the resource group created above
    | Function App name | Give your function app a meaningful name
    | Publish           | Code
    | Runtime stack     | Select **.NET Core** from the dropdown menu
    | Version           | 3.1
    | Region            | Select a region to deploy your function app too
   
5. Click **Review + Create** and then once the validation has passed, select **Create**
6. Once the new Function App has been provision, navigate to the newly deployed resource and locate the **Keys** tab under the **Settings** subheading. You will need to grab the connection string later on.

---

# Part 2 - Building Azure Functions to enable real time flight data

In Part 1 of the workshop we will now focus on building out the Azure Functions which will enable real time data updates for the flight data on our map.
The following image describes the flow we are looking to create to enable real time functionality.

1. A record or change is updated in the Event Hub. 
2. The updated record is propogated to the Stream Analytics job
3. The record is processed and outputted from the Stream Analytics job to the Azure Function
3. An Azure Functions is triggered by the change event using an HTTP trigger
4. The SignalR Service output binding publishes a message to SignalR Service
5. SignalR Service publishes the message to all connected clients

[Take a look at the docs if you want to explore this pattern a little further](https://docs.microsoft.com/en-us/azure/azure-signalr/signalr-concept-azure-functions)

## Create the Virtual Machine and the Event Hub

In order to use the Cosmos DB change feed to track changes to the flight data, we are going to need to create a database and collection to store the information in the first place.  

1. In the portal navigate to your Cosmos DB instance we created earlier and open up the **Data Explorer**.
2. In the top left had corner, select **New Container** which will open up the **Add Container** dialog. Fill in the fields as per the table below.

    | Name          | Value |
    | ---           | ---   |
    | Database id   | Select **Create new** and give your database a name. Tick the option to **Provision database throughput**
    | Throughput    | Leave as the minimum of 400 RUs
    | Container id  | Give your collection a meaningful name
    | Partition Key | Set to /originCountry as this will be derived from our data set.

    ![CC](Artifacts/CosmosContainer.png)

3. Select **OK** and after a couple of seconds, you should have your new database and collection provisioned.

4. (Optional) - Navigate to the **Settings** section of your new collection.
    - Turn on **Time to live** and set the number of seconds a record should remain before being removed.
    - I've enabled this and set it to 10 min as a quick 'hack' to clear out any stale flight data as that database will constantly be updated with new flight information.

## HTTP Trigger Function

1. Open up **Visual Studio Code** and create a new **New File**.

2. Make sure you have Azure Functions v2 selected and search for the Timer trigger function template.

3. Copy and paste the code below into your new file. There are two functions, one called negoitate which establishes the connection with the SignalR hub and another called message which relays the records from the Stream Analytics job to the SignalR hub.

4. Make sure the code is editted to include your specific SignalR hub name. 

```CSharp
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace HTTPTrigger
{   
    public static class NegotiateFunction
    {
        [FunctionName("negotiate")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
                                        [SignalRConnectionInfo(HubName = "<Insert Hub Name here>")] SignalRConnectionInfo info,
                                        ILogger log)
        {
            return info != null
                ? (ActionResult)new OkObjectResult(info)
                : new NotFoundObjectResult("Failed to load SignalR Info.");
        }
    }

    public static class MessageFunction
    {
        [FunctionName("message")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function)] HttpRequest req,
                                                    [SignalR(HubName = "<Insert Hub Name here>")] IAsyncCollector<SignalRMessage> signalRMessages,
                                                    ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Request {requestBody}");
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Please pass a payload to broadcast in the request body.");
            }

            await signalRMessages.AddAsync(new SignalRMessage()
            {
                Target = "notify",
                Arguments = new object[] { requestBody }
            });

            return new OkResult();
        }
    }

}
```

To use the SignalR binding extensions you need to add the [Microsoft.Azure.WebJobs.Extensions.SignalRService](https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.SignalRService) package from nuget as a dependency to your project.

5. Add an app setting to the **local.settings.json** file called `AzureSignalRConnectionString` and set the value to the connection string for SignalR service.

The next thing to do is to create a mechanism to push the taxi data to our SignalR Service that we provisioned earlier. Lets cover that next.

## CosmosDB Change Feed & SignalR Outbound Trigger

To listen for updates to our flight data data-set in CosmosDB we are going to leverage the [CosmosDB Change Feed](https://docs.microsoft.com/en-us/azure/cosmos-db/change-feed). The change feed works by listening for changes to your collection and outputs those changes in the same order they were add or modified. The output from the change feed can then be broadcast to any number of subscribers. We will use the [CosmosDB Trigger binding](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2#trigger) to process the change feed updates in an Azure Function.

In the same Azure Function, we will then use the [SignalR output binding](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-signalr-service#using-signalr-service-with-azure-functions) to push the updated flight data to SignalR. 


1. In **Visual Studio 2017** right click on your Function App project and add a new Azure Function and give your function a meaningful name.

2. Pick the **Cosmos DB Trigger** template from the menu and click OK.
    
    | Name                      | Value |
    | ---                       | ---   |
    | Connection String Setting | Name of you CosmosDB connection string config property (AzureCosmosDBConnection)
    | Database name             | Your CosmosDB flight data database name
    | Collection name           | Your CosmosDB flight data collection name

    ![FCF](Artifacts/FuncChangeFeed.png)

3. You should now have a function that looks lik below. Notice the template has set the database and collection property names.

4. Add one more property to the Cosmos DB Trigger binding called `CreateLeaseCollectionIfNotExists` and set this to `true` as below. This will create a new lease collection in your database, if one doesn't exist, which is where the change feed will keep a track of which records it has already processed.

```csharp
public static class FlightDataChangeFeed
{
    [FunctionName("FlightDataChangeFeed")]
    public static void Run([CosmosDBTrigger(
        databaseName: "flightsdb",
        collectionName: "flights",
        ConnectionStringSetting = "AzureCosmosDBConnection",
        LeaseCollectionName = "leases",
        CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input, ILogger log)
    {
        if (input != null && input.Count > 0)
        {
            log.LogInformation("Documents modified " + input.Count);
            log.LogInformation("First document Id " + input[0].Id);
        }
    }
}
```

If you run your function app now, you should get both your timer triggered function and your change feed listener functions spinning up at the same time. If everything is hooked up correctly you should see some log output to the console like below showing how many flights were added into the database and subsequently processed by the change feed.
    
![FCFL](Artifacts/FuncChangeFeedLog.png)

5. The next step to add the SignalR output binding. To use this binding you will need add the **Microsoft.Azure.WebJobs.Extensions.SignalRService** package dependency from nuget to your project.

6. With the package installed add the binding to your function as per below, setting the **HubName** attribute to `flightdata`

    - The **Target** property is the name of the function to be invoked on the client and the **Arguments** property is the array of objects to be passed to the client.

Your function should now be complete and resemble the logic below.

```csharp
public static class FlightDataChangeFeed
{
    [FunctionName("FlightDataChangeFeed")]
    public static async Task RunAsync(
        [CosmosDBTrigger(
            databaseName: "flightsdb",
            collectionName: "flights",
            ConnectionStringSetting = "AzureCosmosDBConnection",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
        [SignalR(HubName = "flightdata")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
        if (input != null && input.Count > 0)
        {
            log.LogInformation("Documents modified " + input.Count);
            foreach (var flight in input)
            {
                await signalRMessages.AddAsync(new SignalRMessage
                {
                    Target = "newFlightData",
                    Arguments = new[] { flight }
                });
            }
        }
    }
}
```
7. The final thing to do before we run this function is to add the connection string for the SignalR Service to the functions config.

    - Create a new setting property called `"AzureSignalRConnectionString"` and set the value to the connection string for your SignalR instance in Azure.

Once thats done, your functions are all set to go. Let's spin these functions up once again and test out the changes. With the functions running, head on over back the SignalR service in Azure and have a look at the metrics tab. After a couple of minutes you should see some telemetry start to feed through. It can take up to 10-15 minutes before you see data coming through on the metrics blade.

## SignalR Connection Info 

We have one more function to create before we can update the front end to connect to SignalR and that is the [Connection info input binding](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-signalr-service#signalr-connection-info-input-binding) which provides the client a valid token and the service endpoint for communicating with the SignalR service instance. 

1. Once again in **Visual Studio 2017** project, right click and add a new **Azure Function** and call it **SignalRInfo**

2. Pick the HttpTrigger template which should scaffold out a vanilla Http Triggered function

    ![FSR](Artifacts/FuncSignalRInfo.png)

3. Replace the contents of the function with the one below and update the **HubName** property to match the HubName you set in SignalR output binding in the previous function. By convention, the name of the function should be **negotiate** but you can set it to whatever you want.

```csharp
[FunctionName("negotiate")]
public static IActionResult Run(
    [HttpTrigger(AuthorizationLevel.Function)] HttpRequest req,
    [SignalRConnectionInfo(HubName = "flightdata")] SignalRConnectionInfo connectionInfo,
    ILogger log)
{
    return new OkObjectResult(connectionInfo);
}
```
Run your Function App again and make a request to your **SignalRConnectionInfo** endpoint. You should see a service endpoint url which matches your deployed SignalR service in Azure and an access token for that service. 

![FN](Artifacts/FuncNegotiate.png)

4. The final thing to do for local development only is to set the **CORS** settings for your function app. This is done because locally your functions will be running on localhost but your web app is simply being served up from the file system. Add the following code snippet to your `local.settings.json` file to enable cross origin requests.

```json
  "Host": {
    "LocalHttpPort": 7071,
    "CORS": "*"
  }
```

With that done, we are now ready update the client app to connect to SignalR and start receiving real time updates to the flight data on the front end.

---

# Part 3 - Creating a static Web App

The web app we are going to build displays the "real-time" data on an Azure Map interface. In Part 3 of this tutorial, we will be setting up the map so that the data can be  displayed. 

## Create a new Map

1. Open up visual studio code and create a new project directory for this project.
2. Create a new file called `index.html` 
3. Copy the following boilerplate code and then we will fill rest.

```html
<!DOCTYPE html>
<html>

<head>
    <title></title>

    <meta charset="utf-8">

    <!-- Ensures that IE and Edge uses the latest version and doesn't emulate an older version -->
    <meta http-equiv="x-ua-compatible" content="IE=Edge">

    <!-- Ensures the web page looks good on all screen sizes. -->
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">

    <!-- Add references to the Azure Maps Map control JavaScript and CSS files. -->
    <link rel="stylesheet" href="https://atlas.microsoft.com/sdk/javascript/mapcontrol/2/atlas.min.css" type="text/css">
    <script src="https://atlas.microsoft.com/sdk/javascript/mapcontrol/2/atlas.min.js"></script>
    <script src="https://atlas.microsoft.com/sdk/javascript/service/2/atlas-service.min.js"></script>

    <style>
        html,
        body {
            margin: 0;
        }

        #myMap {
            height: 100vh;
            width: 100vw;
        }
    </style>

    <pre id='test'></pre>

    <script>

        var map, symbolLayer, popup, dataSource;

     function init() {
         //Add Map Control JavaScript code here.
     }
     </script>

</head>
<body onload="init()">
    <div id="myMap"></div>
</body>
</html>
```

5. Go ahead and grab the subscription key for your Azure Maps account that you created earlier.
6. Add the following javascript snippet to the `GetMap()` function and your subscription key to the placeholder.

```javascript
//Instantiate a map object
map = new atlas.Map("myMap", {
    //Add your Azure Maps subscription key to the map SDK. Get an Azure Maps key at https://azure.com/maps
    authOptions: {
        authType: 'subscriptionKey',
        subscriptionKey: '<Your Azure Maps Key>'
    }
});
```

7. Save your changes and open your `index.html` file in a browser. You should now see a really basic map of the world.

## Customize your map

To keep things simple and to ensure that this map of the world include New York, we will scope the map to New York specifically.
Add the following options/settings to the `init()` function, just below the `authOptions` section.

```javascript
center: [-73.9, 40.7],
zoom: 12
```

Refresh the page in your browser and notice, the map is now zoomed in on New York.

You might need to tweak the **center coordinates** and **zoom** settings to get a better fit for your screen size and if you are after different styles or other custom configurations, take a look at the **Map** component section of the docs.

- [Supported Styles](https://docs.microsoft.com/en-us/azure/azure-maps/supported-map-styles)
- [Map Control Docs](https://docs.microsoft.com/en-us/javascript/api/azure-maps-control/atlas.map?view=azure-iot-typescript-latest)

## Lets Add Some Taxi Data!!

Finally append the below code snippet to the `init()`function. Here we are adding a ready event to the map so that this logic gets executed only after the map has been initialized.

The snippet below does the following:

1. The first step is to create a new [DataSource](https://docs.microsoft.com/en-us/javascript/api/azure-maps-control/atlas.source.datasource?view=azure-iot-typescript-latest) which will keep track of the taxi flight data within the map.

2. Next, we create a new map [SymbolLayer](https://docs.microsoft.com/en-us/javascript/api/azure-maps-control/atlas.symbollayeroptions?view=azure-iot-typescript-latest) which describes how we want the flight data stored in the data source to be rendered on the map.

3. Finally, add controls for the map which will enable you to zoom or change the theme of your map
```javascript
//Wait until the map resources are ready.
map.events.add('ready', function () {
    //Create a data source and add it to the map.
    dataSource = new atlas.source.DataSource();
    map.sources.add(dataSource);

    //Create a symbol layer to render icons and/or text at points on the map.
    // var symbolLayer = new atlas.layer.SymbolLayer(dataSource);

    //Add a layer for rendering the route lines and have it render under the map labels.
    map.layers.add(new atlas.layer.LineLayer(dataSource, null, {
        strokeColor: '#2272B9',
        strokeWidth: 5,
        lineJoin: 'round',
        lineCap: 'round'
    }), 'labels');

    map.controls.add([
        new atlas.control.ZoomControl(),
        new atlas.control.CompassControl(),
        new atlas.control.PitchControl(),
        new atlas.control.StyleControl()
    ], {
        position: "top-right"
    });
})
```

Refresh your browser... you should now see controls in the top right corner of your map. 

---

# Part 3 - Connect the Web App to Azure SignalR

- Add the following script snippet to your `index.html` file to add the **signalR.js** dependencies to your web app.

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/3.1.7/signalr.min.js"></script>
```
Next we need to add some code in the `init()` function to initiate the connection with the SignalR service.

1. First add a new function that will connect to the signalR hub. Replace the placeholder parameter with your Function App name. 

```javascript
 const connection = new signalR.HubConnectionBuilder()
                    .withUrl('https://<FUNCTION APP NAME>.azurewebsites.net/api')
                    .withAutomaticReconnect()
                    .build()

```

2. Next, once the connection is initiated. We need to add code within the `init()` function to display the taxi data on the web app.

```javascript
connection.on('notify', (msg) => {
    const data = JSON.parse(msg)
    const pre = document.getElementById('test')
    for (const d of data) {
        console.log(d)

        //Create the GeoJSON objects which represent the start and end points of the route.
        var startPoint = new atlas.data.Feature(new atlas.data.Point([d.pickupLon, d.pickupLat]), {
            tripDistanceInMiles: d.tripDistanceInMiles,
            passengerCount: d.passengerCount,
            icon: "pin-round-blue"
        });

        var endPoint = new atlas.data.Feature(new atlas.data.Point([d.dropoffLon, d.dropoffLat]), {
            tripDistanceInMiles: d.tripDistanceInMiles,
            passengerCount: d.passengerCount,
            icon: "pin-round-red"
        });

        //Add the data to the data source.
        dataSource.add([startPoint, endPoint]);

        // Use SubscriptionKeyCredential with a subscription key
        var subscriptionKeyCredential = new atlas.service.SubscriptionKeyCredential(atlas.getSubscriptionKey());

        // Use subscriptionKeyCredential to create a pipeline
        var pipeline = atlas.service.MapsURL.newPipeline(subscriptionKeyCredential);

        // Construct the RouteURL object
        var routeURL = new atlas.service.RouteURL(pipeline);

        //Start and end point input to the routeURL
        var coordinates = [[startPoint.geometry.coordinates[0], startPoint.geometry.coordinates[1]], [endPoint.geometry.coordinates[0], endPoint.geometry.coordinates[1]]];

        //Make a search route request
        routeURL.calculateRouteDirections(atlas.service.Aborter.timeout(10000), coordinates).then((directions) => {
            //Get data features from response
            var data = directions.geojson.getFeatures();
            dataSource.add(data);
        });

        //Only show the most recent 16 taxi rides
        if (dataSource.getShapes().length > 48) {
            dataSource.remove(dataSource.getShapes().slice(0, 3));
        }

    }
})
connection.start();
```

3. Now that the connection is established, we need to add a symbol layer and some events to display useful information about the record. Add the following snippet also inside of the `init()` function 
```javascript
//Add a layer for rendering point data as symbols.
symbolLayer = new atlas.layer.SymbolLayer(dataSource, null, { iconOptions: { allowOverlap: true } });
map.layers.add(symbolLayer);

//Create a popup but leave it closed so we can update it and display it later.
popup = new atlas.Popup({
    position: [0, 0],
    pixelOffset: [0, -18]
});

//Close the popup when the mouse moves on the map.
map.events.add('mousemove', closePopup);

/**
 * Open the popup on mouse move or touchstart on the symbol layer.
 * Mouse move is used as mouseover only fires when the mouse initially goes over a symbol. 
 * If two symbols overlap, moving the mouse from one to the other won't trigger the event for the new shape as the mouse is still over the layer.
 */
map.events.add('mousemove', symbolLayer, symbolHovered);
map.events.add('touchstart', symbolLayer, symbolHovered);
```

    The `ProcessFlightData(flight)` method will be called each time the SignalR connection receives an update from the **flightdata** hub. At which point we create a new **shape** for the map to reflect the new properties, add the shape to the collection of planes in local memory and update the maps **datasource** with the update plane collection values.

4. Remove the call to `GetFlightData()` from the `GetMap()` function as we now handle this in the `ProcessFlightData(flight)` method we just created.

5. Where the datasource is initialized, remove the `var` tag so that it gets set to the local variable we also just created.  

6. The final step is to initialize the **SiganlR** connection and tell the signalr.js client which method to invoke on our web app each time it receives updates.

    - Add the following code snippet to the Map **ready** event so that the connection gets created once the map has been loaded.
    - This logic makes a request to the `GetConnectionInfo` method to get the url and access token of the SignalR service.
    - Once a token has been acquired, create a new connection using the `SignalR.HubConnectionBuilder` method, passing in the url and access token as options.
    - Start the connection and once connected, each time a new message appears in the **newFlightData** hub, make a call to the `ProcessFlightData(flight)` method.
    - If the connection gets lost, retry starting the connection after a couple of seconds

```javascript
GetConnectionInfo().then(function (info) {
    let accessToken = info.accessToken
    const options = {
        accessTokenFactory: function () {
            if (accessToken) {
                const _accessToken = accessToken
                accessToken = null
                return _accessToken
            } else {
                return GetConnectionInfo().then(function (info) {
                    return info.accessToken
                })
            }
        }
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(info.url, options)
        .build()

    StartConnection(connection)

    connection.on('newFlightData', ProcessFlightData)

    connection.onclose(function () {
        console.log('disconnected')
        setTimeout(function () { StartConnection(connection) }, 5000)
    })           
}).catch(console.error)
```

#### Thats everything wired up!
You should now be able to run your Azure Function App, open your web app in a browser and after a couple seconds, see some flights rendered on the map. Open up the console to view trace logs if you want to inspect the flight data objects. 

![RTFLM](Artifacts/RealTimeFlightMap.gif)

Obviously this is only just scratching the surface of what we could do with this particular example or even with other use cases for real time web apps using Azure Functions, CosmosDB and SignalR. I hope you enjoyed this exercise as much as I did putting it together.

Thank You!

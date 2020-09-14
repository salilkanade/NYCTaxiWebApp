# Real-time Data Visualization with Azure Stream Analytics, Azure Functions, SignalR and Azure Maps

## What we will cover

In this tutorial, we will process and visualize data coming from NYC Taxi trips on a serverless web app utilizing several Azure technologies.

- To get the NYC taxi data information, we will create a Virtual Machine from the provisioned image. This VM is using historical data from NYC taxi and will replay the events and send them to an Event Hub.
- The Stream Analytics job will then process and transform the data received from the Event Hub in real-time.
- We will then create an Azure Function that listens to the Stream Analytics job and updates an Azure SignalR hub with all changes to the taxi data. 
- The web app will be configured with a SignalR client to handle the data changes in real-time and use Azure Maps to visualize the data

For an introduction to the architecture, familiarize yourself with the picture below. 
<img width="1000" alt="DemoArch" src="https://user-images.githubusercontent.com/68666863/92974360-de1dd900-f43a-11ea-88ec-01b6b62e5b96.PNG">

## What you will need

- [Microsoft Azure Account](https://azure.microsoft.com/en-us/free/)
- Visual Studio Code
    - Install the *Azure Functions*, *Azure Account*, *Azure Serverless*, *C#*, *Live Server*, *Azure App Service* extensions
- Azure Function CLI Tools *(Will be downloaded automatically when debugging functions locally)*

# Creating the Azure Resources

## Azure Maps

[**Azure Maps from the docs**](https://docs.microsoft.com/en-us/azure/azure-maps/about-azure-maps)

### Create a new Azure Maps resource

1. In the upper left corner of the portal, click on **Create a resource**
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
6. Once the new Azure Maps resource has been provisioned, navigate to the newly deployed resource and locate the **Authentication** tab under the **Settings** subheading. You will need to grab the key later on.

---

## Azure SignalR

[**Azure SignalR from the docs**](https://docs.microsoft.com/en-us/azure/azure-signalr/signalr-overview)

### Create a new Azure SignalR resource

1. In the upper left corner of the portal, click on **Create a resource**
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

1. In the upper left corner of the portal, click on **Create a resource**
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

1. In the upper left corner of the portal, click on **Create a resource**
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

## Azure Event Hubs

[**Azure Event Hubs from the docs**](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-about)

### Create a new Event Hub namespace

1. In the upper left corner of the portal, click on **Create a resource**
2. Type in **Event Hubs** in the search bar and select **Event Hubs** in the dropdown.
3. Click the **Create** button that appears on the Event Hubs page.
4. Enter the following information into the **Create Namespace** template

    | Name              | Value |
    | ---               | ---   |
    | Subscription      | Select your subscription
    | Resource Group    | Select the resource group created above
    | Namespace name    | Give your event hub namespace app a meaningful name
    | Location          | Select a location to deploy your event hub namespace too
    | Pricing tier      | Select **Standard or Basic** from the dropdown menu
    | Throughput Units  | Leave it at 3
   
5. Click **Review + Create** and then once the validation has passed, select **Create**

---

# Part 1 - Creating the Virtual Machine and Event Hubs

## Create the VM from the provisioned image

## Set up the Event Hub to gather records from the Virtual Machine


---

# Part 2 - Building Azure Function

In Part 2 of this tutorial, we will now focus on building out the Azure Functions which will enable real time data updates for the taxi data on our map.
The following image describes the flow we are looking to create to enable real time functionality.

1. A record or change is updated in the Event Hub. 
2. The updated record is propogated to the Stream Analytics job
3. The record is processed and outputted from the Stream Analytics job to the Azure Function
3. An Azure Functions is triggered by the change event using an HTTP trigger
4. The SignalR Service output binding publishes a message to SignalR Service
5. SignalR Service publishes the message to all connected clients

[Take a look at the docs if you want to explore this pattern a little further](https://docs.microsoft.com/en-us/azure/azure-signalr/signalr-concept-azure-functions)

## HTTP Trigger Function

1. Open up **Visual Studio Code** and create a new **New File**.
2. Copy and paste the code below into your new file. There is a function called message which relays the records from the Stream Analytics job to the SignalR hub.
3. Make sure the code is editted to include your specific SignalR hub name. 

```CSharp
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

public static class NegotiateFunction
{
    [FunctionName("negotiate")]
    public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
                                    [SignalRConnectionInfo(HubName = "taxidata")] SignalRConnectionInfo info,
                                    ILogger log)
    {
        return info != null
            ? (ActionResult)new OkObjectResult(info)
            : new NotFoundObjectResult("Failed to load SignalR Info.");
    }
}
```

To use the SignalR binding extensions you need to add the [Microsoft.Azure.WebJobs.Extensions.SignalRService](https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.SignalRService) package from nuget as a dependency to your project.

4. Add an app setting to the **local.settings.json** file called `AzureSignalRConnectionString` and set the value to the connection string for SignalR service.

The next thing to do is to create a mechanism to push the taxi data to our SignalR Service that we provisioned earlier. Lets cover that next.

## SignalR Outbound Trigger

In the same Azure Function namespace, we will then use the [SignalR output binding](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-signalr-service#using-signalr-service-with-azure-functions) to push the updated flight data to SignalR. 

```csharp
namespace HTTPTrigger
{   
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

If you run your function app now, you should get both your negotiate and message functions spinning up at the same time. If everything is hooked up correctly you should see some log output to the console like below showing how many flights were added into the database and subsequently processed by the change feed.
    
1. The next step to add the SignalR output binding. To use this binding you will need add the **Microsoft.Azure.WebJobs.Extensions.SignalRService** package dependency from nuget to your project.

2. With the package installed add the binding to your function as per below, setting the **HubName** attribute to your specific SignalR hub name.

    - The **Target** property is the name of the function to be invoked on the client and the **Arguments** property is the array of objects to be passed to the client.

Your function should now be complete and resemble the logic below.

```csharp
namespace HTTPTrigger
{   
    public static class NegotiateFunction
    {
        [FunctionName("negotiate")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
                                        [SignalRConnectionInfo(HubName = "taxidata")] SignalRConnectionInfo info,
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
                                                    [SignalR(HubName = "taxidata")] IAsyncCollector<SignalRMessage> signalRMessages,
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

3. The final thing to do before we run this function is to add the connection string for the SignalR Service to the functions config.

    - Create a new setting property called `"AzureSignalRConnectionString"` and set the value to the connection string for your SignalR instance in Azure.

Once thats done, your functions are all set to go. Let's spin these functions up once again and test out the changes. With the functions running, head on over back the SignalR service in Azure and have a look at the metrics tab. After a couple of minutes you should see some telemetry start to feed through. It can take up to 10-15 minutes before you see data coming through on the metrics blade.

Run your Function App again and make a request to your **SignalRConnectionInfo** endpoint. You should see a service endpoint url which matches your deployed SignalR service in Azure and an access token for that service. 

4. The final thing to do for local development only is to set the **CORS** settings for your function app. This is done because locally your functions will be running on localhost but your web app is simply being served up from the file system. Add the following code snippet to your `local.settings.json` file to enable cross origin requests.

```json
  "Host": {
    "CORS": "http://localhost:8000"
    "CORSCredentials": true
  }
```

With that done, we are now ready update our Stream Analytics jobs to complete the backend data flow. Once this is done, we can display the simulated "real-time" data in a serverless environment.

---

# Part 3 - Setting Up the Stream Analytics Job

Streaming Analytics job consists of an input, query, and an output. This Stream Analytics job injests the data from the Azure Event Hubs and the query (which is SQL based) can be used to easily filter, sort, and join streaming data to output to our Azure Functions. 

## Set up the Stream Analytics job to query and process the data
1. Head over to your Stream Analytics job that we created in the first step within the Azure portal.
2. Click **Inputs** under the Job topology section.
3. Next, click **Add Stream Input** and select **Event Hub**
4. Name the input alias as **TaxiRide** and fill out select the Event Hub you created right before in the drop down.
5. Next, click **Save** and once the connection is successful, click **Outputs** under the Job topology section in the left-hand menu bar.
6. In the **Ouputs** tab, click **Add** and select **Azure Function** from the dropdown menu.
7. Name the output alias as **ASAFunction** and select the **Provide azure function settings manually**
8. Provide the correct subscription where the function app was created, and provide the correct function app name.
9. Under the **Function** query box, type **message** and provide the appropriate key from the Function App. 
10. Next, click **Query** and copy and paste the following code for the SQL Query:

```SQL
--SELECT all relevant fields from TaxiRide Streaming input
WITH 
TripData AS (
    SELECT TRY_CAST(pickupLat AS float) as pickupLat,
    TRY_CAST(pickupLon AS float) as pickupLon,
    TRY_CAST(dropoffLon AS float) as dropoffLon,
    TRY_CAST(dropoffLat AS float) as dropoffLat,
    TRY_CAST(passengerCount as float) as passengerCount, TripTimeinSeconds, pickupTime, VendorID, tripDistanceInMiles
    FROM TaxiRide timestamp by pickupTime
    WHERE pickupLat > -90 AND pickupLat < 90 AND pickupLon > -180 AND pickupLon < 180
),
RegionAgg AS (
SELECT RegionReferenceData.Name AS countyName, system.timestamp as timestamps, TripData.pickupLat, TripData.pickupLon, TripData.pickupTime, TripData.VendorID, TripData.tripDistanceInMiles, TripData.dropoffLon, TripData.dropoffLat, TripData.passengerCount, TripData.TripTimeinSeconds
FROM TripData
JOIN RegionReferenceData ON 
ST_WITHIN(CreatePoint(TripData.pickupLat, TripData.pickupLon), RegionReferenceData.geometry) = 1
)

SELECT *
INTO ASAFunction
FROM RegionAgg
```

11. Click **Save Query**
12. Go back to the **Overview** page and start the Stream Analytics job
12. Now once the job has started, click the **Metrics** page and make sure input and output events are being processed correctly. 
14. Go back to the Query page, and click the TaxiRide input, the following data should show up in the input preview:

<img width="966" alt="SampleInputData" src="https://user-images.githubusercontent.com/68666863/93040670-5f25cd80-f5ff-11ea-8e7f-067358037982.PNG"> 

If the input preview shows sample data similar to this, we are ready to start developing the front end so that our data can be displayed.

---

# Part 4 - Creating a static Web App

The web app we are going to build displays the "real-time" data on an Azure Map interface. In Part 4 of this tutorial, we will be setting up the map so that the data can be  displayed. 

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

1. The first step is to create a new [DataSource](https://docs.microsoft.com/en-us/javascript/api/azure-maps-control/atlas.source.datasource?view=azure-iot-typescript-latest) which will keep track of the taxi data within the map.

2. Next, we create a new map [SymbolLayer](https://docs.microsoft.com/en-us/javascript/api/azure-maps-control/atlas.symbollayeroptions?view=azure-iot-typescript-latest) which describes how we want the taxi data stored in the data source to be rendered on the map.

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

# Part 4 - Connect the Web App to Azure SignalR

In Part 4, we will now be adding some code to make the map interactive and functional.

- Add the following script snippet to your `index.html` file to add the **signalR.js** dependencies to your web app.

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/3.1.7/signalr.min.js"></script>
```
Next, we need to add some code in the `init()` function to initiate the connection with the SignalR service.

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

3. Now that the connection is established, we need to add a symbol layer and some events to display useful information about the record. Add the following snippet also inside of the `init()` function.

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
4. Next, we need to add create two functions for the popups on each pin to display the taxi ride information.

```
function closePopup() {
    popup.close();
}

function symbolHovered(e) {
    //Make sure the event occurred on a shape feature.
    if (e.shapes && e.shapes.length > 0) {
        var properties = e.shapes[0].getProperties();

        //Update the content and position of the popup.
        popup.setOptions({
            //Create the content of the popup.
            content: `<div style="padding:10px;"> 'Trip Distance: ${properties.tripDistanceInMiles} Miles'<br/> 'Passenger Count: ${properties.passengerCount}' </div>`,
            position: e.shapes[0].getCoordinates(),
            pixelOffset: [0, -18]
        });
        //Open the popup.
        popup.open(map);
    }
}
```

5. The final step now is to save your update `index.html` file. Your [index.html](https://github.com/salilkanade/NYCTaxiWebApp/blob/master/NYCTaxi/index.html) code should look similar to the page hyperlinked. 
6. Once that has been completed, head over to your storage account in the Azure online portal. 

- Click on **Static Website** on the side tab, and click **Enable**. 
- Type in `index.html` into the **Index document name** 
- Finally, click on **Storage Explorer (preview)**. Under *Blob Containers*, there should be a `$web` in the dropdown. Click on that and verify that the `index.html` file listed is the correct one.

#### That's everything wired up!
You should now be able to run your Azure Function App, open your web app in a browser and after a couple seconds, see your taxi data rendered on the map. Open up the console to view trace logs if you want to inspect the taxi data objects. 


Obviously this is only just scratching the surface of what we could do with this particular example or even with other use cases for real time serverless web apps using Stream Analytics, Azure Functions, and SignalR. I hope you enjoyed this tutorial as much as I did putting it together.

Thank You!

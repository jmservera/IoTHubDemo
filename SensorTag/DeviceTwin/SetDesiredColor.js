 /* jshint node: true */
 'use strict';
 var iothub = require('azure-iothub');
 var connectionString = process.env.CONNECTIONSTRING;
if(connectionString==="" || connectionString === undefined){
    console.log("Set your env value CONNECTIONSTRING to a valid IoT Hub device connection string");
    return;
}

 var registry = iothub.Registry.fromConnectionString(connectionString);

 registry.getTwin('sensortag', function(err, twin){
     if (err) {
         console.error(err.constructor.name + ': ' + err.message);
     } else {
         var patch = {
             tags: {
                 location: {
                     country: 'ES',
                     region: 'Madrid'
               }
             },
             properties:{
                desired:{
                    background:{
                        color: process.argv[2] || "#ff0000"
                    }
                }
             }
         };

         twin.update(patch, function(err) {
           if (err) {
             console.error('Could not update twin: ' + err.constructor.name + ': ' + err.message);
           } else {
             console.log(twin.deviceId + ' twin updated successfully, looping 5 seconds for update');
             queryTwins();
             setInterval(queryTwins, 5000);
           }
         });
         
     }
 });

 /*var queryTwins = function() {
     var query = registry.createQuery("SELECT * FROM devices WHERE tags.location.plant = 'Madrid'", 100);
     query.nextAsTwin(function(err, results) {
         if (err) {
             console.error('Failed to fetch the results: ' + err.message);
         } else {
             console.log("Devices in Madrid: " + results.map(function(twin) {return twin.deviceId}).join(','));
         }
     });

     query = registry.createQuery("SELECT * FROM devices WHERE tags.location.plant = 'Madrid' AND properties.reported.connectivity.type = 'cellular'", 100);
     query.nextAsTwin(function(err, results) {
         if (err) {
             console.error('Failed to fetch the results: ' + err.message);
         } else {
             console.log("Devices in Madrid using cellular network: " + results.map(function(twin) {return twin.deviceId}).join(','));
         }
     });
 };*/

  var queryTwins = function() {
     var query = registry.createQuery("SELECT * FROM devices WHERE deviceId = 'sensortag'", 100);
     query.nextAsTwin(function(err, results) {
         if (err) {
             console.error('Failed to fetch the results: ' + err.message);
         } else {
             console.log();
             results.forEach(function(twin) {
                 var desiredConfig = twin.properties.desired;
                 var reportedConfig = twin.properties.reported;
                 var tags=twin.tags;
                 console.log("Tags for: "+twin.deviceId);
                 console.log(JSON.stringify(tags,null,2));
                 console.log("Config report for: " + twin.deviceId);
                 console.log("Desired: ");
                 console.log(JSON.stringify(desiredConfig, null, 2));
                 console.log("Reported: ");
                 console.log(JSON.stringify(reportedConfig, null, 2));
             });
         }
     });
 };

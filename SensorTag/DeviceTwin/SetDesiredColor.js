 /* jshint node: true*/
 /* jshint  esversion: 6*/

/* Example

node SetDesiredColor.js "#ffffff" -q "SELECT deviceId, tags.location.region FROM devices where tags.location.region='Madrid'"

*/

 'use strict';

 var iothub = require('azure-iothub');
 var program= require('commander');

 program
 .version('0.0.1')
  .arguments('[color]')
  .option('-q, --sqlQuery <sqlQuery>', 'The query to run.')
  .option('-C, --connectionString <connectionString>', 'The IoT Hub Connection String')
  .option('--country <country>','The country to set to the twin','ES')
  .option('--region <region>','The region where this device is located','Madrid')
  .option('-n, --deviceName <deviceName>', 'The device name','sensortag')
  .option('-i --interval <interval>','The interval for querying, if 0 it only performs one query',0)
  .action(function(color) {
      action(color);
  })
  .parse(process.argv);

if(program.color===undefined){
    action("#FF0000");
}

function action(color){
    console.log(`Color: ${color} query: ${program.sqlQuery} conn: ${program.connectionString} country: ${program.country} region: ${program.region} name: ${program.deviceName}`);
      var connectionString=program.connectionString||process.env.CONNECTIONSTRING;
      if(connectionString==="" || connectionString === undefined){
        console.log("Set your env value CONNECTIONSTRING to a valid IoT Hub device connection string");
        return;
      }
      var sqlQuery=program.sqlQuery||"SELECT * FROM devices WHERE deviceId = '"+program.deviceName+"'";
      start(connectionString,program.deviceName,sqlQuery,color|| "#ff0000",program.interval, program.country, program.region);
}

function start(connectionString, deviceName,sqlQuery,color, interval, country, region){
    console.log(`${connectionString} ${deviceName} ${sqlQuery} ${color}`);
 var registry = iothub.Registry.fromConnectionString(connectionString);
var twinsQuery=sqlQuery;
 
 registry.getTwin(deviceName, function(err, twin){
     if (err) {
         console.error(err.constructor.name + ': ' + err.message);
     } else {
         var patch = {
             tags: {
                 location: {
                     country: country,
                     region: region
               }
             },
             properties:{
                desired:{
                    background:{
                        color: color
                    }
                }
             }
         };

         twin.update(patch, function(err) {
           if (err) {
             console.error('Could not update twin: ' + err.constructor.name + ': ' + err.message);
           } else {
             console.log(`${twin.deviceId} twin updated successfully, looping ${interval}ms for update`);
             queryTwins();
             if(interval>0){
                setInterval(queryTwins, interval);
             }
           }
         });
         
     }
 });

  var queryTwins = function() {
     var query = registry.createQuery(twinsQuery, 100);
     query.next(function(err, results) {
         if (err) {
             console.error('Failed to fetch the results: ' + err.message);
         } else {
             console.log();
             results.forEach(function(twin) {
                 console.log(JSON.stringify(twin,null,2));
             });
         }
     });
 };
}

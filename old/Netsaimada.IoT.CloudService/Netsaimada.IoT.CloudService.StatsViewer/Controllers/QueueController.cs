using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Netsaimada.IoT.CloudService.StatsViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Netsaimada.IoT.CloudService.StatsViewer.Controllers
{
    public class QueueController : Controller
    {
        // GET: Queue
        public async Task<ActionResult> Index()
        {
            string serviceBusConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);

            string eventHubName = "telemetry";

            var client = EventHubClient.CreateFromConnectionString(serviceBusConnectionString, eventHubName);
            var data = await client.GetRuntimeInformationAsync();
            
            var vm = new QueueViewModel { RuntimeInfo = data };
            return View(vm);
        }

        public ActionResult ResetQueue()
        {
            string serviceBusConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);


            string eventHubName = "telemetry";

            var client = EventHubClient.CreateFromConnectionString(serviceBusConnectionString, eventHubName);
            //var data = await client.GetRuntimeInformationAsync();
            return RedirectToAction("Index");
        }
        // GET: Queue/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Queue/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Queue/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Queue/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Queue/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Queue/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Queue/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

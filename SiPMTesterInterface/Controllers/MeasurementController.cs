﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetMQ;
using NetMQ.Sockets;
using SiPMTesterInterface.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SiPMTesterInterface.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeasurementController : ControllerBase
    {
        private static readonly MainViewModel _viewModel = new MainViewModel
        {
            SelectedSiPMs = new SiPMModel
            {
                SelectedSiPMs = new List<List<string>>
            {
                new List<string> { "Up - 0", "Down - 0", "Up - 1", "Down - 2" },
                new List<string> { "Up - 2", "Down - 2", "Up - 4", "Down - 4" },
                new List<string> { "Up - 3", "Down - 3", "Up - 12", "Down - 12" },
                new List<string> { "Up - 16", "Down - 16", "Up - 16", "Down - 16" }
            }
            },
            IV = new IVModel
            {
                State = "Running",
                CurrentArray = 0,
                CurrentSiPM = 0,
                Voltage = 0.0
            },
            SPS = new SPSModel
            {
                State = "Stopped",
                Current = new List<List<string>>
            {
                new List<string>(),
                new List<string> { "Up - 4", "Down - 4" },
                new List<string> { "Up - 3", "Down - 3" },
                new List<string> { "Up - 16", "Down - 16" }
            }
            },
            Results = new ResultsModel
            {
                IV = new List<List<string>>
            {
                new List<string> { "OK", "OK", "Not tested", "Not tested" },
                new List<string> { "Error", "Error", "Not tested", "Not tested" },
                new List<string> { "OK", "OK", "Not tested", "Not tested" },
                new List<string> { "OK", "Error", "Not tested", "Not tested" }
            },
                SPS = new List<List<string>>
            {
                new List<string> { "OK", "OK", "Not tested", "Not tested" },
                new List<string> { "Error", "Error", "Not tested", "Not tested" },
                new List<string> { "OK", "OK", "Not tested", "Not tested" },
                new List<string> { "OK", "Error", "Not tested", "Not tested" }
            }
            }
        };

        private readonly ILogger<MeasurementController> _logger;
        private readonly RequestSocket reqSocket = new RequestSocket();

        private bool AskServer(string message, out string response)
        {
            string received = "";
            bool success = (reqSocket.TrySendFrame(TimeSpan.FromSeconds(2), message) && reqSocket.TryReceiveFrameString(TimeSpan.FromSeconds(2), out received));
            response = received;
            //_logger.LogInformation(response);
            return success;
        }

        public MeasurementController(ILogger<MeasurementController> logger)
        {
            _logger = logger;
            reqSocket.Connect("tcp://192.168.0.45:5556");
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_viewModel);
        }

        [HttpGet("state")]
        public IActionResult GetMeasurementStates()
        {
            //reqSocket.SendFrame("Hello");
            
            string received = "";
            bool success = AskServer("GetState", out received);
            if (success)
            {
                //var msg = reqSocket.ReceiveFrameString();
                _logger.LogInformation(received);
                
            }
            else
            {
                return BadRequest("Server Unavailable");
            }

            int state = 0;
            int.TryParse(received, out state);
            var measurementStates = new
            {
                IV = state,
                SPS = 0
            };
            return Ok(measurementStates);
        }
    }
}


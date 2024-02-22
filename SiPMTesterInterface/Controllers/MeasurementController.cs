using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetMQ;
using NetMQ.Sockets;
using SiPMTesterInterface.ClientApp.Services;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SiPMTesterInterface.Controllers
{
    //for testing the endpooint
    public class MeasurementStatesDto
    {
        public MeasurementState IVState { get; set; }
        public MeasurementState SPSState { get; set; }
    }


    [ApiController]
    [Route("[controller]")]
    public class MeasurementController : ControllerBase
    {
        private readonly MeasurementService _measurementService;

        private readonly ILogger<MeasurementController> _logger;
        //private readonly RequestSocket reqSocket = new RequestSocket();

        /*
        private bool AskServer(string message, out string response)
        {
            string received = "";
            bool success = (reqSocket.TrySendFrame(TimeSpan.FromSeconds(2), message) && reqSocket.TryReceiveFrameString(TimeSpan.FromSeconds(2), out received));
            response = received;
            //_logger.LogInformation(response);
            return success;
        }
        */

        public MeasurementController(ILogger<MeasurementController> logger, MeasurementService measurementService)
        {
            _logger = logger;
            //reqSocket.Connect("tcp://192.168.0.45:5556");
            _measurementService = measurementService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }

        [HttpGet("states")]
        public IActionResult GetMeasurementStates()
        {
            var measurementStates = new
            {
                IVConnectionState = _measurementService.IVConnectionState,
                SPSConnectionState = _measurementService.SPSConnectionState,
                IVState = _measurementService.IVState,
                SPSState = _measurementService.SPSState
            };
            return Ok(measurementStates);
        }

        [HttpPost("states")]
        public IActionResult UpdateMeasurementStates([FromBody] MeasurementStatesDto measurementStates)
        {
            // Validate the received data
            if (measurementStates == null)
            {
                return BadRequest("Invalid input data");
            }

            _measurementService.UpdateIVState(measurementStates.IVState);
            _measurementService.UpdateSPSState(measurementStates.SPSState);

            // Log the received data
            _logger.LogInformation($"Received measurement states: IVState = {measurementStates.IVState}, SPSState = {measurementStates.SPSState}");

            // Return a success response
            return Ok("Measurement states updated successfully");
        }

        //leave here... using this snippet later
        [HttpGet("ivendpoint")]
        public IActionResult GetIVEndpointState()
        {
            var measurementStates = new
            {
                IV = 0,
                SPS = 0
            };
            return Ok(measurementStates);
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using SiPMTesterInterface.ClientApp.Services;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;
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
                IVState = _measurementService.GlobalIVState,
                SPSState = _measurementService.GlobalIVState //only for testing
            };
            return Ok(measurementStates);
        }

        
        [HttpGet]
        [Route("getsipmdata/{blockId}/{moduleId}/{arrayId}/{sipmId}/")]
        public IActionResult GetSIPMData(int blockId, int moduleId, int arrayId, int sipmId)
        {
            CurrentMeasurementDataModel data = _measurementService.GetSiPMMeasurementData(blockId, moduleId, arrayId, sipmId);

            return Ok(data);
        }

        [HttpGet("data")]
        public IActionResult GetMeasurementData()
        {
            _measurementService.StopMeasurement();
            return Ok(_measurementService.MeasurementData);
        }

        [HttpGet("stop")]
        public IActionResult StopMeasurement()
        {
            return Ok(_measurementService.MeasurementData);
        }

        [HttpPost("start")]
        public IActionResult StartMeasurement([FromBody] MeasurementStartModel measurementStart)
        {
            if (_measurementService.GlobalIVState == MeasurementState.Running || _measurementService.GLobalSPSState == MeasurementState.Running)
            {
                return BadRequest(ResponseMessages.Error("Measurements already running"));
            }
            // Validate the received data
            if (measurementStart == null)
            {
                return BadRequest(ResponseMessages.Error("Empty start data frame"));
            }

            _logger.LogInformation($"MeasurementStart information received: {JsonConvert.SerializeObject(measurementStart)}");

            _measurementService.StartMeasurement(measurementStart);

            // Return a success response
            return Ok("Measurement start requested successfully");
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


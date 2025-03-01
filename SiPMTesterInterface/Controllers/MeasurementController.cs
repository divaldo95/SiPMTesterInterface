﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SiPMTesterInterface.ClientApp.Services;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;
using SiPMTesterInterface.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            _measurementService = measurementService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }

        [HttpGet("states")]
        public IActionResult GetInstrumentStates()
        {
            var instrumentStates = new
            {
                CurrentTask = _measurementService.CurrentTask,
                IVConnectionState = _measurementService.IVConnectionState,
                SPSConnectionState = _measurementService.SPSConnectionState,
                IVState = _measurementService.GlobalIVState,
                SPSState = _measurementService.GLobalSPSState //only for testing
            };
            return Ok(instrumentStates);
        }

        [HttpGet("pulsers")]
        public IActionResult GetPulserValues()
        {
            return Ok(_measurementService.PulserValues);
        }

        [HttpPost("pulsers")]
        public IActionResult SetPulserValue([FromBody] SiPMPulserValue pulserValue)
        {
            if (_measurementService.CurrentTask != TaskTypes.Finished && _measurementService.CurrentTask != TaskTypes.Idle)
            {
                return BadRequest(ResponseMessages.Error("Measurements already running"));
            }
            // Validate the received data
            if (pulserValue == null)
            {
                return BadRequest(ResponseMessages.Error("Empty pulser value data"));
            }

            _logger.LogInformation($"Pulser value information received: {JsonConvert.SerializeObject(pulserValue)}");
            try
            {
                //LEDValueHelper.UpdatePulserValueForSiPM(pulserValue.SiPM, pulserValue.PulserValue, _measurementService.PulserValues);
                _measurementService.PulserValues.SetPulserValue(pulserValue.SiPM, pulserValue.PulserValue);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }

            // Return a success response
            return Ok("Pulser value updated successfully");
        }

        [HttpPost("pulsers/offset")]
        public IActionResult SetPulserArrayOffset([FromBody] SiPMPulserValue pulserValue)
        {
            if (_measurementService.CurrentTask != TaskTypes.Finished && _measurementService.CurrentTask != TaskTypes.Idle)
            {
                return BadRequest(ResponseMessages.Error("Measurements already running"));
            }
            // Validate the received data
            if (pulserValue == null)
            {
                return BadRequest(ResponseMessages.Error("Empty pulser value data"));
            }

            _logger.LogInformation($"Pulser offset information received: {JsonConvert.SerializeObject(pulserValue)}");
            try
            {
                //LEDValueHelper.UpdatePulserValueForSiPM(pulserValue.SiPM, pulserValue.PulserValue, _measurementService.PulserValues);
                _measurementService.PulserValues.SetArrayOffset(pulserValue.SiPM, pulserValue.PulserValue);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }

            // Return a success response
            return Ok("Pulser offset updated successfully");
        }


        [HttpGet]
        [Route("getsipmdata/{blockId}/{moduleId}/{arrayId}/{sipmId}/")]
        public IActionResult GetSIPMData(int blockId, int moduleId, int arrayId, int sipmId)
        {
            try
            {
                CurrentMeasurementDataModel data = _measurementService.GetSiPMMeasurementData(blockId, moduleId, arrayId, sipmId);
                return Ok(data);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return NotFound(ResponseMessages.Error(ex.Message));
            }
            catch(NullReferenceException ex)
            {
                return NotFound(ResponseMessages.Error("Measurement not started yet"));
            }
        }

        [HttpGet]
        [Route("getsipmdata/{blockId}/{moduleId}/{arrayId}/{sipmId}/RootFile/IV")]
        public IActionResult GetSIPMIVRootFile(int blockId, int moduleId, int arrayId, int sipmId)
        {
            try
            {
                CurrentMeasurementDataModel data = _measurementService.GetSiPMMeasurementData(blockId, moduleId, arrayId, sipmId);
                if (!data.IVResult.AnalysationResult.Analysed || string.IsNullOrWhiteSpace(data.IVResult.AnalysationResult.RootFileLocation))
                {
                    return NotFound(ResponseMessages.Error("Not analysed or Root file is not available"));
                }
                if (!System.IO.File.Exists(data.IVResult.AnalysationResult.RootFileLocation))
                {
                    return NotFound(ResponseMessages.Error("Root file is not available"));
                }

                // Return the file as a physical file result
                string filename = Path.GetFileName(data.IVResult.AnalysationResult.RootFileLocation);
                return PhysicalFile(data.IVResult.AnalysationResult.RootFileLocation, "application/octet-stream", filename);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return NotFound(ResponseMessages.Error(ex.Message));
            }
            catch (NullReferenceException ex)
            {
                return NotFound(ResponseMessages.Error("Measurement not started yet"));
            }
        }

        [HttpGet]
        [Route("getsipmdata/TestRootFile")]
        public IActionResult GetStaticTestSIPMIVRootFile()
        {
            try
            {
                // Return the file as a physical file result
                string rootFilePath = "test.root";
                if (!System.IO.File.Exists(rootFilePath))
                {
                    return NotFound(ResponseMessages.Error("Root file is not available"));
                }

                
                string filename = Path.GetFileName(rootFilePath);
                return PhysicalFile(rootFilePath, "application/octet-stream", filename);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return NotFound(ResponseMessages.Error(ex.Message));
            }
            catch (NullReferenceException ex)
            {
                return NotFound(ResponseMessages.Error("Measurement not started yet"));
            }
        }

        [HttpGet("times")]
        public IActionResult GetMeasurementTimes()
        {
            var measurementTimes = new
            {
                Start = _measurementService.StartTimestamp,
                End = _measurementService.EndTimestamp,
                Elapsed = _measurementService.EndTimestamp - _measurementService.StartTimestamp
            };
            return Ok(measurementTimes);
        }

        [HttpGet("data")]
        public IActionResult GetMeasurementData()
        {
            // returns current run configuration (MeasurementStartModel)
            return Ok(_measurementService.MeasurementData);
        }

        [HttpGet("logs/all")]
        public IActionResult GetAllLogs()
        {
            var errors = _measurementService.GetAllLogs();
            return Ok(errors);
        }

        [HttpGet("logs/needsattention")]
        public IActionResult GetNeedsAttentionLogs()
        {
            var errors = _measurementService.GetAttentionNeededLogs();
            return Ok(errors);
        }

        [HttpGet("logs/unresolved")]
        public IActionResult GetUnresolvedLogs()
        {
            var errors = _measurementService.GetUnresolvedLogs();
            return Ok(errors);
        }

        [HttpPost("logs/resolve")]
        public IActionResult ResolveLogMessage([FromBody] ErrorResolveModel data)
        {
            try
            {
                Console.WriteLine($"Received data: {data}");

                _measurementService.TryResolveLog(data);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            } 
        }

        [HttpGet("measurementstates")]
        public IActionResult GetSiPMMeasurementStates()
        {
            // returns the SiPM states of the current run (waiting, mesured, analyzed)
            try
            {
                return Ok(_measurementService.GetSiPMMeasurementStatesJSON());
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }
            
        }

        [HttpPost("stop")]
        public IActionResult StopMeasurement()
        {
            try
            {
                _measurementService.StopMeasurement();
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }
            
            return Ok();
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
            try
            {
                _measurementService.StartMeasurement(measurementStart);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }

            // Return a success response
            return Ok("Measurement start requested successfully");
        }

        // To check if force restart is available or not
        [HttpGet("forcerestart")]
        public IActionResult ForceRestartDetails()
        {
            if (_measurementService.CurrentTask == TaskTypes.Idle || _measurementService.CurrentTask == TaskTypes.Finished) //sps
            {
                return BadRequest(ResponseMessages.Error("Measurements probably not running"));
            }
            bool canRequest;
            long elapsedTime;
            long waitingTime;
            _measurementService.GetForceRestartInformation(out canRequest, out elapsedTime, out waitingTime);


            // Return a success response
            return Ok(new
            {
                CanRequest = canRequest,
                ElapsedTime = elapsedTime,
                WaitingTime = waitingTime
            });
        }

        [HttpPost("forcerestart")]
        public IActionResult ForceRestartCurrentMeasurement()
        {
            if (_measurementService.CurrentTask == TaskTypes.Idle || _measurementService.CurrentTask == TaskTypes.Finished) //sps
            {
                return BadRequest(ResponseMessages.Error("Measurements probably not running"));
            }

            _logger.LogInformation($"Measurement force restart asked");
            try
            {
                _measurementService.ForceRestartCurrent();
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }

            // Return a success response
            return Ok("Measurement restart requested successfully");
        }

        [HttpGet("start")]
        public IActionResult GetCurrentRun()
        {
            try
            {
                return Ok(_measurementService.CurrentRun);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }
        }

        [HttpGet("getarrayprops/{sn}")]
        public IActionResult GetArrayPropertiesBySN(string sn)
        {
            try
            {
                return Ok(_measurementService.GetArrayData(sn));
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }
        }

        [HttpPost("cooler")]
        public IActionResult SetCooler([FromBody] CoolerSettingsModel coolerSettings)
        {
            if (coolerSettings == null)
            {
                return BadRequest(ResponseMessages.Error("Empty cooler data frame"));
            }

            _logger.LogInformation($"Cooler information received: {JsonConvert.SerializeObject(coolerSettings)}");

            try
            {
                _measurementService.SetCooler(coolerSettings, true);
                return Ok("Cooler set successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }            
        }

        [HttpGet("cooler/all")]
        public IActionResult GetAllCooler()
        {
            try
            {
                var coolerData = _measurementService.GetAllCooler();
                return Ok(coolerData);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }
        }

        [HttpGet]
        [Route("cooler/{blockId}/{mooduleId}")]
        public IActionResult GetCooler(int blockId, int moduleId)
        {
            try
            {
                var coolerData = _measurementService.GetCooler(blockId, moduleId);
                return Ok(coolerData);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }
        }

        [HttpPost("pulser")]
        public IActionResult SetPulser([FromBody] PulserSettingsModel pulserSettings)
        {
            if (pulserSettings == null)
            {
                return BadRequest(ResponseMessages.Error("Empty pulser data frame"));
            }

            _logger.LogInformation($"Pulser information received: {JsonConvert.SerializeObject(pulserSettings)}");

            if (_measurementService.PulserState != SerialPortState.Connected)
            {
                return BadRequest(ResponseMessages.Error("Pulser is not connected"));
            }

            try
            {
                _measurementService.PulserReadingInterval = TimeSpan.FromSeconds(pulserSettings.RefreshInterval);
                return Ok("Pulser set successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }
        }

        [HttpGet("pulser")]
        public IActionResult GetPulserState()
        {
            // returns the SiPM states of the current run (waiting, mesured, analyzed)
            try
            {
                var data = new
                {
                    _measurementService.PulserState
                };
                string json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                return Ok(json);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }
        }

        [HttpGet("pulser/details")]
        public IActionResult GetPulserStateDetails()
        {
            // returns the SiPM states of the current run (waiting, mesured, analyzed)
            try
            {
                var data = new
                {
                    _measurementService.PulserState,
                    _measurementService.PulserReadingInterval.TotalSeconds,
                    _measurementService.Temperatures,
                    _measurementService.CoolerStates
                };
                string json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                return Ok(json);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }

        }

        [HttpGet("export/dir")]
        public IActionResult GetCurrentExportDir()
        {
            // returns the SiPM states of the current run (waiting, mesured, analyzed)
            try
            {
                ExportDirModel exportDir = new ExportDirModel(_measurementService.GetCurrentExportDir());
                string json = JsonConvert.SerializeObject(exportDir, Newtonsoft.Json.Formatting.Indented);
                return Ok(json);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }
        }

        [HttpPost("export/dir")]
        public IActionResult SetCurrentExportDir([FromBody] ExportDirModel dir)
        {
            if (dir == null)
            {
                return BadRequest(ResponseMessages.Error("Empty export directory data frame"));
            }

            _logger.LogInformation($"Export directory data received: {JsonConvert.SerializeObject(dir)}");

            try
            {
                if (_measurementService.SetDir(dir.ExportDir))
                {
                    return Ok("Export directory set successfully");
                }
                else
                {
                    return BadRequest(ResponseMessages.Error("Failed to set export directory"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }
        }

        [HttpPost("export/dir/list")]
        public IActionResult GetGivenDirList([FromBody] ExportDirModel dir)
        {
            if (dir == null)
            {
                return BadRequest(ResponseMessages.Error("Empty directory list data frame"));
            }

            _logger.LogInformation($"Directory list data received: {JsonConvert.SerializeObject(dir)}");

            try
            {
                List<string> list = _measurementService.GetListOfDirs(dir.ExportDir);
                string json = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented);
                return Ok(json);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }
        }

        [HttpPost("export")]
        public IActionResult ExportListedSiPMs([FromBody] ExportSiPMList list)
        {
            if (list == null)
            {
                return BadRequest(ResponseMessages.Error("Empty export list data frame"));
            }

            _logger.LogInformation($"Export list data received: {JsonConvert.SerializeObject(list)}");

            try
            {
                List<CurrentSiPMModel> skipped = _measurementService.ExportSiPMsData(list);
                return Ok(new
                {
                    SiPMs = skipped
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseMessages.Error(ex.Message));
            }
        }
    }
}


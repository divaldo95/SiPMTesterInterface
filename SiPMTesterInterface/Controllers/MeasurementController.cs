using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        public MeasurementController(ILogger<MeasurementController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_viewModel);
        }

        [HttpGet("state")]
        public IActionResult GetMeasurementStates()
        {
            var measurementStates = new
            {
                IV = _viewModel.IV.State,
                SPS = _viewModel.SPS.State
            };

            return Ok(measurementStates);
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using SeleniumBaseApi;
using SeleniumBaseApi.Services;
using SeleniumBaseApi.Models;
using System;
using System.Threading.Tasks;

namespace SeleniumBaseApi.Controllers;

[Route("[controller]")]
[ApiController]
public class BypassUrlController : ControllerBase
{
    public BypassUrlController()
    {
    }

    // normal getall request
    [HttpGet]
    public ActionResult<List<BypassUrl>> GetAll() =>
        BypassUrlService.GetAll();

    // normal get request
    [HttpGet("{id}")]
    public ActionResult<BypassUrl> Get(int id)
    {
        var bypassUrl = BypassUrlService.Get(id);

        if (bypassUrl == null)
            return NotFound();

        return bypassUrl;
    }

    // normal post request
    [HttpPost]
    public IActionResult Create(BypassUrl bypassUrl)
    {
        BypassUrlService.Add(bypassUrl);
        return CreatedAtAction(nameof(Get), new { id = bypassUrl.Id }, bypassUrl);
    }

    // normal update request
    [HttpPut("{id}")]
    public IActionResult Update(int id, BypassUrl bypassUrl)
    {
        if (id != bypassUrl.Id)
            return BadRequest();

        var exisstingBypassUrl = BypassUrlService.Get(id);
        if (exisstingBypassUrl is null)
            return NotFound();

        BypassUrlService.Update(bypassUrl);

        return NoContent();
    }

    // seleniumbase update request
    [HttpPut("updateHtml/{id}")]
    public async Task<IActionResult> UpdateHtml(int id, BypassUrl bypassUrl)
    {
        // checks for requests validity and returns the suitble error
        if (id != bypassUrl.Id)
            return BadRequest();

        var exisstingBypassUrl = BypassUrlService.Get(id);
        if (exisstingBypassUrl is null)
            return NotFound();

        // sends the url string to python
        string inputToPython = bypassUrl.Url;

        var (pythonOutput, pythonError, pythonExitCode) = await PythonRunner.RunPythonScriptAsync(inputToPython);

        // returns the"500 internal server error" status code along with the error massege from python.exe
        if (pythonExitCode != 0)
            return StatusCode(500, $"Python script failed to update HTML for ID {id}. Error: {pythonError}");
        
        // Updates the html source page
        exisstingBypassUrl.HtmlContent = pythonOutput;

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var bypassUrl = BypassUrlService.Get(id);

        if (bypassUrl is null)
            return NotFound();

        BypassUrlService.Delete(id);

        return NoContent();
    }
}
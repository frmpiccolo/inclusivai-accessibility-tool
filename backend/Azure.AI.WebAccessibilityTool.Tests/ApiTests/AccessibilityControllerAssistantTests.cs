using AzureAI.WebAccessibilityTool.API.Controllers;
using AzureAI.WebAccessibilityTool.API.Models;
using AzureAI.WebAccessibilityTool.Models;
using AzureAI.WebAccessibilityTool.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;

namespace AzureAI.WebAccessibilityTool.Tests.ApiTests;

/// <summary>
/// Test suite for the <see cref="AccessibilityController"/> class.
/// </summary>
public class AccessibilityControllerAssistantTests
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the test class and sets up mock configuration.
    /// </summary>
    public AccessibilityControllerAssistantTests()
    {
        // Arrange mock configuration
        var host = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile("appsettings.Development.json");
                    })
                    .Build();

        _configuration = host.Services.GetRequiredService<IConfiguration>();
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeImage"/> method with a valid URL.
    /// </summary>
    [Fact]
    public async Task AnalyzeImage_InvalidUrl_ReturnsOk()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);        

        // Act
        var result = await controller.AnalyzeImage(GlobalVariables.emptyUrl) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeImage"/> method with a valid URL.
    /// </summary>
    [Fact]
    public async Task AnalyzeImage_ValidUrl_ReturnsOk()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);        

        // Act
        var result = await controller.AnalyzeImage(GlobalVariables.testImageUrl) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        List<string> results = result.Value as List<string> ?? new List<string>();
        Assert.True(results?.Count > 0);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtmlWithAssistant"/> method with empty HTML content.
    /// </summary>    
    [Fact]
    public async Task AnalyzeHtmlWithAssistant_EmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);        

        // Act
        HtmlInput input = new HtmlInput { Html = GlobalVariables.emptyHtmlContent };
        var result = await controller.AnalyzeHtmlWithAssistant(input) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtmlWithAssistant"/> method with valid HTML content.
    /// </summary>    
    [Fact]
    public async Task AnalyzeHtmlWithAssistant_ValidContent_ReturnsNoAdvisory()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);

        // Act
        HtmlInput input = new HtmlInput { Html = GlobalVariables.validHtmlContent };
        var result = await controller.AnalyzeHtmlWithAssistant(input) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        AnalysisResult results = result.Value as AnalysisResult ?? new AnalysisResult { Items = new List<AnalysisItem>(), Explanation = "" };
        var lowOrImprovementsResult = results.Items.Where(e => !e.Severity.Equals("Low") && !e.Severity.Equals("Improvement")).ToList();
        Assert.True(lowOrImprovementsResult.Count == 0);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtmlWithAssistant"/> method with invalid HTML content.
    /// </summary>    
    [Fact]
    public async Task AnalyzeHtmlWithAssistant_InvalidContent_ReturnsAdvisory()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);

        // Act
        HtmlInput input = new HtmlInput { Html = GlobalVariables.invalidHtmlContent };
        var result = await controller.AnalyzeHtmlWithAssistant(input) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        AnalysisResult results = result.Value as AnalysisResult ?? new AnalysisResult { Items = new List<AnalysisItem>(), Explanation = "" };
        Assert.True(results.Items.Count > 0);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtmlFromUrlWithAssistant"/> method with a valid URL.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AnalyzeHtmlFromUrlWithAssistant_EmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);
        var urlInput = new UrlInput { Url = GlobalVariables.emptyUrl };

        // Act
        var result = await controller.AnalyzeHtmlFromUrlWithAssistant(urlInput) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtmlFromUrlWithAssistant"/> method with a valid URL.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AnalyzeHtmlFromUrlWithAssistant_ValidContent_ReturnsNoAdvisory()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);
        var urlInput = new UrlInput { Url = GlobalVariables.validUrl, GetImageDescriptions = true};

        // Act
        var result = await controller.AnalyzeHtmlFromUrlWithAssistant(urlInput) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        AnalysisResult results = result.Value as AnalysisResult ?? new AnalysisResult { Items = new List<AnalysisItem>(), Explanation = "" };
        var lowOrImprovementsResult = results.Items.Where(e => !e.Severity.Equals("Low") && !e.Severity.Equals("Improvement")).ToList();
        Assert.True(lowOrImprovementsResult.Count == 0);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtmlFromUrlWithAssistant"/> method with a valid URL.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AnalyzeHtmlFromUrlWithAssistant_InvalidContent_ReturnsAdvisory()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);
        var urlInput = new UrlInput { Url = GlobalVariables.invalidUrl, GetImageDescriptions = true};

        // Act
        // Act
        var result = await controller.AnalyzeHtmlFromUrlWithAssistant(urlInput) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        AnalysisResult results = result.Value as AnalysisResult ?? new AnalysisResult { Items = new List<AnalysisItem>(), Explanation = "" };
        Assert.True(results.Items.Count > 0);
    }
}

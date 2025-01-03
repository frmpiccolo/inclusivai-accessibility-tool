using System;
using Microsoft.Extensions.Configuration;
using AzureAI.WebAccessibilityTool.Services;
using Xunit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AzureAI.WebAccessibilityTool.Models;

namespace AzureAI.WebAccessibilityTool.Tests.BusinessTests
{
    /// <summary>
    /// Test suite for the <see cref="AccessibilityAnalyzer"/> class.
    /// </summary>
    public class AccessibilityAnalyzerAssistantTests
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the test class, setting up configuration for tests.
        /// </summary>
        public AccessibilityAnalyzerAssistantTests()
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
        /// Tests that the <see cref="AccessibilityAnalyzer.AnalyzeImage(string)"/> method
        /// returns expected results when provided with a valid image URL.
        /// </summary>
        [Fact]
        public async Task AnalyzeImage_ValidUrl_ReturnsExpectedResults()
        {
            // Arrange
            var analyzer = new AccessibilityAnalyzer(_configuration);            

            // Act
            var results = await analyzer.AnalyzeImageAsync(GlobalVariables.testImageUrl);

            // Assert
            Assert.NotNull(results); // Ensure the result is not null.            
            Assert.True(results.Count > 0); 
        }

        /// <summary>
        /// Tests that the <see cref="AccessibilityAnalyzer.AnalyzeHtml(string)"/> method
        /// throws an <see cref="ArgumentException"/> when provided with empty HTML content.
        /// </summary>
        [Fact]
        public async Task AnalyzeHtml_EmptyContent_ThrowsException()
        {
            // Arrange
            var analyzer = new AccessibilityAnalyzer(_configuration);
            var emptyHtmlContent = string.Empty;

            // Act & Assert
            AnalysisInput analysisInput = new AnalysisInput()
            {
                Type = AnalysisType.HTML,
                Content = string.Empty,
            };

            await Assert.ThrowsAsync<ArgumentException>(() => analyzer.AnalyzeWithAssistantAsync(analysisInput));
        }

        /// <summary>
        /// Tests that the <see cref="AccessibilityAnalyzer.AnalyzeHtml(string)"/> method
        /// returns expected results with 0 issues when provided with HTML content without accessibility issues.
        /// </summary>
        [Fact]
        public async Task AnalyzeHtml_ValidContent_ReturnsNoAdvisory()
        {
            // Arrange
            var analyzer = new AccessibilityAnalyzer(_configuration);

            // Act
            AnalysisInput analysisInput = new AnalysisInput()
            {
                Type = AnalysisType.HTML,
                Content = string.Empty,
            };

            var result = await analyzer.AnalyzeWithAssistantAsync(analysisInput);

            // Assert            
            Assert.NotNull(result);
            var lowOrImprovementsResult = result.Items.Where(e => !e.Severity.Equals("Low") && !e.Severity.Equals("Improvement")).ToList();
            Assert.True(lowOrImprovementsResult.Count == 0);
        }

        /// <summary>
        /// Tests that the <see cref="AccessibilityAnalyzer.AnalyzeHtml(string)"/> method
        /// returns expected results with 1 or more issues when provided with HTML content with accessibility issues.
        /// </summary>        
        [Fact]
        public async Task AnalyzeHtml_InvalidContent_ReturnsAdvisory()
        {
            // Arrange
            var analyzer = new AccessibilityAnalyzer(_configuration);

            // Act
            AnalysisInput analysisInput = new AnalysisInput()
            {
                Type = AnalysisType.HTML,
                Content = GlobalVariables.invalidHtmlContent,
            };

            var result = await analyzer.AnalyzeWithAssistantAsync(analysisInput);

            // Assert            
            Assert.NotNull(result);
            Assert.True(result.Items.Count > 0); 
        }
    }
}

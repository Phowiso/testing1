using System.Text.Json;
using BudgetApi.Controllers;
using BudgetApi.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BudgetApi.Tests;

public class BudgetControllerTests
{
    [Fact]
    public void Calculate_WithoutInsurance_ReturnsOkAnd500()
    {
        var controller = new BudgetController();
        var request = new BudgetRequest
        {
            DailyCost = 100m, Days = 5, IncludeInsurance = false
        };

        var result = controller.Calculate(request);

        AssertTotal(result, 500m);
    }

    [Fact]
    public void Calculate_WithInsurance_AddsFivePercent()
    {
        var controller = new BudgetController();
        var request = new BudgetRequest
        {
            DailyCost = 100m, Days = 5, IncludeInsurance = true
        };

        var result = controller.Calculate(request);

        AssertTotal(result, 525m);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Calculate_NonPositiveDays_ReturnsBadRequest(int days)
    {
        var controller = new BudgetController();
        var request = new BudgetRequest { DailyCost = 100m, Days = days };

        var result = controller.Calculate(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.Equal("Количество дней должно быть больше нуля.", badRequest.Value);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Calculate_ZeroDailyCost_ReturnsOkAndZero(bool insurance)
    {
        var controller = new BudgetController();
        var request = new BudgetRequest
        {
            DailyCost = 0m, Days = 5, IncludeInsurance = insurance
        };

        var result = controller.Calculate(request);

        AssertTotal(result, 0m);
    }

    [Fact]
    public void Calculate_FractionalDailyCost_PreservesKopecks()
    {
        var controller = new BudgetController();
        var request = new BudgetRequest
        {
            DailyCost = 100.50m, Days = 3, IncludeInsurance = false
        };

        var result = controller.Calculate(request);

        AssertTotal(result, 301.50m);
    }

    private static void AssertTotal(IActionResult result, decimal expected)
    {
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.NotNull(ok.Value);
        var json = JsonSerializer.SerializeToElement(ok.Value);
        Assert.Equal(expected, json.GetProperty("TotalAmount").GetDecimal());
    }
}

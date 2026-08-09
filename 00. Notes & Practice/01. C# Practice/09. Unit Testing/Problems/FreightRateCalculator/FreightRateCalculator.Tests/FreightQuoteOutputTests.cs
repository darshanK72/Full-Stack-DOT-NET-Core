using System;
using System.Collections.Generic;
using LogisticsPricing;
using Xunit;
using Xunit.Abstractions;

namespace FreightRateCalculator.Tests;

public sealed class FreightQuoteOutputTests
{
    private readonly FreightQuoteService _service = new FreightQuoteService();
    private readonly ITestOutputHelper _output;

    public FreightQuoteOutputTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void QualifiesForBulkDiscount_LogsSubtotal()
    {
        // TODO: build lines with subtotal >= 500; WriteLine subtotal; Assert.True bulk eligible
        throw new NotImplementedException();
    }
}

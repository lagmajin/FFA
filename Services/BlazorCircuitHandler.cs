using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

namespace FFA.Services;

// Logs circuit lifecycle events to help diagnose SignalR/circuit issues.
// This is intended for development and lightweight diagnostics. It
// does not change application behavior.
public class BlazorCircuitHandler : CircuitHandler
{
    private readonly ILogger<BlazorCircuitHandler> _logger;
    private readonly StartupLogger? _startupLogger;

    public BlazorCircuitHandler(ILogger<BlazorCircuitHandler> logger, StartupLogger? startupLogger = null)
    {
        _logger = logger;
        _startupLogger = startupLogger;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Circuit opened: {CircuitId}", circuit.Id);
            try { _startupLogger?.LogInfo($"Circuit opened: {circuit.Id}"); } catch { }
        }
        catch { }
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Circuit closed: {CircuitId}", circuit.Id);
            try { _startupLogger?.LogInfo($"Circuit closed: {circuit.Id}"); } catch { }
        }
        catch { }
        return Task.CompletedTask;
    }
}

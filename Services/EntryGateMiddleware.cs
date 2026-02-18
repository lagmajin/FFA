using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FFA.Services
{
    /// <summary>
    /// EntryGate middleware: when enabled, requires a short-lived cookie flag (VisitedEntry)
    /// to allow access to most pages. Home (/), /login and /register are always allowed.
    /// This is the "entry-page flag" approach requested by the user. It is not enabled
    /// by default; enable it in Program.cs by registering EntryGateOptions and calling UseEntryGate().
    /// </summary>
    public class EntryGateOptions
    {
        public string CookieName { get; set; } = "VisitedEntry";
        public string[] AllowedPrefixes { get; set; } = new[] { "/", "/index.html", "/login", "/register", "/signin", "/signout", "/css/", "/js/", "/_framework/", "/favicon.ico" };
        public string RedirectPath { get; set; } = "/";
    }

    public class EntryGateMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly EntryGateOptions _opts;

        public EntryGateMiddleware(RequestDelegate next, EntryGateOptions opts)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _opts = opts ?? new EntryGateOptions();
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Allow static and allowed prefixes
            if (_opts.AllowedPrefixes.Any(p => path.Equals(p, StringComparison.OrdinalIgnoreCase) || path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Allow if cookie flag present
            if (context.Request.Cookies.TryGetValue(_opts.CookieName, out var v) && v == "1")
            {
                await _next(context);
                return;
            }

            // Otherwise redirect to entry page
            context.Response.Redirect(_opts.RedirectPath);
        }
    }

    public static class EntryGateExtensions
    {
        public static IApplicationBuilder UseEntryGate(this IApplicationBuilder app)
        {
            // Resolve options from DI if available, otherwise use defaults
            var opts = app.ApplicationServices.GetService(typeof(EntryGateOptions)) as EntryGateOptions ?? new EntryGateOptions();
            return app.UseMiddleware<EntryGateMiddleware>(opts);
        }
    }
}

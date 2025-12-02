using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ERPAccounting.Common.Interfaces;
using ERPAccounting.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ERPAccounting.Infrastructure.Middleware
{
    /// <summary>
    /// Middleware za automatsko logovanje svih API poziva sa JSON snapshot podrškom.
    /// Hvata request/response i čuva u tblAPIAuditLog tabelu.
    /// 
    /// NOVI PRISTUP SA HttpContext.Items:
    /// - Koristi HttpContext.Items za deljenje audit log ID-a
    /// - Svi DbContext instance-i mogu da pročitaju audit log ID iz HttpContext-a
    /// - Rešava problem različitih DI scope-ova
    /// - Hvata RequestBody i ResponseBody za SVE HTTP metode
    /// </summary>
    public class ApiAuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiAuditMiddleware> _logger;
        
        // Ključ za čuvanje audit log ID-a u HttpContext.Items
        public const string AuditLogIdKey = "__AuditLogId__";

        // Maximum body size to log (10 MB)
        private const int MaxBodySize = 10 * 1024 * 1024;

        public ApiAuditMiddleware(RequestDelegate next, ILogger<ApiAuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService)
        {
            var request = context.Request;
            var originalBodyStream = context.Response.Body;

            // 1. Pripremi audit log pre izvršavanja requesta
            var auditLog = new ApiAuditLog
            {
                Timestamp = DateTime.UtcNow,
                HttpMethod = request.Method,
                Endpoint = request.Path.Value ?? string.Empty,
                RequestPath = request.Path.Value ?? string.Empty,
                QueryString = request.QueryString.HasValue ? request.QueryString.Value : null,
                IPAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = request.Headers["User-Agent"].ToString(),
                UserId = currentUserService.UserId,
                Username = currentUserService.Username,
                CorrelationId = Guid.NewGuid()
            };

            // 2. 🔧 FIXED: Properly read request body
            auditLog.RequestBody = await ReadRequestBodyAsync(request);

            // 3. Privremeni stream za response
            var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            var stopwatch = Stopwatch.StartNew();
            int auditLogId = 0;

            try
            {
                // 4. Kreiraj audit log I DOBIJ ID
                await auditLogService.LogAsync(auditLog);
                auditLogId = auditLog.IDAuditLog;

                // 5. KRITIČNO: Postavi audit log ID u HttpContext.Items
                // Ovaj ID će biti dostupan svim DbContext instance-ima kroz HttpContextAccessor
                if (auditLogId > 0)
                {
                    context.Items[AuditLogIdKey] = auditLogId;
                    
                    _logger.LogDebug(
                        "Audit log ID {AuditLogId} set in HttpContext for {Method} {Endpoint}",
                        auditLogId,
                        request.Method,
                        request.Path);
                }

                // 6. Izvrši request pipeline (ovde se događa SaveChangesAsync sa audit tracking-om)
                await _next(context);

                stopwatch.Stop();

                // 7. Nakon uspešnog izvršavanja
                auditLog.ResponseStatusCode = context.Response.StatusCode;
                auditLog.IsSuccess = context.Response.StatusCode >= 200 && context.Response.StatusCode < 400;
                auditLog.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;

                // 8. 🔧 FIXED: Properly read response body
                auditLog.ResponseBody = await ReadResponseBodyAsync(responseBodyStream);

                // 9. Ažuriraj audit log sa response podacima
                await auditLogService.UpdateAsync(auditLog);

                // 10. Vrati response u originalni stream
                await CopyResponseBodyAsync(responseBodyStream, originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // 11. U slučaju exceptiona, upiši detalje
                auditLog.ResponseStatusCode = context.Response.StatusCode > 0 
                    ? context.Response.StatusCode 
                    : StatusCodes.Status500InternalServerError;
                auditLog.IsSuccess = false;
                auditLog.ErrorMessage = ex.Message;
                auditLog.ExceptionDetails = ex.ToString();
                auditLog.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;

                // Pokuša da pročita response body i za error
                auditLog.ResponseBody = await ReadResponseBodyAsync(responseBodyStream);

                try
                {
                    await auditLogService.UpdateAsync(auditLog);
                }
                catch (Exception auditEx)
                {
                    // Ne dozvoljavamo da audit failure crašuje aplikaciju
                    _logger.LogError(auditEx, "Failed to update audit entry for failed request");
                }

                // 12. Vrati response body u originalni stream ako ima nečega
                await CopyResponseBodyAsync(responseBodyStream, originalBodyStream);

                // Re-throw originalni exception
                throw;
            }
            finally
            {
                // 13. Uvek vrati originalni stream i očisti privremeni
                context.Response.Body = originalBodyStream;
                await responseBodyStream.DisposeAsync();
            }
        }

        /// <summary>
        /// Reads request body from stream without consuming it
        /// </summary>
        private async Task<string?> ReadRequestBodyAsync(HttpRequest request)
        {
            // Skip if no content or OPTIONS request
            if (request.ContentLength == null || request.ContentLength == 0)
            {
                return null;
            }

            if (request.Method == "OPTIONS")
            {
                return null;
            }

            // Skip large bodies
            if (request.ContentLength > MaxBodySize)
            {
                return $"[Body too large: {request.ContentLength} bytes]";
            }

            try
            {
                // Enable buffering so we can read the body multiple times
                request.EnableBuffering();

                // Ensure we're at the start
                if (request.Body.CanSeek)
                {
                    request.Body.Position = 0;
                }

                // Read the body
                using (var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 4096,
                    leaveOpen: true))
                {
                    var body = await reader.ReadToEndAsync();

                    // Reset position for the next middleware
                    if (request.Body.CanSeek)
                    {
                        request.Body.Position = 0;
                    }

                    return string.IsNullOrWhiteSpace(body) ? null : body;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read request body");
                return $"[Error reading body: {ex.Message}]";
            }
        }

        /// <summary>
        /// Reads response body from memory stream
        /// </summary>
        private async Task<string?> ReadResponseBodyAsync(MemoryStream responseBodyStream)
        {
            if (responseBodyStream.Length == 0)
            {
                return null;
            }

            // Skip large responses
            if (responseBodyStream.Length > MaxBodySize)
            {
                return $"[Body too large: {responseBodyStream.Length} bytes]";
            }

            try
            {
                responseBodyStream.Position = 0;
                
                using (var reader = new StreamReader(
                    responseBodyStream,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 4096,
                    leaveOpen: true))
                {
                    var body = await reader.ReadToEndAsync();
                    responseBodyStream.Position = 0; // Reset for copying
                    return string.IsNullOrWhiteSpace(body) ? null : body;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read response body");
                return $"[Error reading body: {ex.Message}]";
            }
        }

        /// <summary>
        /// Copies response body from temporary stream to original stream
        /// </summary>
        private async Task CopyResponseBodyAsync(MemoryStream source, Stream destination)
        {
            if (source.Length > 0)
            {
                source.Position = 0;
                await source.CopyToAsync(destination);
                await destination.FlushAsync();
            }
        }
    }
}

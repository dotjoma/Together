using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using Microsoft.Extensions.Logging;

namespace Together.Infrastructure.Services;

/// <summary>
/// Service for configuring TLS 1.2+ enforcement for secure connections
/// </summary>
public static class TlsConfigurationService
{
    private static bool _isConfigured = false;
    private static readonly object _lock = new object();

    /// <summary>
    /// Configures the application to use TLS 1.2 or higher for all connections
    /// </summary>
    public static void ConfigureTls(ILogger? logger = null)
    {
        lock (_lock)
        {
            if (_isConfigured)
            {
                logger?.LogInformation("TLS configuration already applied");
                return;
            }

            try
            {
                // Set minimum TLS version to 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                // Configure certificate validation
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

                // Set connection limits
                ServicePointManager.DefaultConnectionLimit = 10;
                ServicePointManager.MaxServicePointIdleTime = 30000; // 30 seconds

                // Enable expect 100-continue for better performance
                ServicePointManager.Expect100Continue = true;

                _isConfigured = true;
                logger?.LogInformation("TLS 1.2+ enforcement configured successfully");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to configure TLS settings");
                throw;
            }
        }
    }

    /// <summary>
    /// Validates server certificates for secure connections
    /// </summary>
    private static bool ValidateServerCertificate(
        object sender,
        System.Security.Cryptography.X509Certificates.X509Certificate? certificate,
        System.Security.Cryptography.X509Certificates.X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // In production, always validate certificates
        #if DEBUG
        // In development, you might want to allow self-signed certificates
        // But still log the policy errors
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            Console.WriteLine($"Certificate validation warning: {sslPolicyErrors}");
        }
        return true;
        #else
        // In production, only accept valid certificates
        return sslPolicyErrors == SslPolicyErrors.None;
        #endif
    }

    /// <summary>
    /// Gets the configured TLS protocols
    /// </summary>
    public static SecurityProtocolType GetConfiguredProtocols()
    {
        return ServicePointManager.SecurityProtocol;
    }

    /// <summary>
    /// Checks if TLS 1.2 or higher is enabled
    /// </summary>
    public static bool IsTls12OrHigherEnabled()
    {
        var protocols = ServicePointManager.SecurityProtocol;
        return (protocols & SecurityProtocolType.Tls12) == SecurityProtocolType.Tls12 ||
               (protocols & SecurityProtocolType.Tls13) == SecurityProtocolType.Tls13;
    }
}

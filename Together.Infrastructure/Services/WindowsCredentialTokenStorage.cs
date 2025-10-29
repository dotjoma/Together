using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Together.Application.Interfaces;

namespace Together.Infrastructure.Services;

/// <summary>
/// Secure token storage implementation using Windows Credential Manager
/// </summary>
public class WindowsCredentialTokenStorage : ISecureTokenStorage
{
    private readonly ILogger<WindowsCredentialTokenStorage> _logger;
    private const string TargetNamePrefix = "Together_";

    public WindowsCredentialTokenStorage(ILogger<WindowsCredentialTokenStorage> logger)
    {
        _logger = logger;
    }

    public Task<bool> StoreTokenAsync(string key, string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Attempted to store token with null or empty key/token");
                return Task.FromResult(false);
            }

            var targetName = GetTargetName(key);
            var credential = new CREDENTIAL
            {
                Type = CRED_TYPE.GENERIC,
                TargetName = targetName,
                CredentialBlob = Marshal.StringToCoTaskMemUni(token),
                CredentialBlobSize = (uint)(token.Length * 2), // Unicode = 2 bytes per char
                Persist = CRED_PERSIST.LOCAL_MACHINE,
                AttributeCount = 0,
                UserName = Environment.UserName
            };

            var result = CredWrite(ref credential, 0);
            
            // Free the credential blob memory
            if (credential.CredentialBlob != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(credential.CredentialBlob);
            }

            if (result)
            {
                _logger.LogInformation("Token stored successfully for key: {Key}", key);
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                _logger.LogError("Failed to store token for key: {Key}, Error: {Error}", key, error);
            }

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while storing token for key: {Key}", key);
            return Task.FromResult(false);
        }
    }

    public Task<string?> RetrieveTokenAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Attempted to retrieve token with null or empty key");
                return Task.FromResult<string?>(null);
            }

            var targetName = GetTargetName(key);
            var result = CredRead(targetName, CRED_TYPE.GENERIC, 0, out var credPtr);

            if (!result)
            {
                var error = Marshal.GetLastWin32Error();
                if (error != ERROR_NOT_FOUND)
                {
                    _logger.LogWarning("Failed to retrieve token for key: {Key}, Error: {Error}", key, error);
                }
                return Task.FromResult<string?>(null);
            }

            try
            {
                var credential = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                var token = Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);
                
                _logger.LogInformation("Token retrieved successfully for key: {Key}", key);
                return Task.FromResult<string?>(token);
            }
            finally
            {
                CredFree(credPtr);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while retrieving token for key: {Key}", key);
            return Task.FromResult<string?>(null);
        }
    }

    public Task<bool> DeleteTokenAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Attempted to delete token with null or empty key");
                return Task.FromResult(false);
            }

            var targetName = GetTargetName(key);
            var result = CredDelete(targetName, CRED_TYPE.GENERIC, 0);

            if (result)
            {
                _logger.LogInformation("Token deleted successfully for key: {Key}", key);
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                if (error != ERROR_NOT_FOUND)
                {
                    _logger.LogWarning("Failed to delete token for key: {Key}, Error: {Error}", key, error);
                }
            }

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while deleting token for key: {Key}", key);
            return Task.FromResult(false);
        }
    }

    public Task<bool> TokenExistsAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Task.FromResult(false);
            }

            var targetName = GetTargetName(key);
            var result = CredRead(targetName, CRED_TYPE.GENERIC, 0, out var credPtr);

            if (result)
            {
                CredFree(credPtr);
            }

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while checking token existence for key: {Key}", key);
            return Task.FromResult(false);
        }
    }

    private static string GetTargetName(string key)
    {
        return $"{TargetNamePrefix}{key}";
    }

    #region Windows Credential Manager P/Invoke

    private const int ERROR_NOT_FOUND = 1168;

    [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

    [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDelete(string target, CRED_TYPE type, int flags);

    [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
    private static extern void CredFree([In] IntPtr cred);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public uint Flags;
        public CRED_TYPE Type;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string TargetName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public CRED_PERSIST Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string TargetAlias;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string UserName;
    }

    private enum CRED_TYPE : uint
    {
        GENERIC = 1,
        DOMAIN_PASSWORD = 2,
        DOMAIN_CERTIFICATE = 3,
        DOMAIN_VISIBLE_PASSWORD = 4,
        GENERIC_CERTIFICATE = 5,
        DOMAIN_EXTENDED = 6,
        MAXIMUM = 7,
        MAXIMUM_EX = 1007
    }

    private enum CRED_PERSIST : uint
    {
        SESSION = 1,
        LOCAL_MACHINE = 2,
        ENTERPRISE = 3
    }

    #endregion
}

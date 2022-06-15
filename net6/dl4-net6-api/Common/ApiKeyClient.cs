using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace dl4_net6_api.Common
{
    public class ApiKeyClient
    {
        private const string _dateFormat = "yyyyMMdd";

        private readonly string _basePath;
        private readonly string _apiClientId;
        private readonly string _apiClientKey;
        private readonly string _apiDealerId;
        private readonly string _apiUsername;
        private readonly string _apiKeyName;
        private readonly string _apiKeyValue;
        private readonly byte[] _initialKey;

        public ApiKeyClient(string basePath, string apiClientId, string apiClientKey, string apiDealerId, string apiUsername, string apiKeyName, string apiKeyValue)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _apiClientId = apiClientId ?? throw new ArgumentNullException(nameof(apiClientId));
            _apiClientKey = apiClientKey ?? throw new ArgumentNullException(nameof(apiClientKey));
            _apiDealerId = apiDealerId ?? throw new ArgumentNullException(nameof(apiDealerId));
            _apiUsername = apiUsername ?? throw new ArgumentNullException(nameof(apiUsername));
            _apiKeyName = apiKeyName ?? throw new ArgumentNullException(nameof(apiKeyName));
            _apiKeyValue = apiKeyValue ?? throw new ArgumentNullException(nameof(apiKeyValue));
            _initialKey = Encoding.UTF8.GetBytes(apiClientKey);
        }

        /// <summary>
        /// Generate the signing key. 
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        private SigningKey GenerateSigningKey(DateTimeOffset timestamp)
        {
            SigningKey signingKey = new SigningKey();
            List<string> credentialList = new()
            {
                timestamp.ToString(_dateFormat),
                "external",
                _apiClientId,
                _apiUsername,
                _apiKeyName
            };
            if (!string.IsNullOrEmpty(_apiDealerId))
            {
                credentialList = new()
                {
                    timestamp.ToString(_dateFormat),
                    "external",
                    _apiClientId,
                    _apiDealerId,
                    _apiUsername,
                    _apiKeyName
                };
            }

            signingKey.Key = _initialKey;

            string separator = "";
            foreach (var credential in credentialList)
            {
                var hmac = new HMACSHA256(signingKey.Key);
                signingKey.Key = hmac.ComputeHash(Encoding.UTF8.GetBytes(credential));
                signingKey.Credentials += separator + credential;
                separator = "/";
            }

            return signingKey;
        }

        /// <summary>
        /// Adds the TCI relevant headers to the HTTP request.
        /// TCIv1-HmacSHA256 Credential=yyyyMMdd/external/clientId/userId/keyName, SignedEntities=METHOD;PATH;x-tci-timestamp, Signature=&lt;signatureInHex&gt;
        /// also adds a custom x-tci-timestamp header that is set to current time in milliseconds since Unix epoch time
        /// </summary>
        /// <param name="request"></param>
        /// <param name="path"></param>
        public void AddAuthHeaderToRequest(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.RequestUri == null)
            {
                throw new NullReferenceException("RequestUri cannot be null.");
            }

            string path = request.RequestUri.AbsolutePath;
            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }

            DateTimeOffset timestamp = DateTime.UtcNow;

            long ts = timestamp.ToUnixTimeMilliseconds();

            SigningKey signingKey = GenerateSigningKey(timestamp);

            Dictionary<string, string> toSignDict = new()
            {
                { "METHOD", request.Method.Method },
                { "PATH", path },
                { "x-tci-timestamp", ts.ToString() }
            };

            string toSign = "";
            string toSignKeys = "";
            string separator = "";

            foreach (var entry in toSignDict)
            {
                toSignKeys += separator + entry.Key;
                toSign += separator + entry.Value;
                separator = ";";
            }
            byte[] toSignBytes = Encoding.UTF8.GetBytes(toSign);

            HMACSHA256 hmac = new HMACSHA256(signingKey.Key);
            byte[] signature = hmac.ComputeHash(toSignBytes);

            string authHeader = $"Credential={signingKey.Credentials},";
            authHeader += $" SignedEntities={toSignKeys},";
            authHeader += $" Signature={Convert.ToHexString(signature).ToLower()}";
            
            request.Headers.Authorization = new AuthenticationHeaderValue("TCIv1-HmacSHA256", authHeader);
            request.Headers.Add("x-tci-timestamp", ts.ToString());
            request.Headers.Add("x-api-key", _apiKeyValue);
        }

        private class SigningKey
        {
            public string Credentials { get; set; } = default!;
            public byte[] Key { get; set; } = default!;
        }
    }
}

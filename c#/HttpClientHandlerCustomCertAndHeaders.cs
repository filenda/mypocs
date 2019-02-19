public class CustomHttpClientHandler : HttpClientHandler
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string bucketName = Constants.AWSBucket;
    private string certFileName = Constants.AWSCertifileName;

    public CustomHttpClientHandler(string clientId, string clientSecret)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        ClientCertificateOptions = ClientCertificateOption.Manual;
        SslProtocols = System.Security.Authentication.SslProtocols.None;

        byte[] certFile;

        //  Reads binary certificate file from aws s3 and attaches it to the request
        var s3Client = new AmazonS3Client(RegionEndpoint.USEast1);

        GetObjectRequest certRequest = new GetObjectRequest()
        {
            BucketName = bucketName,
            Key = certFileName
        };

        using (GetObjectResponse response = s3Client.GetObjectAsync(certRequest).Result)
        {
            using (Stream responseStream = response.ResponseStream)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    responseStream.CopyTo(ms);
                    certFile = ms.ToArray();
                }
            }
        }

        var cert = new X509Certificate2(certFile, "certPass", X509KeyStorageFlags.DefaultKeySet);

        ClientCertificates.Add(cert);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        //  You can add custom headers here for e.g

        string token = new TokenService(_clientId, _clientSecret).GetTokenAsync();

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.TryAddWithoutValidation("x-custom-header-name", "value-of-custom-header");

        return await base.SendAsync(request, cancellationToken);
    }
}
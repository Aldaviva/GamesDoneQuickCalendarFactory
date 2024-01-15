namespace Tests.Helpers;

public abstract class FakeHttpMessageHandler: HttpMessageHandler {

    internal static readonly HttpRequestOptionsKey<Stream> REQUEST_BODY_STREAM = new(nameof(REQUEST_BODY_STREAM));

    protected sealed override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
        if (request.Content != null) {
            // the request body is disposed after sending the request, before we can assert any of its properties in the test, so make a copy of the body
            MemoryStream requestBodyCopy = new();
            request.Content.CopyToAsync(requestBodyCopy, cancellationToken);
            request.Options.Set(REQUEST_BODY_STREAM, requestBodyCopy);
            requestBodyCopy.Seek(0, SeekOrigin.Begin);
        }

        return SendAsync(request);
    }

    // ReSharper disable once InconsistentNaming - named after an existing method that isn't mine
    public abstract Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);

}
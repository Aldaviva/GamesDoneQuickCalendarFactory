namespace Tests.Helpers;

internal class HttpRequestMessageFormatter: ArgumentValueFormatter<HttpRequestMessage> {

    protected override string GetStringValue(HttpRequestMessage request) {
        return $"<{request.Method} {request.RequestUri}{(request.Headers.Any() ? "\n" + request.Headers : "")}\n" +
            (request.Content?.ReadAsStringAsync().GetAwaiter().GetResult() ?? "") + ">";
    }

}
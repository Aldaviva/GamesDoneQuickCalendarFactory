using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

// ReSharper disable InconsistentNaming - named after methods that aren't mine

namespace Tests.Helpers;

public static class TestExtensions {

    public static HttpRequestMessage Matches(this IArgumentConstraintManager<HttpRequestMessage> manager, HttpMethod expectedVerb, string expectedUri) {
        return manager.Matches(actual => IsMatch(actual, expectedVerb, expectedUri), writer => {
            writer.Write(expectedVerb);
            writer.Write(" ");
            writer.Write(expectedUri);
        });
    }

    public static HttpRequestMessage Matches(this IArgumentConstraintManager<HttpRequestMessage> manager,
                                             HttpMethod expectedVerb,
                                             string expectedUri,
                                             [LanguageInjection(InjectedLanguage.JSON)] string? expectedJsonBody) {
        return manager.Matches(actual => IsMatch(actual, expectedVerb, expectedUri, expectedJsonBody), writer => {
            writer.Write(expectedVerb);
            writer.Write(" ");
            writer.Write(expectedUri);
            if (expectedJsonBody != null) {
                writer.Write("\n");
                writer.Write(expectedJsonBody);
            }
        });
    }

    public static bool IsMatch(HttpRequestMessage actual, HttpMethod expectedMethod, string expectedUri) {
        return actual.Method == expectedMethod && actual.RequestUri?.ToString() == expectedUri;
    }

    public static bool IsMatch(HttpRequestMessage actual, HttpMethod expectedMethod, string expectedUri, [LanguageInjection(InjectedLanguage.JSON)] string? expectedJsonBody) {
        return IsMatch(actual, expectedMethod, expectedUri) &&
            (actual.Options.TryGetValue(FakeHttpMessageHandler.REQUEST_BODY_STREAM, out Stream? actualBody)
                ? expectedJsonBody != null && JToken.DeepEquals(JToken.ReadFrom(new JsonTextReader(new StreamReader(actualBody, Encoding.UTF8))), JObject.Parse(expectedJsonBody))
                : expectedJsonBody == null);
    }

}
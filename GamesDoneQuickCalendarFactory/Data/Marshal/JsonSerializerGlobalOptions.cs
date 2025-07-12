using System.Text.Json;
using System.Text.Json.Serialization;

namespace GamesDoneQuickCalendarFactory.Data.Marshal;

public static class JsonSerializerGlobalOptions {

    public static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new(JsonSerializerDefaults.Web) {
        Converters = {
            EmptyToNullUriConverter.INSTANCE,
            OffsetDateTimeConverter.INSTANCE,
            DurationConverter.INSTANCE,
            new JsonStringEnumConverter()
        }
    };

}
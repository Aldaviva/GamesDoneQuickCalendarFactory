using GamesDoneQuickCalendarFactory.Data.Marshal;
using NodaTime;
using System.Text.Json;

namespace Tests;

public class MarshalTests {

    [Theory]
    [MemberData(nameof(DESERIALIZE_OFFSET_DATE_TIME_DATA))]
    public void deserializeOffsetDateTime(string json, OffsetDateTime expected) {
        JsonSerializerOptions options = new() { Converters = { new OffsetDateTimeConverter() } };

        OffsetDateTime actual = JsonSerializer.Deserialize<OffsetDateTime>(json, options);

        actual.Should().Be(expected);
    }

    public static readonly TheoryData<string, OffsetDateTime> DESERIALIZE_OFFSET_DATE_TIME_DATA = new() {
        { "\"2024-01-29T14:08:10-08:00\"", new LocalDateTime(2024, 1, 29, 14, 8, 10).WithOffset(Offset.FromHours(-8)) },
        { "\"2024-01-29T14:08:10-08\"", new LocalDateTime(2024, 1, 29, 14, 8, 10).WithOffset(Offset.FromHours(-8)) }
    };

    [Fact]
    public void deserializeOffsetDateTimeConverterErrors() {
        JsonSerializerOptions options = new() { Converters = { new OffsetDateTimeConverter() } };

        Action thrower = () => JsonSerializer.Deserialize<OffsetDateTime>("\"123\"", options);
        thrower.Should().Throw<FormatException>();

        thrower = () => JsonSerializer.Deserialize<OffsetDateTime>("123", options);
        thrower.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void serializeOffsetDateTime() {
        JsonSerializerOptions options = new() { Converters = { new OffsetDateTimeConverter() } };

        string actual = JsonSerializer.Serialize(new LocalDateTime(2024, 1, 29, 14, 8, 10).WithOffset(Offset.FromHours(-8)), options);
        actual.Should().Be("\"2024-01-29T14:08:10-08\"");
    }

    [Theory]
    [MemberData(nameof(DESERIALIZE_PERIOD_DATA))]
    public void deserializePeriod(string json, Period expected) {
        JsonSerializerOptions options = new() { Converters = { new PeriodConverter() } };

        Period? actual = JsonSerializer.Deserialize<Period>(json, options);

        actual.Should().Be(expected);
    }

    public static readonly TheoryData<string, Period> DESERIALIZE_PERIOD_DATA = new() {
        { "\"0\"", Period.Zero },
        { "\"0:0:0\"", Period.Zero },
        { "\"0:0:1\"", Period.FromSeconds(1) },
        { "\"0:1:2\"", Period.FromMinutes(1) + Period.FromSeconds(2) },
        { "\"1:2\"", Period.FromMinutes(1) + Period.FromSeconds(2) },
        { "\"1:2:3\"", Period.FromHours(1) + Period.FromMinutes(2) + Period.FromSeconds(3) },
        { "\"01:02:03\"", Period.FromHours(1) + Period.FromMinutes(2) + Period.FromSeconds(3) },
    };

    [Fact]
    public void deserializePeriodErrors() {
        JsonSerializerOptions options = new() { Converters = { new PeriodConverter() } };

        ((Action) (() => JsonSerializer.Deserialize<Period>("\"1:2:3:4\"", options))).Should().Throw<FormatException>();
        ((Action) (() => JsonSerializer.Deserialize<Period>("\"0:0:a\"", options))).Should().Throw<FormatException>();
        ((Action) (() => JsonSerializer.Deserialize<Period>("123", options))).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void serializePeriod() {
        JsonSerializerOptions options = new() { Converters = { new PeriodConverter() } };

        string actual = JsonSerializer.Serialize(Period.FromHours(1) + Period.FromMinutes(2) + Period.FromSeconds(3), options);
        actual.Should().Be("\"1:2:3\"");

        JsonSerializer.Serialize<Period?>(null, options).Should().Be("null");
    }

    [Theory]
    [MemberData(nameof(DESERIALIZE_URI_DATA))]
    public void deserializeUri(string json, Uri? expected) {
        JsonSerializerOptions options = new() { Converters = { new EmptyToNullUriConverter() } };

        Uri? actual = JsonSerializer.Deserialize<Uri?>(json, options);

        actual.Should().Be(expected);
    }

    public static readonly TheoryData<string, Uri?> DESERIALIZE_URI_DATA = new() {
        { "null", null },
        { "\"\"", null },
        { "\" \"", null },
        { "\"  \"", null },
        { "\"https://aldaviva.com\"", new Uri("https://aldaviva.com", UriKind.Absolute) }
    };

    [Fact]
    public void deserializeUriErrors() {
        JsonSerializerOptions options = new() { Converters = { new EmptyToNullUriConverter() } };

        ((Action) (() => JsonSerializer.Deserialize<Uri?>("\"https://aldaviva.com:-999999\"", options))).Should().Throw<FormatException>();
        ((Action) (() => JsonSerializer.Deserialize<Uri?>("123", options))).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void serializeUri() {
        JsonSerializerOptions options = new() { Converters = { new EmptyToNullUriConverter() } };

        string actual = JsonSerializer.Serialize(new Uri("https://aldaviva.com"), options);
        actual.Should().Be("\"https://aldaviva.com\"");

        JsonSerializer.Serialize<Uri?>(null, options).Should().Be("null");
    }

}
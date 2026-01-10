using System.Buffers;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Azure.Cosmos;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Data.Cosmos;
public class JsonSerializer2CosmosSerializerAdaptor(IJsonSerializer JsonSerializer, string? TypeDiscriminatorPropertyNameToDedupe = null) : CosmosLinqSerializer
{
    public override Stream ToStream<T>(T input)
    {
        var json = JsonSerializer.ToJson(input);
        if (input!=null && TypeDiscriminatorPropertyNameToDedupe != null)
        {
            var tInput = input.GetType();
            if (!tInput.IsPrimitive && 
                !tInput.IsEnum &&
                tInput != typeof(string) &&
                tInput != typeof(DateTime) &&
                tInput != typeof(DateOnly) &&
                tInput != typeof(TimeOnly) &&
                tInput != typeof(DateTimeOffset) && 
                tInput != typeof(TimeSpan))
            {
                var propertyFinderExpr = RegexHelpers.Create($"\"{Regex.Escape(TypeDiscriminatorPropertyNameToDedupe)}\"\\s*:\\s*\"[^\"]+\"");
                if (propertyFinderExpr.IsMatch(json))
                {
                    return JsonTypeCleaner.RemoveDuplicateTypeKeys(json, TypeDiscriminatorPropertyNameToDedupe);
                }
            }
        }
        var bytes = Encoding.UTF8.GetBytes(json);
        return new MemoryStream(bytes, false);
    }

    public override T FromStream<T>(Stream stream)
    {
        try
        {
            var json = stream.ReadToEnd();
            return JsonSerializer.FromJson<T>(json);
        }
        finally
        {
            //rules of parent class demand disposing of stream in all cases
            stream.Dispose();
        }
    }

    public override string SerializeMemberName(MemberInfo memberInfo)
        => JsonSerializer.GetMemberName(memberInfo);


    private static class JsonTypeCleaner
    {
        // Keep track of whether we're inside an object or array
        private enum Context { Object, Array }

        /// <summary>
        /// Parses the input JSON and drops any duplicate "$type" properties
        /// beyond the first one in each object.
        /// </summary>
        public static Stream RemoveDuplicateTypeKeys(string json, string propertyName)
        {
            var utf8 = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(utf8, isFinalBlock: true, state: default);

            var output = new MemoryStream();
            using var writer = new Utf8JsonWriter(output, new JsonWriterOptions { Indented = false });

            var seenTypeStack = new Stack<bool>();     // per-object: have we seen $type?
            var contextStack = new Stack<Context>();  // are we in Object or Array?

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        seenTypeStack.Push(false);
                        contextStack.Push(Context.Object);
                        writer.WriteStartObject();
                        break;

                    case JsonTokenType.EndObject:
                        seenTypeStack.Pop();
                        contextStack.Pop();
                        writer.WriteEndObject();
                        break;

                    case JsonTokenType.StartArray:
                        contextStack.Push(Context.Array);
                        writer.WriteStartArray();
                        break;

                    case JsonTokenType.EndArray:
                        contextStack.Pop();
                        writer.WriteEndArray();
                        break;

                    case JsonTokenType.PropertyName:
                        var propName = reader.GetString()!;
                        reader.Read();  // move to the value token

                        // If it's a duplicate $type inside an object, skip the entire value
                        if (propName == propertyName
                            && contextStack.Count > 0
                            && contextStack.Peek() == Context.Object
                            && seenTypeStack.Count > 0
                            && seenTypeStack.Peek())
                        {
                            SkipValue(ref reader);
                            continue;
                        }

                        // Otherwise write the name (and mark first $type as seen)
                        if (propName == propertyName && contextStack.Count > 0 && contextStack.Peek() == Context.Object)
                        {
                            seenTypeStack.Pop();
                            seenTypeStack.Push(true);
                        }
                        writer.WritePropertyName(propName);

                        // Now write whatever the value is (object, array, or primitive)
                        WriteValueOrStart(ref reader, writer, seenTypeStack, contextStack);
                        break;

                    // Handle primitives - both at top-level and inside arrays
                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Null:
                        // Write primitives if they're at top-level or inside an array
                        if (contextStack.Count == 0 || contextStack.Peek() == Context.Array)
                        {
                            WritePrimitive(ref reader, writer);
                        }
                        break;
                }
            }

            writer.Flush();
            output.Position = 0;
            return output;
        }

        // Handles writing either a nested object/array start or a primitive value
        private static void WriteValueOrStart(
            ref Utf8JsonReader reader,
            Utf8JsonWriter writer,
            Stack<bool> seenTypeStack,
            Stack<Context> contextStack)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    seenTypeStack.Push(false);
                    contextStack.Push(Context.Object);
                    writer.WriteStartObject();
                    break;

                case JsonTokenType.StartArray:
                    contextStack.Push(Context.Array);
                    writer.WriteStartArray();
                    break;

                default:
                    WritePrimitive(ref reader, writer);
                    break;
            }
        }

        // Writes string/number/boolean/null, preserving exact number text when needed
        private static void WritePrimitive(
            ref Utf8JsonReader reader,
            Utf8JsonWriter writer)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    writer.WriteStringValue(reader.GetString());
                    break;

                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out var l))
                        writer.WriteNumberValue(l);
                    else if (reader.TryGetDouble(out var d))
                        writer.WriteNumberValue(d);
                    else
                    {
                        // Fallback: grab raw UTF-8 bytes and emit them exactly
                        var span = reader.HasValueSequence
                            ? reader.ValueSequence.ToArray()
                            : reader.ValueSpan;
                        var raw = Encoding.UTF8.GetString(span);
                        writer.WriteRawValue(raw, skipInputValidation: true);
                    }
                    break;

                case JsonTokenType.True:
                    writer.WriteBooleanValue(true);
                    break;

                case JsonTokenType.False:
                    writer.WriteBooleanValue(false);
                    break;

                case JsonTokenType.Null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unexpected token in primitive writer: {reader.TokenType}");
            }
        }

        // Advances the reader past whatever value is at current position
        private static void SkipValue(ref Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.StartObject ||
                reader.TokenType == JsonTokenType.StartArray)
            {
                var depth = 1;
                while (depth > 0 && reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.StartObject ||
                        reader.TokenType == JsonTokenType.StartArray)
                        depth++;
                    else if (reader.TokenType == JsonTokenType.EndObject ||
                             reader.TokenType == JsonTokenType.EndArray)
                        depth--;
                }
            }
            // primitives: do nothing; next reader.Read() will move past them
        }
    }
}

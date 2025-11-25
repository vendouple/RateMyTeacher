using System.Text;

namespace RateMyTeacher.Services;

internal static class LessonTextHelper
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a","an","and","are","as","at","be","by","for","from","how","in","is","it","of","on","or","that","the","their","to","was","what","when","where","why","with","we","you","your","our","this","these","those","i","he","she","they","them"
    };

    public static List<string> ExtractKeywords(string input, int take)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new List<string>();
        }

        var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var builder = new StringBuilder();
        foreach (var ch in input)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
            else
            {
                CaptureToken(builder, wordCounts);
            }
        }
        CaptureToken(builder, wordCounts);

        return wordCounts
            .Where(pair => pair.Value > 0)
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key)
            .Select(pair => pair.Key)
            .Take(Math.Max(1, take))
            .ToList();
    }

    public static List<string> SplitSentences(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new List<string>();
        }

        var parts = input
            .Split(new[] { '.', '!', '?', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => NormalizeWhitespace(p))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        return parts;
    }

    public static string NormalizeWhitespace(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return string.Join(' ', input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static void CaptureToken(StringBuilder builder, IDictionary<string, int> store)
    {
        if (builder.Length == 0)
        {
            return;
        }

        var token = builder.ToString();
        builder.Clear();
        if (token.Length < 3 || StopWords.Contains(token))
        {
            return;
        }

        if (store.TryGetValue(token, out var count))
        {
            store[token] = count + 1;
        }
        else
        {
            store[token] = 1;
        }
    }
}

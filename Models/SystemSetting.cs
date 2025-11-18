using System.Collections.Generic;
using RateMyTeacher.Models.Enums;

namespace RateMyTeacher.Models;

public class SystemSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public static class Keys
    {
        public static class Ai
        {
            public const string GlobalEnabled = "AI.Control.GlobalEnabled";
            public const string GlobalMode = "AI.Control.GlobalMode";
            public const string DepartmentDefaultMode = "AI.Control.DepartmentDefaultMode";
            public const string ClassDefaultMode = "AI.Control.ClassDefaultMode";
        }
    }

    public static class Defaults
    {
        public static readonly IReadOnlyDictionary<string, string> Ai = new Dictionary<string, string>
        {
            [Keys.Ai.GlobalEnabled] = bool.TrueString,
            [Keys.Ai.GlobalMode] = AiInteractionMode.Explain.ToString(),
            [Keys.Ai.DepartmentDefaultMode] = AiInteractionMode.Explain.ToString(),
            [Keys.Ai.ClassDefaultMode] = AiInteractionMode.Explain.ToString()
        };
    }
}

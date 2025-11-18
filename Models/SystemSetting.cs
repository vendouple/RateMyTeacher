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
            public const string TeacherMode = "AI.Control.TeacherMode";
            public const string StudentMode = "AI.Control.StudentMode";
        }
    }

    public static class Defaults
    {
        public static readonly IReadOnlyDictionary<string, string> Ai = new Dictionary<string, string>
        {
            [Keys.Ai.GlobalEnabled] = bool.TrueString,
            [Keys.Ai.TeacherMode] = TeacherAiMode.Guided.ToString(),
            [Keys.Ai.StudentMode] = StudentAiMode.Learning.ToString()
        };
    }
}

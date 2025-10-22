# RateMyTeacher - Project Principles

## Code Quality Principles

### 1. Clean Architecture

- **Separation of Concerns**: Keep controllers thin, move business logic to services
- **Dependency Injection**: Use ASP.NET Core's built-in DI container for loose coupling
- **Repository Pattern**: Abstract data access for testability and maintainability
- **Single Responsibility**: Each class/method should have one clear purpose

### 2. Code Standards

- **Naming Conventions**: Follow C# conventions (PascalCase for classes/methods, camelCase for variables)
- **SOLID Principles**: Apply all five SOLID principles consistently
- **DRY (Don't Repeat Yourself)**: Extract reusable logic into helper methods/classes
- **Comments**: Write self-documenting code; use comments only for complex business logic
- **Error Handling**: Implement comprehensive exception handling with meaningful messages

### 3. Testing & Quality Assurance

- **Unit Tests**: Aim for 80%+ code coverage on business logic
- **Integration Tests**: Test controller endpoints and database interactions
- **Code Reviews**: All changes must be reviewed before merging
- **Static Analysis**: Use tools like SonarQube or Roslyn analyzers
- **Performance**: Profile and optimize database queries and API responses

### 4. Security Best Practices

- **Input Validation**: Validate all user inputs on both client and server
- **Authentication & Authorization**: Implement role-based access control
- **SQL Injection Prevention**: Use parameterized queries or EF Core
- **XSS Protection**: Sanitize output, use Content Security Policy
- **HTTPS Only**: Enforce secure connections in production

### 5. Maintainability

- **Version Control**: Meaningful commit messages, feature branches
- **Documentation**: Keep README, API docs, and inline documentation updated
- **Refactoring**: Regularly improve code structure without changing behavior
- **Technical Debt**: Track and address technical debt systematically
- **Code Metrics**: Monitor complexity, maintainability index, and code smells

---

## UX Design Principles

### 1. Neuromorphic Design Language

#### Visual Hierarchy

- **Soft Shadows**: Use subtle, multi-layered shadows to create depth
  - Inner shadows for pressed/active states
  - Outer shadows for elevated elements
  - Shadow blur: 10-30px for soft, realistic effects
- **Subtle Elevation**: Elements appear to float or sink into the surface
- **Rounded Corners**: Use border-radius (8-16px) for friendly, modern feel

#### Color Philosophy

- **Monochromatic Base**: Use variations of the same hue for background/foreground
- **Low Contrast**: Avoid harsh blacks and whites
- **Accent Colors**: Sparingly use vibrant colors for CTAs and important actions
- **Gradient Overlays**: Subtle gradients enhance depth perception

#### Interactive Elements

- **Smooth Transitions**: All state changes should animate (200-300ms)
- **Tactile Feedback**: Buttons should appear to press into the surface
- **Hover States**: Subtle shadow/brightness changes on hover
- **Focus States**: Clear, accessible focus indicators for keyboard navigation

### 2. Dark & Light Mode Support

#### Implementation Strategy

- **System Preference Detection**: Default to user's OS theme preference
- **Manual Toggle**: Allow users to override with theme switcher
- **Persistent Choice**: Save theme preference in localStorage/cookies
- **No Flash**: Load correct theme before first paint

#### Color Tokens

**Light Mode:**

```
--bg-primary: #e4e8ec
--bg-secondary: #d8dde3
--surface: #e4e8ec
--text-primary: #2c3e50
--text-secondary: #5a6c7d
--shadow-light: rgba(255, 255, 255, 0.8)
--shadow-dark: rgba(0, 0, 0, 0.15)
--accent: #3498db
```

**Dark Mode:**

```
--bg-primary: #1a1d23
--bg-secondary: #24282f
--surface: #2a2f38
--text-primary: #e4e8ec
--text-secondary: #a0a8b4
--shadow-light: rgba(255, 255, 255, 0.05)
--shadow-dark: rgba(0, 0, 0, 0.4)
--accent: #5dade2
```

#### Accessibility

- **WCAG AA Compliance**: Maintain 4.5:1 contrast ratio for text
- **Color Independence**: Don't rely solely on color for information
- **High Contrast Mode**: Support for users with visual impairments

### 3. User Experience Excellence

#### Performance

- **Fast Load Times**: Target < 2s initial load, < 500ms subsequent
- **Progressive Enhancement**: Core functionality works without JavaScript
- **Lazy Loading**: Load images and components on demand
- **Optimistic UI**: Show expected results immediately, sync in background

#### Responsiveness

- **Mobile-First**: Design for mobile, enhance for desktop
- **Breakpoints**: xs (< 576px), sm (576px), md (768px), lg (992px), xl (1200px)
- **Touch Targets**: Minimum 44x44px for interactive elements
- **Flexible Layouts**: Use CSS Grid and Flexbox for adaptive designs

#### Navigation

- **Intuitive Structure**: Maximum 3 clicks to any content
- **Breadcrumbs**: Show current location in hierarchy
- **Search**: Implement fast, relevant search functionality
- **Back Button**: Support browser navigation patterns

#### Feedback & Clarity

- **Loading States**: Show spinners/skeletons during async operations
- **Success/Error Messages**: Clear, actionable feedback
- **Empty States**: Guide users when no data exists
- **Tooltips**: Provide contextual help without cluttering UI

### 4. Neuromorphic Component Standards

#### Buttons

```css
/* Elevated state */
box-shadow: -5px -5px 10px var(--shadow-light), 5px 5px 10px var(--shadow-dark);

/* Pressed state */
box-shadow: inset -3px -3px 8px var(--shadow-light), inset 3px 3px 8px var(--shadow-dark);
```

#### Cards

- Soft shadows with 12-20px blur
- Subtle background gradient
- Hover: increase shadow intensity by 20%
- Border-radius: 12-16px

#### Form Inputs

- Inset shadow for input fields (sunken appearance)
- Elevated shadow for dropdowns/selects
- Focus: add colored glow shadow
- Validation: green/red accent shadows

#### Typography

- Font weights: 300 (light), 400 (regular), 600 (semibold), 700 (bold)
- Line height: 1.5 for readability
- Letter spacing: -0.01em for headings
- Font families: Inter, Roboto, or System UI fonts

### 5. Multi-Language Support (i18n)

#### Implementation Strategy

- **Base Language**: English (en-US) as the default/fallback language
- **Localization Framework**: Use ASP.NET Core's built-in localization (`IStringLocalizer`)
- **Resource Files**: Organize translations in `.resx` files per language
- **Language Detection**: Automatic detection based on browser preference
- **Manual Selection**: Allow users to override language preference
- **Persistent Choice**: Save language preference in cookies/user profile

#### Architecture

```
Resources/
├── Controllers/
│   ├── HomeController.en.resx     (English - base)
│   ├── HomeController.id.resx     (Indonesian)
│   └── HomeController.zh.resx     (Chinese)
├── Views/
│   ├── Shared.en.resx
│   ├── Shared.id.resx
│   └── Shared.zh.resx
└── Models/
    ├── ValidationMessages.en.resx
    ├── ValidationMessages.id.resx
    └── ValidationMessages.zh.resx
```

#### Localization Best Practices

**Code Implementation:**

```csharp
// In Startup/Program.cs
services.AddLocalization(options => options.ResourcesPath = "Resources");
services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en-US", "id-ID", "zh-CN" };
    options.SetDefaultCulture("en-US")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// In Controllers
public class HomeController : Controller
{
    private readonly IStringLocalizer<HomeController> _localizer;

    public HomeController(IStringLocalizer<HomeController> localizer)
    {
        _localizer = localizer;
    }

    public IActionResult Index()
    {
        ViewData["Welcome"] = _localizer["WelcomeMessage"];
        return View();
    }
}
```

**In Views:**

```cshtml
@inject IViewLocalizer Localizer
@inject IHtmlLocalizer<SharedResource> SharedLocalizer

<h1>@Localizer["PageTitle"]</h1>
<p>@SharedLocalizer["CommonMessage"]</p>
```

**In JavaScript:**

```javascript
// Pass translations to client-side
const i18n = {
  loading: '@Localizer["Loading"]',
  error: '@Localizer["ErrorMessage"]',
  success: '@Localizer["SuccessMessage"]',
};
```

#### Content Guidelines

**Do Localize:**

- ✅ UI text (buttons, labels, headings)
- ✅ Error and validation messages
- ✅ Navigation menus
- ✅ Success/info notifications
- ✅ Form placeholders and tooltips
- ✅ Email templates
- ✅ Date and time formats
- ✅ Currency and number formats

**Don't Localize:**

- ❌ User-generated content (unless using translation API)
- ❌ Log messages (keep in English for debugging)
- ❌ API keys and configuration values
- ❌ Code identifiers and technical terms

#### Translation Workflow

1. **Development Phase**:

   - Write all strings in English as base language
   - Use meaningful resource keys (e.g., `WelcomeMessage`, not `Label1`)
   - Group related strings in appropriate resource files

2. **Translation Phase**:

   - Extract all resource keys from `.en.resx` files
   - Send to translators or use translation service
   - Review translations for context and accuracy
   - Update corresponding `.{lang}.resx` files

3. **Testing Phase**:

   - Test all languages for UI breaking issues
   - Check text overflow and truncation
   - Verify RTL support if applicable (Arabic, Hebrew)
   - Validate date/time/number formatting

4. **Maintenance**:
   - Track missing translations
   - Update all languages when adding new features
   - Version control translation files

#### Language Switcher Component

**UI Requirements:**

- Dropdown or flag selector in header/navigation
- Display current language clearly
- Persist selection across sessions
- Reload/update UI immediately on change

**Implementation Example:**

```cshtml
<div class="language-selector">
    <select id="languageSelect" onchange="changeLanguage(this.value)">
        <option value="en-US" selected>🇺🇸 English</option>
        <option value="id-ID">🇮🇩 Bahasa Indonesia</option>
        <option value="zh-CN">🇨🇳 中文</option>
    </select>
</div>

<script>
function changeLanguage(culture) {
    document.cookie = `.AspNetCore.Culture=c=${culture}|uic=${culture}; path=/; max-age=31536000`;
    location.reload();
}
</script>
```

#### Formatting Standards

**Dates:**

- Use culture-aware formatting: `DateTime.ToString(CultureInfo.CurrentCulture)`
- Display format examples:
  - en-US: `MM/dd/yyyy` → 10/22/2025
  - id-ID: `dd/MM/yyyy` → 22/10/2025
  - zh-CN: `yyyy年MM月dd日` → 2025 年 10 月 22 日

**Numbers:**

- Decimal separator varies: `1,234.56` (en-US) vs `1.234,56` (id-ID)
- Use `string.Format(CultureInfo.CurrentCulture, "{0:N2}", value)`

**Currency:**

- Display with culture-specific symbol and position
- en-US: `$1,234.56`
- id-ID: `Rp 1.234,56`
- zh-CN: `¥1,234.56`

#### Planned Language Roadmap

**Phase 1 (Current):**

- ✅ English (en-US) - Base language

**Phase 2 (Near Future):**

- 🔄 Indonesian (id-ID) - Local audience
- 🔄 Mandarin Chinese (zh-CN) - Expanded reach

**Phase 3 (Future):**

- ⏳ Spanish (es-ES)
- ⏳ French (fr-FR)
- ⏳ German (de-DE)
- ⏳ Japanese (ja-JP)

#### SEO Considerations

- **URL Structure**: Use `/en/`, `/id/`, `/zh/` prefixes for language-specific pages
- **Hreflang Tags**: Add `<link rel="alternate" hreflang="x" href="...">` tags
- **Meta Tags**: Translate page titles, descriptions, and keywords
- **Sitemap**: Generate language-specific sitemaps
- **Canonical URLs**: Set proper canonical URLs for each language version

#### Accessibility & i18n

- **Screen Readers**: Set `lang` attribute on HTML element
- **Direction Support**: Prepare for RTL (Right-to-Left) languages
- **Font Support**: Ensure fonts support special characters (é, ñ, ü, 中, etc.)
- **Text Expansion**: Allow 30-50% more space for translations (German, Russian expand)
- **Audio/Video**: Provide subtitles or transcripts in multiple languages

---

## Implementation Checklist

### Code Quality

- [ ] Set up dependency injection structure
- [ ] Implement repository pattern for data access
- [ ] Configure logging (Serilog/NLog)
- [ ] Add input validation attributes
- [ ] Set up unit test project
- [ ] Configure CI/CD pipeline
- [ ] Add API documentation (Swagger/OpenAPI)
- [ ] Implement error handling middleware

### UX/Design

- [ ] Create CSS custom properties for theming
- [ ] Implement theme toggle component
- [ ] Build neuromorphic component library
- [ ] Set up responsive breakpoints
- [ ] Add loading states for all async operations
- [ ] Implement smooth page transitions
- [ ] Create empty states for all views
- [ ] Add form validation with clear error messages
- [ ] Test accessibility with screen readers
- [ ] Optimize images and assets
- [ ] Implement dark mode for all pages
- [ ] Add prefers-reduced-motion support

### Internationalization (i18n)

- [ ] Configure ASP.NET Core localization services
- [ ] Create Resources folder structure
- [ ] Create English (en-US) base resource files
- [ ] Implement IStringLocalizer in controllers
- [ ] Add localization to all views
- [ ] Create language switcher component
- [ ] Test date/time/number formatting
- [ ] Add hreflang tags for SEO
- [ ] Set up cookie-based culture persistence
- [ ] Prepare UI for text expansion (30-50%)
- [ ] Add lang attribute to HTML element
- [ ] Verify font support for special characters

---

## Resources

### Code Quality Tools

- **Roslyn Analyzers**: Built-in C# code analysis
- **SonarQube**: Code quality and security scanner
- **xUnit/NUnit**: Testing frameworks
- **Moq**: Mocking library for tests
- **BenchmarkDotNet**: Performance profiling

### Design Resources

- **Neumorphism.io**: Shadow generator tool
- **Coolors.co**: Color palette generator
- **Contrast Checker**: WCAG compliance verification
- **Google Fonts**: Inter, Poppins, Roboto
- **CSS-Tricks**: Neuromorphism tutorials

### Localization Resources

- **Microsoft Docs**: ASP.NET Core Globalization and Localization
- **CLDR**: Unicode Common Locale Data Repository
- **i18next**: JavaScript internationalization framework
- **Crowdin/Lokalise**: Translation management platforms
- **Google Translate API**: Automated translation service
- **PhraseApp**: Localization platform for teams

### Inspiration

- **Dribbble**: Neuromorphic UI designs
- **Behance**: Modern web app interfaces
- **Awwwards**: Award-winning websites
- **CodePen**: Interactive component examples

---

_Last Updated: October 22, 2025_
_Version: 1.0_

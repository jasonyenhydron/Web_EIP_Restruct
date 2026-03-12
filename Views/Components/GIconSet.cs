namespace Web_EIP_Restruct.Views.Components
{
    internal static class GIconSet
    {
        private static readonly Dictionary<string, string> ToneClassMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["brand"] = "text-blue-600",
            ["muted"] = "text-slate-400",
            ["soft-muted"] = "text-slate-300",
            ["tree-folder"] = "text-amber-400",
            ["tree-setting"] = "text-slate-500",
            ["tree-code"] = "text-cyan-500",
            ["tree-user"] = "text-indigo-500",
        };

        private static readonly Dictionary<string, string> ContextToneMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["empty-state"] = "soft-muted",
            ["page-title"] = "brand",
            ["page-meta"] = "muted",
            ["tree-leaf"] = "muted",
            ["tree-node:folder"] = "tree-folder",
            ["tree-node:setting"] = "tree-setting",
            ["tree-node:settings"] = "tree-setting",
            ["tree-node:cog"] = "tree-setting",
            ["tree-node:code"] = "tree-code",
            ["tree-node:user"] = "tree-user",
            ["tree-node:*"] = "tree-folder",
        };

        public static string Render(string? icon, string cls = "w-4 h-4", string extraClass = "")
            => RenderCore(icon, cls, string.Empty, extraClass);

        public static string Render(string? icon, string cls, string tone, string extraClass)
            => RenderCore(icon, cls, tone, extraClass);

        public static string RenderForContext(string? icon, string cls, string context, string extraClass = "")
            => RenderCore(icon, cls, GetContextTone(icon, context), extraClass);

        public static string ResolveToneClass(string? toneOrClass)
        {
            if (string.IsNullOrWhiteSpace(toneOrClass))
            {
                return string.Empty;
            }

            return ToneClassMap.TryGetValue(toneOrClass.Trim(), out var toneClass)
                ? toneClass
                : toneOrClass.Trim();
        }

        public static string GetContextTone(string? icon, string? context)
        {
            var normalizedIcon = (icon ?? string.Empty).Trim().ToLowerInvariant();
            var normalizedContext = (context ?? string.Empty).Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(normalizedContext))
            {
                return string.Empty;
            }

            var specificKey = $"{normalizedContext}:{normalizedIcon}";
            if (ContextToneMap.TryGetValue(specificKey, out var specificTone))
            {
                return specificTone;
            }

            var wildcardKey = $"{normalizedContext}:*";
            if (ContextToneMap.TryGetValue(wildcardKey, out var wildcardTone))
            {
                return wildcardTone;
            }

            return ContextToneMap.TryGetValue(normalizedContext, out var defaultTone)
                ? defaultTone
                : string.Empty;
        }

        private static string RenderCore(string? icon, string cls, string tone, string extraClass)
        {
            var name = (icon ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            var toneClass = ResolveToneClass(tone);
            var finalClass = string.Join(" ", new[] { cls, toneClass, extraClass }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            static string Stroke(string classes, string path) =>
                $"""<svg class="{classes}" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="{path}"/></svg>""";

            static string StrokeMulti(string classes, params string[] paths)
            {
                var joined = string.Join("", paths.Select(p =>
                    $"""<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="{p}"/>"""));
                return $"""<svg class="{classes}" fill="none" stroke="currentColor" viewBox="0 0 24 24">{joined}</svg>""";
            }

            return name switch
            {
                "save" => Stroke(finalClass, "M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4"),
                "trash" or "delete" => Stroke(finalClass, "M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"),
                "edit" => Stroke(finalClass, "M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"),
                "add" => StrokeMulti(finalClass, "M12 9v3m0 0v3m0-6h3m-3 0H9", "M21 12a9 9 0 11-18 0 9 9 0 0118 0z"),
                "search" => Stroke(finalClass, "M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"),
                "plus" => Stroke(finalClass, "M12 4v16m8-8H4"),
                "close" => Stroke(finalClass, "M6 18L18 6M6 6l12 12"),
                "check" => Stroke(finalClass, "M5 13l4 4L19 7"),
                "refresh" => Stroke(finalClass, "M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"),
                "upload" => Stroke(finalClass, "M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"),
                "download" => Stroke(finalClass, "M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"),
                "print" => StrokeMulti(finalClass, "M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z"),
                "eye" => StrokeMulti(finalClass, "M15 12a3 3 0 11-6 0 3 3 0 016 0z", "M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"),
                "list" => Stroke(finalClass, "M4 6h16M4 10h16M4 14h16M4 18h16"),
                "play" or "excute" => $"""<svg class="{finalClass}" fill="currentColor" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>""",
                "copy" => StrokeMulti(finalClass, "M8 11h3v10h2V11h3l-4-4-4 4z", "M4 19v2h16v-2H4z"),
                "filter" => Stroke(finalClass, "M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"),
                "back" => Stroke(finalClass, "M10 19l-7-7m0 0l7-7m-7 7h18"),
                "forward" => Stroke(finalClass, "M14 5l7 7m0 0l-7 7m7-7H3"),
                "export" => Stroke(finalClass, "M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"),
                "info" => StrokeMulti(finalClass, "M13 16h-1v-4h-1", "M12 8h.01", "M21 12a9 9 0 11-18 0 9 9 0 0118 0z"),
                "calendar" => Stroke(finalClass, "M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"),
                "folder" => Stroke(finalClass, "M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z"),
                "file" => Stroke(finalClass, "M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"),
                "circle" => $"""<svg class="{finalClass}" fill="currentColor" viewBox="0 0 24 24"><circle cx="12" cy="12" r="4"/></svg>""",
                "user" => Stroke(finalClass, "M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"),
                "setting" or "settings" or "cog" => StrokeMulti(finalClass, "M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z", "M15 12a3 3 0 11-6 0 3 3 0 016 0z"),
                "clock" => Stroke(finalClass, "M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"),
                "code" => Stroke(finalClass, "M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4"),
                "chart" => Stroke(finalClass, "M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"),
                "database" => Stroke(finalClass, "M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4m0 5c0 2.21-3.582 4-8 4s-8-1.79-8-4"),
                "shield" => Stroke(finalClass, "M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"),
                "inbox" => Stroke(finalClass, "M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4"),
                "error" => StrokeMulti(finalClass, "M12 8v4", "M12 16h.01", "M21 12a9 9 0 11-18 0 9 9 0 0118 0z"),
                "document" => Stroke(finalClass, "M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"),
                "home" => Stroke(finalClass, "M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"),
                _ => string.Empty
            };
        }
    }
}


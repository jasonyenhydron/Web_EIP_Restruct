namespace Web_EIP_Restruct.Views.Components
{
    internal static class TagHelperClassResolver
    {
        public static string Resolve(string defaultClass, string overrideClass, string extraClass = "")
        {
            if (!string.IsNullOrWhiteSpace(overrideClass))
            {
                return overrideClass.Trim();
            }

            if (string.IsNullOrWhiteSpace(extraClass))
            {
                return defaultClass?.Trim() ?? string.Empty;
            }

            return $"{defaultClass} {extraClass}".Trim();
        }
    }
}



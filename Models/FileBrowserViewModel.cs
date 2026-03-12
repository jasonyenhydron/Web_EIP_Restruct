namespace Web_EIP_Restruct.Models
{
    public class FileBrowserViewModel
    {
        public string RootDisplay { get; set; } = string.Empty;
        public string CurrentRelativePath { get; set; } = string.Empty;
        public string? ParentRelativePath { get; set; }
        public string? ErrorMessage { get; set; }
        public string SearchQuery { get; set; } = string.Empty;
        public bool RequiresWindowsAuth { get; set; }
        public bool IsWindowsAuthPassed { get; set; }
        public string AuthUserName { get; set; } = string.Empty;
        public string RequestedPath { get; set; } = string.Empty;
        public List<FileEntryViewModel> Entries { get; set; } = new();
    }

    public class FileEntryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public long? SizeBytes { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}


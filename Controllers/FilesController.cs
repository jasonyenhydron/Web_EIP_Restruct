
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Web_EIP_Restruct.Models;

namespace Web_EIP_Restruct.Controllers
{
    public class FilesController : Controller
    {
        private readonly IConfiguration _configuration;
        private const string SessionWinUser = "files_win_user";
        private const string SessionWinPwd = "files_win_pwd";

        public FilesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index(string? path = null, string? q = null)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var model = new FileBrowserViewModel
            {
                RequestedPath = path ?? string.Empty,
                SearchQuery = (q ?? string.Empty).Trim()
            };

            var root = ResolveRootPath();
            if (string.IsNullOrEmpty(root))
            {
                model.ErrorMessage = "File root path is not configured.";
                return View(model);
            }

            model.RootDisplay = root;
            model.RequiresWindowsAuth = IsUncPath(root);

            if (model.RequiresWindowsAuth)
            {
                var winUser = HttpContext.Session.GetString(SessionWinUser) ?? string.Empty;
                var winPwd = HttpContext.Session.GetString(SessionWinPwd) ?? string.Empty;
                model.AuthUserName = winUser;

                if (string.IsNullOrEmpty(winUser) || string.IsNullOrEmpty(winPwd))
                {
                    model.ErrorMessage = "Windows authentication is required.";
                    return View(model);
                }

                if (!TryConnectShare(root, winUser, winPwd, out var authError))
                {
                    model.ErrorMessage = $"Windows authentication failed: {authError}";
                    return View(model);
                }

                model.IsWindowsAuthPassed = true;
            }

            var fullPath = ResolveSafePath(root, path, out var normalizedRelative);
            if (fullPath == null || !Directory.Exists(fullPath))
            {
                model.ErrorMessage = "Directory does not exist or is outside the allowed root.";
                return View(model);
            }

            model.CurrentRelativePath = normalizedRelative;
            model.ParentRelativePath = GetParentRelativePath(normalizedRelative);
            var entries = ReadDirectoryEntries(root, fullPath);
            if (!string.IsNullOrWhiteSpace(model.SearchQuery))
            {
                entries = entries
                    .Where(x => x.Name.Contains(model.SearchQuery, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            model.Entries = entries;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Authenticate(string username, string password, string? domain, string? path = null, string? q = null)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var root = ResolveRootPath();
            if (string.IsNullOrEmpty(root) || !IsUncPath(root))
            {
                return RedirectToAction(nameof(Index), new { path, q });
            }

            var account = NormalizeAccount(username, domain);
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
            {
                TempData["FilesAuthError"] = "Please provide Windows account and password.";
                return RedirectToAction(nameof(Index), new { path, q });
            }

            if (!TryConnectShare(root, account, password, out var authError))
            {
                TempData["FilesAuthError"] = $"Windows authentication failed: {authError}";
                return RedirectToAction(nameof(Index), new { path });
            }

            HttpContext.Session.SetString(SessionWinUser, account);
            HttpContext.Session.SetString(SessionWinPwd, password);
            return RedirectToAction(nameof(Index), new { path, q });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearAuth()
        {
            HttpContext.Session.Remove(SessionWinUser);
            HttpContext.Session.Remove(SessionWinPwd);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Open(string path)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var root = ResolveRootPath();
            if (string.IsNullOrEmpty(root)) return NotFound();
            if (!EnsureShareAuth(root)) return RedirectToAction(nameof(Index), new { path = string.Empty });

            var fullPath = ResolveSafePath(root, path, out _);
            if (fullPath == null || !System.IO.File.Exists(fullPath)) return NotFound();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullPath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var inline = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                         || contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
                         || contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase);

            if (inline)
            {
                return PhysicalFile(fullPath, contentType, enableRangeProcessing: true);
            }

            return PhysicalFile(fullPath, contentType, Path.GetFileName(fullPath), enableRangeProcessing: true);
        }

        [HttpGet]
        public IActionResult Download(string path)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var root = ResolveRootPath();
            if (string.IsNullOrEmpty(root)) return NotFound();
            if (!EnsureShareAuth(root)) return RedirectToAction(nameof(Index), new { path = string.Empty });

            var fullPath = ResolveSafePath(root, path, out _);
            if (fullPath == null || !System.IO.File.Exists(fullPath)) return NotFound();

            return PhysicalFile(fullPath, "application/octet-stream", Path.GetFileName(fullPath), enableRangeProcessing: true);
        }

        [HttpPost("/api/files/upload")]
        [RequestSizeLimit(209715200)] // 200 MB
        public async Task<IActionResult> Upload([FromForm] List<IFormFile> files, [FromForm] string? folder = null, CancellationToken cancellationToken = default)
        {
            if (!IsLoggedIn()) return Unauthorized(new { status = "error", message = "Unauthorized" });

            if (files == null || files.Count == 0)
            {
                return BadRequest(new { status = "error", message = "No file uploaded" });
            }

            var root = ResolveRootPath();
            if (string.IsNullOrEmpty(root))
            {
                return BadRequest(new { status = "error", message = "File root path is not configured" });
            }

            if (!EnsureShareAuth(root))
            {
                return Unauthorized(new { status = "error", message = "Windows authentication required" });
            }

            var targetDirectory = ResolveSafePath(root, folder, out var normalizedFolder);
            if (targetDirectory == null)
            {
                return BadRequest(new { status = "error", message = "Invalid target folder" });
            }

            Directory.CreateDirectory(targetDirectory);

            var savedFiles = new List<object>();
            foreach (var file in files.Where(f => f != null && f.Length > 0))
            {
                var originalName = Path.GetFileName(file.FileName ?? string.Empty);
                if (string.IsNullOrWhiteSpace(originalName))
                {
                    continue;
                }

                var filePath = BuildUniquePath(targetDirectory, originalName, out var storedName);
                await using (var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    await file.CopyToAsync(fs, cancellationToken);
                }

                var relativePath = string.IsNullOrWhiteSpace(normalizedFolder)
                    ? storedName
                    : $"{normalizedFolder.Replace('\\', '/')}/{storedName}";
                var renamed = !string.Equals(originalName, storedName, StringComparison.OrdinalIgnoreCase);
                savedFiles.Add(new
                {
                    fileName = originalName,
                    storedName,
                    relativePath,
                    size = file.Length,
                    renamed
                });
            }

            if (savedFiles.Count == 0)
            {
                return BadRequest(new { status = "error", message = "No valid file to upload" });
            }

            return Ok(new { status = "ok", files = savedFiles });
        }

        private bool IsLoggedIn()
        {
            var username = HttpContext.Session.GetString("username");
            return !string.IsNullOrEmpty(username);
        }

        private bool EnsureShareAuth(string root)
        {
            if (!IsUncPath(root)) return true;
            var winUser = HttpContext.Session.GetString(SessionWinUser) ?? string.Empty;
            var winPwd = HttpContext.Session.GetString(SessionWinPwd) ?? string.Empty;
            if (string.IsNullOrEmpty(winUser) || string.IsNullOrEmpty(winPwd)) return false;
            return TryConnectShare(root, winUser, winPwd, out _);
        }

        private string? ResolveRootPath()
        {
            var configuredRoots = _configuration.GetSection("FileManagement:Roots").Get<string[]>() ?? Array.Empty<string>();
            foreach (var root in configuredRoots)
            {
                if (string.IsNullOrWhiteSpace(root)) continue;

                try
                {
                    var candidate = root.Trim();
                    if (IsUncPath(candidate)) return candidate;
                    if (Directory.Exists(candidate)) return Path.GetFullPath(candidate);
                }
                catch
                {
                }
            }

            return null;
        }

        private static bool IsUncPath(string path)
        {
            return path.StartsWith(@"\\", StringComparison.Ordinal);
        }

        private static string NormalizeAccount(string username, string? domain)
        {
            var user = (username ?? string.Empty).Trim();
            var dm = (domain ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(user)) return string.Empty;
            if (user.Contains('\\') || user.Contains('@') || string.IsNullOrEmpty(dm)) return user;
            return $@"{dm}\{user}";
        }

        private static bool TryConnectShare(string uncPath, string username, string password, out string error)
        {
            error = string.Empty;
            var remote = GetUncRoot(uncPath);
            if (string.IsNullOrEmpty(remote))
            {
                error = "Invalid UNC root.";
                return false;
            }

            var nr = new NetResource
            {
                Scope = 0,
                ResourceType = 1,
                DisplayType = 0,
                Usage = 0,
                LocalName = null,
                RemoteName = remote,
                Comment = null,
                Provider = null
            };

            WNetCancelConnection2(remote, 0, true);

            var result = WNetAddConnection2(ref nr, password, username, 0);
            if (result == 0 || result == 1219) return true; // 1219: already connected with different credential context

            error = $"Win32 Error {result}";
            return false;
        }

        private static string GetUncRoot(string uncPath)
        {
            var trimmed = uncPath.Trim().TrimEnd('\\');
            if (!trimmed.StartsWith(@"\\", StringComparison.Ordinal)) return string.Empty;
            var parts = trimmed.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return string.Empty;
            return $@"\\{parts[0]}\{parts[1]}";
        }

        private static string? ResolveSafePath(string root, string? relativePath, out string normalizedRelative)
        {
            normalizedRelative = string.Empty;
            var cleaned = (relativePath ?? string.Empty).Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            var combined = string.IsNullOrEmpty(cleaned) ? root : Path.Combine(root, cleaned);
            var fullPath = Path.GetFullPath(combined);
            var fullRoot = Path.GetFullPath(root);

            if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (fullPath.Length > fullRoot.Length)
            {
                normalizedRelative = fullPath.Substring(fullRoot.Length).TrimStart(Path.DirectorySeparatorChar);
            }

            return fullPath;
        }

        private static string? GetParentRelativePath(string currentRelativePath)
        {
            if (string.IsNullOrEmpty(currentRelativePath)) return null;
            var parent = Path.GetDirectoryName(currentRelativePath);
            return parent?.Replace(Path.DirectorySeparatorChar, '/');
        }

        private static List<FileEntryViewModel> ReadDirectoryEntries(string root, string fullPath)
        {
            var entries = new List<FileEntryViewModel>();

            foreach (var dir in Directory.GetDirectories(fullPath).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
            {
                var info = new DirectoryInfo(dir);
                entries.Add(new FileEntryViewModel
                {
                    Name = info.Name,
                    RelativePath = ToRelative(root, dir),
                    IsDirectory = true,
                    ModifiedAt = info.LastWriteTime
                });
            }

            foreach (var file in Directory.GetFiles(fullPath).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var info = new FileInfo(file);
                entries.Add(new FileEntryViewModel
                {
                    Name = info.Name,
                    RelativePath = ToRelative(root, file),
                    IsDirectory = false,
                    SizeBytes = info.Length,
                    ModifiedAt = info.LastWriteTime
                });
            }

            return entries;
        }

        private static string ToRelative(string root, string fullPath)
        {
            var rel = Path.GetRelativePath(root, fullPath);
            return rel.Replace(Path.DirectorySeparatorChar, '/');
        }

        private static string BuildUniquePath(string directory, string originalName, out string storedName)
        {
            var nameOnly = Path.GetFileNameWithoutExtension(originalName);
            var ext = Path.GetExtension(originalName);
            storedName = $"{nameOnly}{ext}";
            var full = Path.Combine(directory, storedName);
            if (!System.IO.File.Exists(full)) return full;

            var suffix = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            storedName = $"{nameOnly}_{suffix}{ext}";
            full = Path.Combine(directory, storedName);
            var seq = 1;
            while (System.IO.File.Exists(full))
            {
                storedName = $"{nameOnly}_{suffix}_{seq}{ext}";
                full = Path.Combine(directory, storedName);
                seq++;
            }
            return full;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NetResource
        {
            public int Scope;
            public int ResourceType;
            public int DisplayType;
            public int Usage;
            public string? LocalName;
            public string? RemoteName;
            public string? Comment;
            public string? Provider;
        }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetAddConnection2(ref NetResource netResource, string? password, string? username, int flags);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);
    }
}


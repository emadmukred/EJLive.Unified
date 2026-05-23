using System;
using System.Collections.Generic;

namespace EJLive.Core.Services
{
    /// <summary>
    /// نظام صلاحيات المستخدمين (Role-Based Access Control)
    /// يطبق: L-12 (RBAC) — 4 أدوار: Observer / Admin / Auditor / Support
    /// </summary>
    public class RoleBasedAccess
    {
        private readonly Dictionary<string, HashSet<string>> _permissions = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Admin"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "*" },
            ["Auditor"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "view", "export" },
            ["Support"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "view", "remote", "sync" },
            ["Observer"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "view" }
        };

        private static readonly Dictionary<string, UserSession> _activeSessions =
            new Dictionary<string, UserSession>(StringComparer.OrdinalIgnoreCase);

        private static readonly object _lock = new object();

        // مستخدمون تجريبيون افتراضيون (في الإنتاج: استخدم LDAP أو قاعدة بيانات)
        private static readonly Dictionary<string, (string PasswordHash, UserRole Role)> _users =
            new Dictionary<string, (string, UserRole)>(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"]    = (HashPassword("admin123"),    UserRole.Admin),
            ["observer"] = (HashPassword("obs123"),      UserRole.Observer),
            ["auditor"]  = (HashPassword("audit123"),    UserRole.Auditor),
            ["support"]  = (HashPassword("support123"),  UserRole.Support),
        };

        public static LoginResult Login(string userId, string password, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
                return new LoginResult { Success = false, Message = "بيانات الدخول غير مكتملة" };

            lock (_lock)
            {
                if (!_users.TryGetValue(userId, out var userInfo))
                    return new LoginResult { Success = false, Message = "المستخدم غير موجود" };

                if (userInfo.PasswordHash != HashPassword(password))
                    return new LoginResult { Success = false, Message = "كلمة السر غير صحيحة" };

                var token = Guid.NewGuid().ToString("N");
                var session = new UserSession
                {
                    Token = token,
                    UserId = userId,
                    Role = userInfo.Role,
                    LoginTime = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow,
                    IpAddress = ipAddress ?? "127.0.0.1"
                };

                _activeSessions[token] = session;
                AuditLogger.LogLogin(userId, ipAddress ?? "127.0.0.1", true);

                return new LoginResult
                {
                    Success = true,
                    Token = token,
                    Role = userInfo.Role,
                    Message = $"مرحبًا {userId} — دورك: {userInfo.Role}"
                };
            }
        }

        public static void Logout(string token)
        {
            lock (_lock)
            {
                if (_activeSessions.TryGetValue(token, out var session))
                {
                    AuditLogger.Log(AuditAction.Logout, session.UserId, null, "User logged out", true);
                    _activeSessions.Remove(token);
                }
            }
        }

        public static bool HasPermission(string token, Permission permission)
        {
            lock (_lock)
            {
                if (!_activeSessions.TryGetValue(token, out var session))
                    return false;

                // تحديث النشاط
                session.LastActivity = DateTime.UtcNow;

                return RolePermissions.TryGetValue(session.Role, out var perms) &&
                       perms.Contains(permission);
            }
        }

        public static UserSession? GetSession(string token)
        {
            lock (_lock)
            {
                _activeSessions.TryGetValue(token, out var s);
                return s;
            }
        }

        public static void CleanExpiredSessions(TimeSpan maxIdle)
        {
            var cutoff = DateTime.UtcNow - maxIdle;
            lock (_lock)
            {
                var expired = new List<string>();
                foreach (var kv in _activeSessions)
                    if (kv.Value.LastActivity < cutoff)
                        expired.Add(kv.Key);
                foreach (var t in expired)
                    _activeSessions.Remove(t);
            }
        }

        private static string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes("EJLive_Salt_" + password));
            return Convert.ToBase64String(bytes);
        }

        // ==========================================
        // جدول الصلاحيات حسب الدور
        // ==========================================

        private static readonly Dictionary<UserRole, HashSet<Permission>> RolePermissions =
            new Dictionary<UserRole, HashSet<Permission>>
        {
            [UserRole.Observer] = new HashSet<Permission>
            {
                Permission.ViewDashboard,
                Permission.ViewATMStatus,
                Permission.ViewJournal,
                Permission.ViewAlerts,
            },
            [UserRole.Support] = new HashSet<Permission>
            {
                Permission.ViewDashboard,
                Permission.ViewATMStatus,
                Permission.ViewJournal,
                Permission.ViewAlerts,
                Permission.TakeScreenshot,
                Permission.GhostView,
                Permission.ViewLogs,
            },
            [UserRole.Auditor] = new HashSet<Permission>
            {
                Permission.ViewDashboard,
                Permission.ViewATMStatus,
                Permission.ViewJournal,
                Permission.ViewAlerts,
                Permission.ViewLogs,
                Permission.ExportReports,
                Permission.ViewAuditLog,
            },
            [UserRole.Admin] = new HashSet<Permission>
            {
                Permission.ViewDashboard,
                Permission.ViewATMStatus,
                Permission.ViewJournal,
                Permission.ViewAlerts,
                Permission.ViewLogs,
                Permission.ExportReports,
                Permission.ViewAuditLog,
                Permission.TakeScreenshot,
                Permission.GhostView,
                Permission.SendCommands,
                Permission.RestartATM,
                Permission.ChangePassword,
                Permission.SyncImages,
                Permission.ManageUsers,
                Permission.ChangeConfig,
                Permission.ForceSync,
            },
        };

        public bool Can(string role, string permission)
        {
            return _permissions.TryGetValue(role, out var permissions) &&
                   (permissions.Contains("*") || permissions.Contains(permission));
        }
    }

    public class UserSession
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LastActivity { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public enum UserRole
    {
        Observer,   // عرض فقط
        Support,    // تشخيص محدود
        Auditor,    // تدقيق وتقارير
        Admin       // تحكم كامل
    }

    public enum Permission
    {
        ViewDashboard,
        ViewATMStatus,
        ViewJournal,
        ViewAlerts,
        ViewLogs,
        ExportReports,
        ViewAuditLog,
        TakeScreenshot,
        GhostView,
        SendCommands,
        RestartATM,
        ChangePassword,
        SyncImages,
        ManageUsers,
        ChangeConfig,
        ForceSync
    }
}

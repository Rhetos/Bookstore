using Rhetos;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Security;
using Rhetos.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Bookstore.Service
{
    [Options("PermissionsRecorder")]
    public class PermissionsRecorderOptions
    {
        /// <summary>User whose permissions are recorded.</summary>
        public string User { get; set; }

        /// <summary>The recorded permissions are added to this target role.</summary>
        public string Role { get; set; }

        /// <summary>Allow all basic permissions (claims) to the user while being recorded.</summary>
        public bool AllowAll { get; set; }

        public bool IsRecordingEnabled() => !string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Role);
    }

    /// <summary>
    /// Records basic permissions (claims) of a specified user, while using the application.
    /// All claims that were checked and allowed for the user will be added to the specified role.
    /// </summary>
    /// <remarks>
    /// To enable recording, configure <see cref="PermissionsRecorderOptions"/> in app settings.
    /// The configured user will have full access rights while recording.
    /// The recording will not work if the user authorization is overridden by options
    /// BuiltinAdminOverride, AllClaimsForAnonymous or AllClaimsForUsers.
    /// </remarks>
    public class PermissionsRecorder : IAuthorizationProvider
    {
        private readonly IAuthorizationProvider _decorated;
        private readonly SqlTransactionBatches _sqlTransactionBatches;
        private readonly ISqlExecuter _sqlExecuter;
        private readonly IAuthorizationData _authorizationData;
        private readonly PermissionsRecorderOptions _options;
        private static readonly ConcurrentDictionary<string, HashSet<Claim>> _recordedUsersClaims = new ConcurrentDictionary<string, HashSet<Claim>>();

        public PermissionsRecorder(
            IAuthorizationProvider decorated,
            SqlTransactionBatches sqlTransactionBatches,
            ISqlExecuter sqlExecuter,
            IAuthorizationData authorizationData,
            PermissionsRecorderOptions options)
        {
            _decorated = decorated;
            _sqlTransactionBatches = sqlTransactionBatches;
            _sqlExecuter = sqlExecuter;
            _authorizationData = authorizationData;
            _options = options;
        }

        public IList<bool> GetAuthorizations(IUserInfo userInfo, IList<Claim> requiredClaims)
        {
            var authorizations = _decorated.GetAuthorizations(userInfo, requiredClaims);

            if (_options.IsRecordingEnabled() && userInfo.IsUserRecognized && userInfo.UserName == _options.User)
            {
                if (_options.AllowAll)
                    for (int a = 0; a < authorizations.Count; a++)
                        authorizations[a] = true;

                RecordAuthorizedClaims(requiredClaims, authorizations);
            }

            return authorizations;
        }

        private void RecordAuthorizedClaims(IList<Claim> requiredClaims, IList<bool> authorizations)
        {
            if (!authorizations.Any(authorized => authorized))
                return;

            var authorizedClaims = requiredClaims.Zip(authorizations, (claim, authorized) => (claim, authorized))
                .Where(authorization => authorization.authorized)
                .Select(authorization => authorization.claim)
                .ToList();

            var recordedClaims = _recordedUsersClaims.GetOrAdd(_options.User, new HashSet<Claim>());

            List<Claim> newClaims;
            lock (recordedClaims) // Locking because there might be parallel requests from the same user in the same web app.
            {
                newClaims = authorizedClaims.Where(claim => !recordedClaims.Contains(claim)).ToList();
                if (!newClaims.Any())
                    return;
                recordedClaims.UnionWith(newClaims);
            }

            InsertNewClaimsToDatabase(newClaims);
        }

        private void InsertNewClaimsToDatabase(List<Claim> newClaims)
        {
            Guid principalID = _authorizationData.GetPrincipal(_options.User).ID;
            Guid roleId = GetRoleId();
            var allClaimsIds = _authorizationData.GetClaims(newClaims); // allClaimsIds may contain more claims than newClaims.
            var newClaimsIds = newClaims
                .Select(claim => allClaimsIds.GetValue(claim, () => $"{nameof(PermissionsRecorder)}: Claim '{claim}' does not exist in database.").ID)
                .ToList();

            var insertScripts = newClaimsIds.Select(claimId =>
$@"IF NOT EXISTS (SELECT TOP 1 1 FROM Common.RolePermission WHERE RoleID = '{roleId}' AND ClaimID = '{claimId}')
    INSERT INTO Common.RolePermission (RoleID, ClaimID, IsAuthorized)
        SELECT '{roleId}', '{claimId}', 1;") // 'IF NOT EXISTS' is needed in SQL for parallel requests on multiple web servers.
                .ToList();

            insertScripts = _sqlTransactionBatches.JoinScripts(insertScripts);
            _sqlExecuter.ExecuteSql(insertScripts);
        }

        private Guid GetRoleId()
        {
            Guid roleId = Guid.Empty;

            _sqlExecuter.ExecuteReader(
                $"SELECT ID FROM Common.Role WHERE Name = {SqlUtility.QuoteText(_options.Role)}",
                reader => roleId = reader.GetGuid(0));

            if (roleId == Guid.Empty)
                throw new UserException($"{nameof(PermissionsRecorder)}: Role '{_options.Role}' does not exist in database. It is configured for recording users claims.");

            return roleId;
        }
    }
}

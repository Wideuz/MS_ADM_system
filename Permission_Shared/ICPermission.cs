namespace Permission_Shared
{
    public interface ICPermission
    {
        static string ProjectIdentity => typeof(ICPermission).FullName ?? nameof(ICPermission);
        /// <summary>
        /// 依照 SteamID64 查詢玩家身分
        /// </summary>
        string GetIdentity(string steamId64);

        /// <summary>
        /// 非同步查詢（如果需要從資料庫比對）
        /// </summary>
        Task<string> GetIdentityFromDatabaseAsync(string steamId64);

        /// <summary>
        /// 取得預設身分
        /// </summary>
        string GetDefaultIdentity();
    }
}

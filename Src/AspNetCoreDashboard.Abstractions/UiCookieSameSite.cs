namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>Cookie SameSite 属性取值。</summary>
  public enum UiCookieSameSite
  {
    /// <summary>不输出 SameSite 属性。</summary>
    Unspecified = 0,

    /// <summary>SameSite=Lax</summary>
    Lax = 1,

    /// <summary>SameSite=Strict</summary>
    Strict = 2,

    /// <summary>SameSite=None（通常需要配合 Secure 使用）。</summary>
    None = 3
  }
}

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// Notification that occurs at the very end of the Umbraco boot process (after all components are initialized).
/// </summary>
public class UmbracoApplicationStartingNotification : IUmbracoApplicationLifetimeNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoApplicationStartingNotification" /> class.
    /// </summary>
    /// <param name="runtimeLevel">The runtime level</param>
    /// <param name="isRestarting">Indicates whether Umbraco is restarting.</param>
    public UmbracoApplicationStartingNotification(RuntimeLevel runtimeLevel, bool isRestarting)
    {
        RuntimeLevel = runtimeLevel;
        IsRestarting = isRestarting;
    }

    /// <summary>
    /// Gets the runtime level.
    /// </summary>
    /// <value>
    /// The runtime level.
    /// </value>
    public RuntimeLevel RuntimeLevel { get; }

    /// <inheritdoc />
    public bool IsRestarting { get; }
}

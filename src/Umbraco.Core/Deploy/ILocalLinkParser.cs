namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Provides methods to parse local link tags in property values.
/// </summary>
public interface ILocalLinkParser
{
    /// <summary>
    ///     Parses an Umbraco property value and produces an artifact property value.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <param name="dependencies">A list of dependencies.</param>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     Turns {{localLink:1234}} into {{localLink:umb://{type}/{id}}} and adds the corresponding udi to the
    ///     dependencies.
    /// </remarks>
    [Obsolete("Please use the overload taking all parameters. This method will be removed in Umbraco 14.")]
    string ToArtifact(string value, ICollection<Udi> dependencies);

    /// <summary>
    ///     Parses an Umbraco property value and produces an artifact property value.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <param name="dependencies">A list of dependencies.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     Turns {{localLink:1234}} into {{localLink:umb://{type}/{id}}} and adds the corresponding udi to the
    ///     dependencies.
    /// </remarks>
#pragma warning disable CS0618 // Type or member is obsolete
    string ToArtifact(string value, ICollection<Udi> dependencies, IContextCache contextCache) => ToArtifact(value, dependencies);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    ///     Parses an artifact property value and produces an Umbraco property value.
    /// </summary>
    /// <param name="value">The artifact property value.</param>
    /// <returns>The parsed value.</returns>
    /// <remarks>Turns {{localLink:umb://{type}/{id}}} into {{localLink:1234}}.</remarks>
    [Obsolete("Please use the overload taking all parameters. This method will be removed in Umbraco 14.")]
    string FromArtifact(string value);

    /// <summary>
    ///     Parses an artifact property value and produces an Umbraco property value.
    /// </summary>
    /// <param name="value">The artifact property value.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>The parsed value.</returns>
    /// <remarks>Turns {{localLink:umb://{type}/{id}}} into {{localLink:1234}}.</remarks>
#pragma warning disable CS0618 // Type or member is obsolete
    string FromArtifact(string value, IContextCache contextCache) => FromArtifact(value);
#pragma warning restore CS0618 // Type or member is obsolete
}

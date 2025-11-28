namespace TfsViewer.Core.Contracts;

/// <summary>
/// Interface for Visual Studio configuration settings
/// </summary>
public interface IVsConfiguration
{
    /// <summary>
    /// Optional full path to Visual Studio executable for opening items in VS.
    /// If not set, VS launch features will be disabled.
    /// </summary>
    string? VsExePath { get; set; }

    /// <summary>
    /// Optional argument to pass to Visual Studio before the item identifier.
    /// Example: "-edit" or solution path; the work item/URL will be appended after.
    /// </summary>
    string? VsArgument { get; set; }
}
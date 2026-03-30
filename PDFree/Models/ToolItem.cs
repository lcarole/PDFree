using Avalonia.Media;

namespace PDFree.Models;

public record ToolItem(string Id, string Title, string Description, StreamGeometry Icon, Color AccentColor);

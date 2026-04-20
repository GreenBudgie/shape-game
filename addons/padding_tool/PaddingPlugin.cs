#if TOOLS
using System;

[Tool]
public partial class PaddingPlugin : EditorPlugin
{
    private const int Padding = 8;
    private PaddingContextMenu _contextMenu = null!;

    public override void _EnterTree()
    {
        _contextMenu = new PaddingContextMenu { Padding = Padding };
        AddContextMenuPlugin(EditorContextMenuPlugin.ContextMenuSlot.Filesystem, _contextMenu);
    }

    public override void _ExitTree()
    {
        RemoveContextMenuPlugin(_contextMenu);
    }
}

[Tool]
public partial class PaddingContextMenu : EditorContextMenuPlugin
{
    public int Padding { get; set; } = 8;

    private PaddingInputDialog? _dialog;

    private void ShowCustomDialog(string[] paths)
    {
        if (_dialog == null)
        {
            _dialog = new PaddingInputDialog();
            EditorInterface.Singleton.GetBaseControl().AddChild(_dialog);
        }

        _dialog.Show(value => OnMenuItemSelected(PaddingMode.AddIfNeeded, paths, value));
    }

    public override void _PopupMenu(string[] paths)
    {
        var hasPng = false;
        foreach (var path in paths)
        {
            if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                hasPng = true;
                break;
            }
        }

        if (!hasPng) return;

        AddContextMenuItem($"Add Padding {Padding}px",
            Callable.From<string>((_) => OnMenuItemSelected(PaddingMode.AddIfNeeded, paths, Padding)));
        AddContextMenuItem($"Replace Padding {Padding}px",
            Callable.From<string>((_) => OnMenuItemSelected(PaddingMode.Force, paths, Padding)));
        AddContextMenuItem("Remove Padding",
            Callable.From<string>((_) => OnMenuItemSelected(PaddingMode.Remove, paths, 0)));
        AddContextMenuItem("Add Custom Padding",
            Callable.From<string>((_) => ShowCustomDialog(paths)));
    }

    private void OnMenuItemSelected(PaddingMode mode, string[] paths, int padding)
    {
        var processed = 0;
        var skipped = 0;

        foreach (var path in paths)
        {
            if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                continue;

            if (ProcessPng(path, mode, padding))
                processed++;
            else
                skipped++;
        }

        GD.Print($"Done! Processed: {processed}, Skipped: {skipped}");
    }

    private enum PaddingMode
    {
        AddIfNeeded,
        Force,
        Remove
    }

    private bool ProcessPng(string path, PaddingMode mode, int padding)
    {
        var texture = ResourceLoader.Load<CompressedTexture2D>(path);
        if (texture == null) return false;

        var image = texture.GetImage();
        if (image == null) return false;

        if (image.DetectAlpha() == Image.AlphaMode.None) return false;

        Image newImage;

        switch (mode)
        {
            case PaddingMode.AddIfNeeded:
                if (HasEnoughPadding(image, padding))
                {
                    GD.Print($"[ok]   {path}");
                    return false;
                }

                newImage = AddPadding(image, padding);
                break;

            case PaddingMode.Force:
                newImage = AddPadding(TrimPadding(image), padding);
                break;

            case PaddingMode.Remove:
                if (!HasAnyPadding(image))
                {
                    GD.Print($"[ok]   {path}");
                    return false;
                }

                newImage = TrimPadding(image);
                break;

            default:
                return false;
        }

        var originalImage = image;

        var action = new PaddingUndoRedoAction();
        action.Init(path, newImage, originalImage);

        var undoRedo = EditorInterface.Singleton.GetEditorUndoRedo();
        undoRedo.CreateAction($"Padding: {mode} {padding}px — {path.GetFile()}");
        undoRedo.AddDoMethod(action, PaddingUndoRedoAction.MethodName.Do);
        undoRedo.AddUndoMethod(action, PaddingUndoRedoAction.MethodName.Undo);
        undoRedo.CommitAction();

        return true;
    }

    private static void Reimport(string path)
    {
        var fs = EditorInterface.Singleton.GetResourceFilesystem();
        fs.UpdateFile(path);
        fs.ReimportFiles([path]);
        fs.Scan();
    }

    private static Image AddPadding(Image image, int padding)
    {
        var newImage = Image.CreateEmpty(
            image.GetWidth() + padding * 2,
            image.GetHeight() + padding * 2,
            false,
            Image.Format.Rgba8
        );

        newImage.BlitRect(
            image,
            new Rect2I(0, 0, image.GetWidth(), image.GetHeight()),
            new Vector2I(padding, padding)
        );

        return newImage;
    }

    private static Image TrimPadding(Image image)
    {
        image.Convert(Image.Format.Rgba8);
        var w = image.GetWidth();
        var h = image.GetHeight();

        var top = 0;
        var bottom = h - 1;
        var left = 0;
        var right = w - 1;

        for (var y = 0; y < h; y++)
        {
            var hasPixel = false;
            for (var x = 0; x < w; x++)
                if (image.GetPixel(x, y).A > 0)
                {
                    hasPixel = true;
                    break;
                }

            if (hasPixel)
            {
                top = y;
                break;
            }
        }

        for (var y = h - 1; y >= 0; y--)
        {
            var hasPixel = false;
            for (var x = 0; x < w; x++)
                if (image.GetPixel(x, y).A > 0)
                {
                    hasPixel = true;
                    break;
                }

            if (hasPixel)
            {
                bottom = y;
                break;
            }
        }

        for (var x = 0; x < w; x++)
        {
            var hasPixel = false;
            for (var y = 0; y < h; y++)
                if (image.GetPixel(x, y).A > 0)
                {
                    hasPixel = true;
                    break;
                }

            if (hasPixel)
            {
                left = x;
                break;
            }
        }

        for (var x = w - 1; x >= 0; x--)
        {
            var hasPixel = false;
            for (var y = 0; y < h; y++)
                if (image.GetPixel(x, y).A > 0)
                {
                    hasPixel = true;
                    break;
                }

            if (hasPixel)
            {
                right = x;
                break;
            }
        }

        var trimmed = Image.CreateEmpty(right - left + 1, bottom - top + 1, false, Image.Format.Rgba8);
        trimmed.BlitRect(image, new Rect2I(left, top, right - left + 1, bottom - top + 1), Vector2I.Zero);
        return trimmed;
    }

    private static bool HasEnoughPadding(Image image, int padding)
    {
        image.Convert(Image.Format.Rgba8);
        var w = image.GetWidth();
        var h = image.GetHeight();

        for (var x = 0; x < w; x++)
        for (var y = 0; y < padding; y++)
        {
            if (image.GetPixel(x, y).A > 0) return false;
            if (image.GetPixel(x, h - 1 - y).A > 0) return false;
        }

        for (var y = 0; y < h; y++)
        for (var x = 0; x < padding; x++)
        {
            if (image.GetPixel(x, y).A > 0) return false;
            if (image.GetPixel(w - 1 - x, y).A > 0) return false;
        }

        return true;
    }

    private static bool HasAnyPadding(Image image)
    {
        image.Convert(Image.Format.Rgba8);
        var w = image.GetWidth();
        var h = image.GetHeight();

        var topEmpty = true;
        for (var x = 0; x < w; x++)
            if (image.GetPixel(x, 0).A > 0)
            {
                topEmpty = false;
                break;
            }

        if (topEmpty) return true;

        var bottomEmpty = true;
        for (var x = 0; x < w; x++)
            if (image.GetPixel(x, h - 1).A > 0)
            {
                bottomEmpty = false;
                break;
            }

        if (bottomEmpty) return true;

        var leftEmpty = true;
        for (var y = 0; y < h; y++)
            if (image.GetPixel(0, y).A > 0)
            {
                leftEmpty = false;
                break;
            }

        if (leftEmpty) return true;

        var rightEmpty = true;
        for (var y = 0; y < h; y++)
            if (image.GetPixel(w - 1, y).A > 0)
            {
                rightEmpty = false;
                break;
            }

        return rightEmpty;
    }
}
#endif
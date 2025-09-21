using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace GenericModConfigMenu
{
    /// <summary>The API which lets other mods add a config UI through Generic Mod Config Menu.</summary>
    public interface IGenericModConfigMenuApi
    {
        /*********
        ** Methods
        *********/
        /****
        ** Must be called first
        ****/
        /// <summary>Register a mod whose config can be edited through the UI.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="reset">Reset the mod's config to its default values.</param>
        /// <param name="save">Save the mod's current config to the <c>config.json</c> file.</param>
        /// <param name="titleScreenOnly">Whether the options can only be edited from the title screen.</param>
        /// <remarks>Each mod can only be registered once, unless it's deleted via <see cref="Unregister"/> before calling this again.</remarks>
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);


        /****
        ** Basic options
        ****/
        /// <summary>Add a section title at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="text">The title text shown in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the title, or <c>null</c> to disable the tooltip.</param>
        void AddSectionTitle(IManifest mod, Func<string> text, Func<string> tooltip = null);

        /// <summary>Add a paragraph of text at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="text">The paragraph text to display.</param>
        void AddParagraph(IManifest mod, Func<string> text);

        /// <summary>Add an image at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="texture">The image texture to display.</param>
        /// <param name="texturePixelArea">The pixel area within the texture to display, or <c>null</c> to show the entire image.</param>
        /// <param name="scale">The zoom factor to apply to the image.</param>
        void AddImage(IManifest mod, Func<Texture2D> texture, Rectangle? texturePixelArea = null, int scale = Game1.pixelZoom);

        /// <summary>Add a boolean option at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);

        /// <summary>Add an integer option at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="min">The minimum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="max">The maximum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="interval">The interval of values that can be selected.</param>
        /// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
        void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, string fieldId = null);

        /// <summary>Add a float option at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="min">The minimum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="max">The maximum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="interval">The interval of values that can be selected.</param>
        /// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
        void AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name, Func<string> tooltip = null, float? min = null, float? max = null, float? interval = null, string fieldId = null);

        /// <summary>Add a string option at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="allowedValues">The values that can be selected, or <c>null</c> to allow any.</param>
        /// <param name="formatAllowedValue">Get the display text to show for a value from <paramref name="allowedValues"/>, or <c>null</c> to show the values as-is.</param>
        /// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
        void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name, Func<string> tooltip = null, string[] allowedValues = null, Func<string, string> formatAllowedValue = null, string fieldId = null);


        /****
        ** Multi-page management
        ****/
        /// <summary>Start a new page in the mod's config UI, or switch to that page if it already exists. All options registered after this will be part of that page.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="pageId">The unique page ID.</param>
        /// <param name="pageTitle">The page title shown in its UI, or <c>null</c> to show the <paramref name="pageId"/> value.</param>
        /// <remarks>You must also call <see cref="AddPageLink"/> to make the page accessible. This is only needed to set up a multi-page config UI. If you don't call this method, all options will be part of the mod's main config UI instead.</remarks>
        void AddPage(IManifest mod, string pageId, Func<string> pageTitle = null);

        /// <summary>Add a link to a page added via <see cref="AddPage"/> at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="pageId">The unique ID of the page to open when the link is clicked.</param>
        /// <param name="text">The link text shown in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the link, or <c>null</c> to disable the tooltip.</param>
        void AddPageLink(IManifest mod, string pageId, Func<string> text, Func<string> tooltip = null);


        /****
        ** Advanced
        ****/
        /// <summary>Add an option at the current position in the form using custom rendering logic.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="draw">Draw the option in the config UI. This is called with the sprite batch being rendered and the pixel position at which to start drawing.</param>
        /// <param name="saveChanges">Save the current value to the mod config.</param>
        /// <param name="height">The pixel height to allocate for the option in the form, or <c>null</c> for a standard input-sized option. This is called and cached each time the form is opened.</param>
        /// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
        /// <remarks>The custom logic represented by <paramref name="draw"/> and <paramref name="saveChanges"/> is responsible for managing its own state if needed. For example, you can store state in a static field or use closures to use a state variable.</remarks>
        void AddComplexOption(IManifest mod, Func<string> name, Func<string> tooltip, Action<SpriteBatch, Vector2> draw, Action saveChanges, Func<int> height = null, string fieldId = null);

        /// <summary>Remove a mod from the config UI and delete all its options and pages.</summary>
        /// <param name="mod">The mod's manifest.</param>
        void Unregister(IManifest mod);
    }
}

/// <summary>
/// Extra GMCM options mod API
/// </summary>
public interface IGMCMOptionsAPI
{
    /// <summary>Add a <c cref="Color">Color</c> option at the current position in the GMCM form.</summary>
    /// <param name="mod">The mod's manifest.</param>
    /// <param name="getValue">Get the current value from the mod config.</param>
    /// <param name="setValue">Set a new value in the mod config.</param>
    /// <param name="name">The label text to show in the form.</param>
    /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
    /// <param name="showAlpha">Whether the color picker should allow setting the Alpha channel</param>
    /// <param name="colorPickerStyle">Flags to control how the color picker is rendered.  <see cref="ColorPickerStyle"/></param>
    /// <param name="fieldId">The unique field ID for use with GMCM's <c>OnFieldChanged</c>, or <c>null</c> to auto-generate a randomized ID.</param>
    /// <param name="drawSample">
    ///   A function to draw a sample of the current color.  The arguments are the SpriteBatch, x and y coordinates
    ///   of the top left corner of the area in which to draw the sample, and the Color to render.
    ///   Passing <c>null</c> is equivalent to passing the result of <c>MakeColorSwatchDrawer()</c>.
    /// </param>
#nullable enable
    void AddColorOption(IManifest mod, Func<Color> getValue, Action<Color> setValue, Func<string> name,
        Func<string>? tooltip = null, bool showAlpha = true, uint colorPickerStyle = 0, string? fieldId = null,
        Action<SpriteBatch, int, int, Color>? drawSample = null);

        #pragma warning disable format
        /// <summary>
        /// Flags to control how the <c cref="ColorPickerOption">ColorPickerOption</c> widget is displayed.
        /// </summary>
        [Flags]
        public enum ColorPickerStyle : uint {
            Default = 0,
            RGBSliders    = 0b00000001,
            HSVColorWheel = 0b00000010,
            HSLColorWheel = 0b00000100,
            AllStyles     = 0b11111111,
            NoChooser     = 0,
            RadioChooser  = 0b01 << 8,
            ToggleChooser = 0b10 << 8
        }
        #pragma warning restore format

        /// <summary>
        ///   Return a function (suitable for passing as the <c>drawSample</c> parameter of <c>AddColorOption</c>)
        ///   that draws a color swatch.
        /// </summary>
        /// <param name="drawBackground">
        ///   A function that draws the background of the color swatch.  By default (i.e., if passed <c>null</c>),
        ///   this draws a black and white checkerboard pattern.
        /// </param>
        /// <param name="drawForeground">
        ///   A function that draws the foreground of the color swatch.  By default (i.e., if passed <c>null</c>),
        ///   this draws a square of the given Color.
        /// </param>
        /// <returns>A function that draws a color swatch</returns>
        Action<SpriteBatch, int, int, Color> MakeColorSwatchDrawer(
            Action<SpriteBatch, Rectangle>? drawBackground = null,
            Action<SpriteBatch, Rectangle, Color>? drawForeground = null);


    /// <summary>
    /// Add a horizontal separator.
    /// </summary>
    /// <param name="mod">The mod's manifest.</param>
    /// <param name="getWidthFraction">
    ///   A function that returns the fraction of the GMCM window that the separator
    ///   should occupy.  1.0 is the entire window.  Defaults to 0.85.
    /// </param>
    /// <param name="height">The height of the separator (in pixels)</param>
    /// <param name="padAbove">How much padding (in pixels) to place above the separator</param>
    /// <param name="padBelow">How much padding (in pixels) to place below the separator</param>
    /// <param name="alignment">
    ///   The horizontal alignment of the separator.
    ///   Use a value from the <c cref="HorizontalAlignment">HorizontalAlignment enumeration</c>.
    /// </param>
    /// <param name="getColor">
    ///   A function to return the color to use for the separator.  Defaults to the game's text color.
    /// </param>
    /// <param name="getShadowColor">
    ///   A function to return the color to use for the shadow drawn under the separator.  Defaults to the
    ///   game's text shadow color.  Return <c>Color.Transparent</c> to remove the shadow completely.
    /// </param>
    void AddHorizontalSeparator(IManifest mod,
                                Func<double>? getWidthFraction = null,
                                int height = 3,
                                int padAbove = 0,
                                int padBelow = 0,
                                int alignment = (int)HorizontalAlignment.Center,
                                Func<Color>? getColor = null,
                                Func<Color>? getShadowColor = null);

    /// <summary>
    /// Valid values for the <c>alignment</c> parameter of <c>AddHorizontalSeparator</c> and <c>AddSimpleHorizontalSeparator</c>
    /// </summary>
    public enum HorizontalAlignment
    {
        Left = -1,
        Center = 0,
        Right = 1
    }
#nullable disable
}



public interface IJsonAssetsApi
    {
        int GetObjectId(string name);
        int GetBigCraftableId(string name);
        void LoadAssets(string path);
    }
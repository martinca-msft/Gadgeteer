// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
namespace Gadgeteer.Modules
{
    using Color = Gadgeteer.Color;

    using System;
    using System.IO;
    using System.Threading;
    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;
    using Microsoft.SPOT.Presentation.Media;

    public partial class Module
    {
        /// <summary>
        /// Abstract class to provide common methods, properties, and events for a display device.
        /// </summary>
        public abstract class DisplayModule : Module
        {
            #region Private Fields

            // Simple graphics is created on demand by the SimpleGraphics property getter.
            // Once created, this cannot be released.
            private SimpleGraphicsInterface _simpleGraphics = null;

            // Display's model name.
            private string _displayModel = null;
            // Cached physical width of the display, ignoring the orientation setting.
            private int _width = 0;
            // Cached physical height of the display, ignoring the orientation setting.
            private int _height = 0;
            // Current orientation of the display. This could override the driver's opinion in the future.
            private DisplayOrientation _orientation = DisplayOrientation.Normal;
            // Timing requirements to be obeyed by the LCD controller.
            // This can be null which means the displays is not driven by the LCD controller.
            private TimingRequirements _timing = null;

            #endregion

            /// <summary>
            /// Initializes a new instance of the <see cref="DisplayModule" /> class.
            /// </summary>
            protected DisplayModule()
            {

            }

            // The only abstract member: does the actual painting to the hardware.
            // Displays using LCD controller should be just able to call bitmap.Flush(x, y, width, height).
            // If the display does not support partial updates, the area parameters can be safely ignored.
            // The bitmap is always guaranteed to contain complete screen. 

            /// <summary>
            /// When overridden in a derived class, renders display data on the display device.
            /// </summary>
            /// <param name="bitmap">The <see cref="Bitmap"/> object to render on the display.</param>
            /// <param name="x">The top left corner of area that was invalidated since last call.</param>
            /// <param name="y">The top left corner of area that was invalidated since last call.</param>
            /// <param name="width">Width of the area invalidated since last call.</param>
            /// <param name="height">Height of the area invalidated since last call.</param>
            protected abstract void Paint(Bitmap bitmap, int x, int y, int width, int height);

            /// <summary>
            /// Call when display is initialized or connected.
            /// </summary>
            /// <param name="displayModel">Model name of the connected display.</param>
            /// <param name="width">Display physical width in pixels, ignoring the orientation setting.</param>
            /// <param name="height">Display physical height in lines, ignoring the orientation setting.</param>
            /// <param name="orientation">Display orientation.</param>
            /// <param name="timing">The required timings from an LCD controller.</param>
            protected bool OnDisplayConnected(string displayModel, int width, int height, DisplayOrientation orientation, TimingRequirements timing)
            {
                bool success = true;

                if (displayModel == null)
                    displayModel = GetType().Name;

                if (timing != null)
                {
                    try
                    {
                        // should restart if needed
                        Mainboard.OnOnboardControllerDisplayConnected(displayModel, width, height, (int)orientation, timing);
                    }
                    catch (Exception e)
                    {
                        ErrorPrint("A display '" + displayModel + "' with resolution " + width + "�" + height + " and " + orientation + " orientation was connected. However, the mainboard is unable to fulfil its requirements. " + e.Message);

                        // We still want to update properties to the new values - the display is actually connected.
                        // Just make a note in case the driver was interested how we are doing.
                        success = false;
                    }
                }

                _displayModel = displayModel;
                _timing = timing;

                if (width != _width || height != _height || (_orientation != orientation && width != height))
                {
                    _width = width;
                    _height = height;
                    _orientation = orientation;

                    if (_simpleGraphics != null)
                        _simpleGraphics.OnScreenInvalidated();
                }
                else
                {
                    _orientation = orientation;
                }

                return success;
            }

            /// <summary>
            /// Call when display is disconnected.
            /// </summary>
            protected void OnDisplayDisconnected()
            {
                if (_timing != null)
                    Mainboard.OnOnboardControllerDisplayDisconnected();
            }

            /// <summary>
            /// Gets the name of the display model.
            /// </summary>
            public string DisplayModel
            {
                get { return _displayModel; }
            }

            /// <summary>
            /// Gets the physical width of the display module in pixels, ignoring the orientation setting.
            /// </summary>
            public int Width
            {
                get { return _width; }
            }

            /// <summary>
            /// Gets the physical height of the display module in lines, ignoring the orientation setting.
            /// </summary>
            public int Height
            {
                get { return _height; }
            }

            /// <summary>
            /// Gets or sets the orientation of the display.
            /// </summary>
            public DisplayOrientation Orientation
            {
                get { return _orientation; }
                set
                {
                    if (_timing != null)
                    {
                        DisplayOrientation currentOrientation = _orientation;

                        if (!OnDisplayConnected(_displayModel, _width, _height, value, _timing))
                        {
                            OnDisplayConnected(_displayModel, _width, _height, currentOrientation, _timing);

                            // Even if the orientation recovery failed, we are back with the properties we started with,
                            // like this request never happened. Error message should already printed, we can exit.
                        }

                        return;
                    }

                    // The current model does not offer any way how to detect driver's orientation changing capabilities,
                    // so it is unfair to throw exceptions now. This may change in future versions, though.

                    if (SupportsOrientationOverride(value))
                    {
                        try { SetOrientationOverride(value); }
                        catch (Exception e) { ErrorPrint("The orientation could not be changed. " + e.Message); }

                        return;
                    }

                    ErrorPrint("This display does not support changing orientation.");
                }
            }

            /// <summary>
            /// When overriden in a derived class, returns whether the display driver supports changing orientation of the display.
            /// </summary>
            /// <param name="orientation">The orientation whose availability to check.</param>
            /// <returns><b>true</b> if the driver supports <paramref name="orientation"/>; <b>false</b> otherwise.</returns>
            protected virtual bool SupportsOrientationOverride(DisplayOrientation orientation)
            {
                return orientation == _orientation;
            }

            /// <summary>
            /// When overriden in a derived class, sets orientation of the display device. If the orientation is different than the current one,
            /// this must either result in <see cref="OnDisplayConnected" /> being called or an exception being thrown.
            /// </summary>
            /// <param name="orientation">The orientation to set.</param>
            protected virtual void SetOrientationOverride(DisplayOrientation orientation)
            {
                if (orientation != _orientation) throw new NotSupportedException();
            }

            /// <summary>
            /// Gets the <see cref="SimpleGraphicsInterface" /> for this display module.
            /// </summary>
            public SimpleGraphicsInterface SimpleGraphics
            {
                get
                {
                    if (_simpleGraphics == null)
                        _simpleGraphics = new SimpleGraphicsInterface(this);

                    return _simpleGraphics;
                }
            }

            #region Enumerations

            /// <summary>
            /// An enumeration of possible LCD rotations.
            /// </summary>
            public enum DisplayOrientation
            {
                /// <summary>
                /// Normal way up.
                /// </summary>
                Normal = 0,

                /// <summary>
                /// Rotated clockwise by 90 degrees.
                /// </summary>
                Clockwise90Degrees = 90,

                /// <summary>
                /// Upside-down
                /// </summary>
                UpsideDown = 180,

                /// <summary>
                /// Rotated counterclockwise by 90 degrees.
                /// </summary>
                Counterclockwise90Degrees = 270
            }

            /// <summary>
            /// Represents the timing requirements that an LCD controller must obey.
            /// </summary>
            public class TimingRequirements
            {
                /// <summary>
                /// The maximum clock speed in Hz that the display supports.
                /// </summary>
                public uint MaximumClockSpeed;
                /// <summary>
                /// <b>true</b> if the display uses single pin for synchronization; <b>false</b> if it uses horizontal and vertical synchronization pulses.
                /// </summary>
                public bool UsesCommonSyncPin;
                /// <summary>
                /// Determines whether the common synchronization pulses are active high or low. Not applicable if <see cref="UsesCommonSyncPin" /> is <b>false</b>.
                /// </summary>
                public bool CommonSyncPinIsActiveHigh;
                /// <summary>
                /// <b>true</b> if high pixel data means the pixel is on; <b>false</b> if high pixel data means the pixel is off.
                /// </summary>
                public bool PixelDataIsActiveHigh;
                /// <summary>
                /// <b>true</b> if pixel data is valid on pixel clock�s rising edge; <b>false</b> if pixel data is valid on the falling edge.
                /// </summary>
                public bool PixelDataIsValidOnClockRisingEdge;
                /// <summary>
                /// Determines whether the horizontal synchronization pulse is active high or low. Not applicable if <see cref="UsesCommonSyncPin" /> is <b>true</b>.
                /// </summary>
                public bool HorizontalSyncPulseIsActiveHigh;
                /// <summary>
                /// Duration of the horizontal synchronization pulse, in pixel clock cycles.
                /// </summary>
                public byte HorizontalSyncPulseWidth;
                /// <summary>
                /// Number of pixel clock cycles on each line, before line data begin.
                /// </summary>
                public byte HorizontalBackPorch;
                /// <summary>
                /// Number of pixel clock cycles on each line, after line data end.
                /// </summary>
                public byte HorizontalFrontPorch;
                /// <summary>
                /// Determines whether the vertical synchronization pulse is active high or low. Not applicable if <see cref="UsesCommonSyncPin" /> is <b>true</b>.
                /// </summary>
                public bool VerticalSyncPulseIsActiveHigh;
                /// <summary>
                /// Duration of vertical synchronization pulse, in lines.
                /// </summary>
                public byte VerticalSyncPulseWidth;
                /// <summary>
                /// Number of lines in each frame, before first line.
                /// </summary>
                public byte VerticalBackPorch;
                /// <summary>
                /// Number of lines in each frame, after last line.
                /// </summary>
                public byte VerticalFrontPorch;
            }

            #endregion

            #region Simple Graphics

            /// <summary>
            /// Provides access to display tasks such as displaying text, simple shapes, and images.
            /// </summary>
            /// <remarks>
            /// <para>
            ///  A <see cref="SimpleGraphicsInterface" /> enables you to perform simple display tasks
            ///  such as displaying text, simple shapes, and images.
            /// </para>
            /// <para>
            ///  You cannot create a <see cref="SimpleGraphicsInterface" /> directly. Access this object 
            ///  via the <see cref="SimpleGraphics" /> property of a <see cref="DisplayModule" /> object.
            /// </para>
            /// </remarks>
            public class SimpleGraphicsInterface : IDisposable
            {
                private DisplayModule _displayModule;
                private Color _backgroundColor;
                private bool _autoRedraw;
                private bool _redrawAll;

                private Bitmap _screen;
                private int _width;
                private int _height;

                internal SimpleGraphicsInterface(DisplayModule displayModule)
                {
                    _displayModule = displayModule;
                    _backgroundColor = Color.Black;
                    _autoRedraw = true;

                    OnScreenInvalidated();
                    Clear();
                }

                /// <summary>
                /// Gets or sets a value that indicates whether the display should automatcially refresh
                /// after it has been issued a drawing command.
                /// </summary>
                /// <remarks>
                /// Set this property to <b>false</b> and use the <see cref="Redraw"/> method to improve
                /// performance when displaying mutiple shapes or lines of text at the same time. When 
                /// <see cref="AutoRedraw"/> is <b>false</b>, display operations do not render until
                /// you call the <see cref="Redraw"/> method. 
                /// </remarks>
                public bool AutoRedraw
                {
                    get { return _autoRedraw; }
                    set { _autoRedraw = value; }
                }

                /// <summary>
                /// Gets the height of this display.
                /// </summary>
                public int Height { get { return _height; } }

                /// <summary>
                /// Gets the width of this display.
                /// </summary>
                public int Width { get { return _width; } }

                /// <summary>
                /// Redraws the display.
                /// </summary>
                /// <remarks>
                /// Use this method in conjunction with the <see cref="AutoRedraw"/> property to improve
                /// performance when displaying mutiple shapes or lines of text at the same time. When 
                /// <see cref="AutoRedraw"/> is <b>false</b>, display operations do not render until
                /// you call this method. 
                /// </remarks>
                public void Redraw()
                {
                    _displayModule.Paint(_screen, 0, 0, _width, _height);
                    _redrawAll = false;
                }

                /// <summary>
                /// Clears the display, but does not redraw it.
                /// </summary>
                /// <seealso cref="Redraw"/>
                /// <seealso cref="AutoRedraw"/>
                public void ClearNoRedraw()
                {
                    _screen.DrawRectangle(_backgroundColor, 0, 0, 0, _width, _height, 0, 0, _backgroundColor, 0, 0, _backgroundColor, 0, 0, Bitmap.OpacityOpaque);
                    _redrawAll = true;
                }

                /// <summary>
                /// Clears the display and (if <see cref="AutoRedraw"/> is <b>true</b>), redraws it.
                /// </summary>
                /// <seealso cref="ClearNoRedraw"/>
                /// <seealso cref="Redraw"/>
                /// <seealso cref="AutoRedraw"/>
                public void Clear()
                {
                    _screen.DrawRectangle(_backgroundColor, 0, 0, 0, _width, _height, 0, 0, _backgroundColor, 0, 0, _backgroundColor, 0, 0, Bitmap.OpacityOpaque);

                    if (_autoRedraw)
                    {
                        _displayModule.Paint(_screen, 0, 0, _width, _height);
                        _redrawAll = false;
                    }
                }

                /// <summary>
                /// Gets or sets the background color for the display.
                /// </summary>
                public Color BackgroundColor
                {
                    get { return _backgroundColor; }
                    set
                    {
                        _backgroundColor = value;

                        if (_autoRedraw) Clear();
                    }
                }

                /// <summary>
                /// Displays the specified text.
                /// </summary>
                /// <param name="text">The text to display.</param>
                /// <param name="font">The font to use for the text display.</param>
                /// <param name="color">The color of the text.</param>
                /// <param name="x">The X coordinate to begin the text display.</param>
                /// <param name="y">The Y coordinate to begin the text display.</param>
                /// <remarks>
                /// This method displays text at the specified screen location. 
                /// If the text is too long for the display, it will be clipped.
                /// </remarks>
                public void DisplayText(string text, Font font, Color color, int x, int y)
                {
                    _screen.DrawText(text, font, color, x, y);

                    if (_autoRedraw)
                    {
                        if (_redrawAll)
                        {
                            _displayModule.Paint(_screen, 0, 0, _width, _height);
                            _redrawAll = false;
                        }
                        else
                        {
                            int right, bottom;
                            font.ComputeExtent(text, out right, out bottom);

                            right += x;
                            bottom += y;

                            if (right >= _width) right = _width - 1;
                            if (bottom >= _height) bottom = _height - 1;

                            if (x < 0) x = 0;
                            if (y < 0) y = 0;

                            if (right >= 0 && bottom >= 0 && x < _width && y < _height)
                                _displayModule.Paint(_screen, x, y, right - x + 1, bottom - y + 1);
                        }
                    }
                }

                /// <summary>
                /// Provides an enumeration that specifies how text will be aligned.
                /// </summary>
                /// <remarks>
                /// The values of this enumeration are used when calling 
                /// the <see cref="DisplayTextInRectangle(string, int, int, int, int, Color, Font, TextAlign, WordWrap, Trimming, ScaleText)">DisplayTextInRectangle</see> method.
                /// </remarks>
                public enum TextAlign
                {
                    /// <summary>
                    /// The text is aligned in the center of the specified rectangular region.
                    /// </summary>
                    Center,
                    /// <summary>
                    /// The text is aligned on the left of the specified rectangular region.
                    /// </summary>
                    Left,
                    /// <summary>
                    /// The text is aligned on the right of the specified rectangular region.
                    /// </summary>
                    Right
                }

                /// <summary>
                /// Provides an enumeration that specifies how text will be trimmed.
                /// </summary>
                /// <remarks>
                /// The values of this enumeration are used when calling 
                /// the <see cref="DisplayTextInRectangle(string, int, int, int, int, Color, Font, TextAlign, WordWrap, Trimming, ScaleText)">DisplayTextInRectangle</see> method.
                /// </remarks>
                public enum Trimming
                {
                    /// <summary>
                    /// Trimming occurs a word boundary, followed by an ellipsis mark.
                    /// </summary>
                    WordEllipsis,
                    /// <summary>
                    /// Trimming occurs a character boundary, followed by an ellipsis mark.
                    /// </summary>
                    CharacterEllipsis
                }

                /// <summary>
                /// Provides an enumeration that specifies how text will be wrapped.
                /// </summary>
                /// <remarks>
                /// The values of this enumeration are used when calling 
                /// the <see cref="DisplayTextInRectangle(string, int, int, int, int, Color, Font, TextAlign, WordWrap, Trimming, ScaleText)">DisplayTextInRectangle</see> method.
                /// </remarks>
                public enum WordWrap
                {
                    /// <summary>
                    /// Do not use word wrap.
                    /// </summary>
                    None,
                    /// <summary>
                    /// Use word wrap.
                    /// </summary>
                    Wrap
                }

                /// <summary>
                /// Provides an enumeration that specifies how text will be scaled.
                /// </summary>
                /// <remarks>
                /// The values of this enumeration are used when calling 
                /// the <see cref="DisplayTextInRectangle(string, int, int, int, int, Color, Font, TextAlign, WordWrap, Trimming, ScaleText)">DisplayTextInRectangle</see> method.
                /// </remarks>
                public enum ScaleText
                {
                    /// <summary>
                    /// If neccessary, the height of the rectangular region will be expanded to accomodate the text.
                    /// </summary>
                    None,
                    /// <summary>
                    /// The height of the rectangular region will not be expanded to accomodate text that is too long.
                    /// In this case, the value of the <see cref="Trimming"/> enumeration determines how the excess text
                    /// is treated.
                    /// </summary>
                    ScaleToFit
                }

                /// <summary>
                /// Displays the specified text within the specified rectangular region.
                /// </summary>
                /// <param name="text">The text to display.</param>
                /// <param name="x">The X coordinate of the rectangular region.</param>
                /// <param name="y">The Y coordinate of the rectangular region.</param>
                /// <param name="width">The width of the rectangular region.</param>
                /// <param name="height">The height of the rectangular region.</param>
                /// <param name="color">The text color.</param>
                /// <param name="font">The text font.</param>
                /// <param name="textAlignment">A value from the <see cref="TextAlign"/> enumeration that specifies how to align the text within the rectangular region.</param>
                /// <param name="wordWrap">A value from the <see cref="WordWrap"/> enumeration that specifies how to wrap the text within the rectangular region.</param>
                /// <param name="trimming">A value from the <see cref="Trimming"/> enumeration that specifies how to trim excess text.</param>
                /// <param name="scaleTextToFit">A value from the <see cref="ScaleText"/> enumeration that specifies how the text should be scaled.</param>
                public void DisplayTextInRectangle(string text, int x, int y, int width, int height,
                    Color color, Font font, TextAlign textAlignment, WordWrap wordWrap, Trimming trimming, ScaleText scaleTextToFit)
                {
                    uint dtFlags = Bitmap.DT_None;

                    switch (textAlignment)
                    {
                        case TextAlign.Center:
                            dtFlags |= Bitmap.DT_AlignmentCenter;
                            break;
                        case TextAlign.Left:
                            dtFlags |= Bitmap.DT_AlignmentLeft;
                            break;
                        case TextAlign.Right:
                            dtFlags |= Bitmap.DT_AlignmentRight;
                            break;
                        default:
                            break;
                    }

                    switch (trimming)
                    {
                        case Trimming.CharacterEllipsis:
                            dtFlags |= Bitmap.DT_TrimmingCharacterEllipsis;
                            break;
                        case Trimming.WordEllipsis:
                            dtFlags |= Bitmap.DT_TrimmingWordEllipsis;
                            break;
                        default:
                            break;
                    }


                    if (wordWrap == WordWrap.Wrap)
                        dtFlags |= Bitmap.DT_WordWrap;

                    if (scaleTextToFit == ScaleText.None)
                        dtFlags |= Bitmap.DT_IgnoreHeight;

                    _screen.DrawTextInRect(text, x, y, width, height, dtFlags, color, font);

                    if (_autoRedraw)
                    {
                        if (_redrawAll)
                        {
                            _displayModule.Paint(_screen, 0, 0, _width, _height);
                            _redrawAll = false;
                        }
                        else
                        {
                            int right, bottom;
                            font.ComputeTextInRect(text, out right, out bottom, 0, 0, width, height, dtFlags);

                            right += x;
                            bottom += y;

                            if (right >= _width) right = _width - 1;
                            if (bottom >= _height) bottom = _height - 1;

                            if (x < 0) x = 0;
                            if (y < 0) y = 0;

                            if (right >= 0 && bottom >= 0 && x < _width && y < _height)
                                _displayModule.Paint(_screen, x, y, right - x + 1, bottom - y + 1);
                        }
                    }
                }

                /// <summary>
                /// Displays the specified text within the specified rectangular region.
                /// </summary>
                /// <param name="text">The text to display.</param>
                /// <param name="x">The X coordinate of the rectangular region.</param>
                /// <param name="y">The Y coordinate of the rectangular region.</param>
                /// <param name="width">The width of the rectangular region.</param>
                /// <param name="height">The height of the rectangular region.</param>
                /// <param name="color">The text color.</param>
                /// <param name="font">The text font.</param>
                /// <param name="textAlignment">A value from the <see cref="TextAlign"/> enumeration that specifies how to align the text within the rectangular region.</param>
                public void DisplayTextInRectangle(string text, int x, int y, int width, int height, Color color, Font font, TextAlign textAlignment)
                {
                    DisplayTextInRectangle(text, x, y, width, height, color, font, textAlignment, WordWrap.None, Trimming.WordEllipsis, ScaleText.None);
                }

                /// <summary>
                /// Displays the specified text within the specified rectangular region.
                /// </summary>
                /// <param name="text">The text to display.</param>
                /// <param name="x">The X coordinate of the rectangular region.</param>
                /// <param name="y">The Y coordinate of the rectangular region.</param>
                /// <param name="width">The width of the rectangular region.</param>
                /// <param name="height">The height of the rectangular region.</param>
                /// <param name="color">The text color.</param>
                /// <param name="font">The text font.</param>
                public void DisplayTextInRectangle(string text, int x, int y, int width, int height, Color color, Font font)
                {
                    DisplayTextInRectangle(text, x, y, width, height, color, font, TextAlign.Left, WordWrap.None, Trimming.WordEllipsis, ScaleText.None);
                }

                /// <summary>
                /// Displays an ellipse.
                /// </summary>
                /// <param name="outlineColor">The color of the ellipse outline.</param>
                /// <param name="thicknessOutline">The thickness of the outline.</param>
                /// <param name="x">The X coordinate of the center of the ellipse.</param>
                /// <param name="y">The Y coordinate of the center of the ellipse.</param>
                /// <param name="xRadius">The radius value for Y.</param>
                /// <param name="yRadius">The radius value for X.</param>
                /// <param name="colorGradientStart">The color to begin the background gradient.</param>
                /// <param name="xGradientStart">The X coordinate to begin the background gradient.</param>
                /// <param name="yGradientStart">The Y coordinate to begin the background gradient.</param>
                /// <param name="colorGradientEnd">The color to end the background gradient.</param>
                /// <param name="xGradientEnd">The X coordinate to end the background gradient.</param>
                /// <param name="yGradientEnd">The Y coordinate to end the background gradient.</param>
                /// <param name="opacity">The opacity of the ellipse, 0 (transparent)..256 (opaque).</param>
                public void DisplayEllipse(Color outlineColor, int thicknessOutline, int x, int y, int xRadius, int yRadius,
                            Color colorGradientStart, int xGradientStart, int yGradientStart,
                            Color colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity)
                {
                    _screen.DrawEllipse(outlineColor, thicknessOutline, x, y, xRadius, yRadius,
                        colorGradientStart, xGradientStart, yGradientStart, colorGradientEnd, xGradientEnd, yGradientEnd, opacity);

                    if (_autoRedraw)
                    {
                        if (_redrawAll)
                        {
                            _displayModule.Paint(_screen, 0, 0, _width, _height);
                            _redrawAll = false;
                        }
                        else
                        {
                            int left = x - xRadius;
                            int top = y - yRadius;
                            int right = x + xRadius;
                            int bottom = y + yRadius;

                            if (thicknessOutline > 1)
                            {
                                left -= thicknessOutline;
                                top -= thicknessOutline;
                                right += thicknessOutline - 1;
                                bottom += thicknessOutline;
                            }

                            if (left < 0) left = 0;
                            if (top < 0) top = 0;
                            if (right >= _width) right = _width - 1;
                            if (bottom >= _height) bottom = _height - 1;

                            if (right >= 0 && bottom >= 0 && left < _width && top < _height)
                                _displayModule.Paint(_screen, left, top, right - left + 1, bottom - top + 1);
                        }
                    }
                }

                /// <summary>
                /// Displays an ellipse.
                /// </summary>
                /// <param name="outlineColor">The color of the ellipse outline.</param>
                /// <param name="thicknessOutline">The thickness of the outline.</param>
                /// <param name="fillColor">The color of the ellipse fill.</param>
                /// <param name="x">The X coordinate of the center of the ellipse.</param>
                /// <param name="y">The Y coordinate of the center of the ellipse.</param>
                /// <param name="xRadius">The radius value for Y.</param>
                /// <param name="yRadius">The radius value for X.</param>
                public void DisplayEllipse(Color outlineColor, int thicknessOutline, Color fillColor, int x, int y, int xRadius, int yRadius)
                {
                    DisplayEllipse(outlineColor, thicknessOutline, x, y, xRadius, yRadius, fillColor, 0, 0, fillColor, 0, 0, Bitmap.OpacityOpaque);
                }

                /// <summary>
                /// Displays an ellipse.
                /// </summary>
                /// <param name="outlineColor">The color of the ellipse outline.</param>
                /// <param name="thicknessOutline">The thickness of the outline.</param>
                /// <param name="fillColor">The color of the ellipse fill.</param>
                /// <param name="x">The X coordinate of the center of the ellipse.</param>
                /// <param name="y">The Y coordinate of the center of the ellipse.</param>
                /// <param name="xRadius">The radius value for Y.</param>
                /// <param name="yRadius">The radius value for X.</param>
                /// <param name="opacity">The opacity of the ellipse, 0 (transparent)..256 (opaque).</param>
                public void DisplayEllipse(Color outlineColor, int thicknessOutline, Color fillColor, int x, int y, int xRadius, int yRadius, ushort opacity)
                {
                    DisplayEllipse(outlineColor, thicknessOutline, x, y, xRadius, yRadius, fillColor, 0, 0, fillColor, 0, 0, opacity);
                }

                /// <summary>
                /// Displays a rectangle.
                /// </summary>
                /// <param name="outlineColor">The color for the outline of the rectangle.</param>
                /// <param name="thicknessOutline">The thickness of the outline.</param>
                /// <param name="x">The X coordinate for the top left corner of the rectangle.</param>
                /// <param name="y">The Y coordinate for the top left corner of the rectangle.</param>
                /// <param name="width">The width of the rectangle.</param>
                /// <param name="height">The height of the rectangle.</param>
                /// <param name="xCornerRadius">The X dimension corner radius, or zero for none.</param>
                /// <param name="yCornerRadius">The Y dimension corner radius, or zero for none.</param>
                /// <param name="colorGradientStart">The color to begin the background gradient.</param>
                /// <param name="xGradientStart">The X coordinate to begin the background gradient.</param>
                /// <param name="yGradientStart">The Y coordinate to begin the background gradient.</param>
                /// <param name="colorGradientEnd">The color to end the background gradient.</param>
                /// <param name="xGradientEnd">The X coordinate to end the background gradient.</param>
                /// <param name="yGradientEnd">The Y coordinate to end the background gradient.</param>
                /// <param name="opacity">The opacity of the rectangle, 0 (transparent)..256 (opaque).</param>
                public void DisplayRectangle(Color outlineColor, int thicknessOutline, int x, int y, int width, int height,
                    int xCornerRadius, int yCornerRadius, Color colorGradientStart, int xGradientStart, int yGradientStart,
                    Color colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity)
                {
                    _screen.DrawRectangle(outlineColor, thicknessOutline, x, y, width, height, xCornerRadius, yCornerRadius,
                        colorGradientStart, xGradientStart, yGradientStart, colorGradientEnd, xGradientEnd, yGradientEnd, opacity);

                    if (_autoRedraw)
                    {
                        if (_redrawAll)
                        {
                            _displayModule.Paint(_screen, 0, 0, _width, _height);
                            _redrawAll = false;
                        }
                        else
                        {
                            int right = x + width - 1;
                            int bottom = y + height - 1;

                            if (thicknessOutline > 1)
                            {
                                int offset = thicknessOutline / 2;
                                x -= offset;
                                y -= offset;
                                right += offset;
                                bottom += offset;
                            }

                            if (right >= _width) right = _width - 1;
                            if (bottom >= _height) bottom = _height - 1;

                            if (x < 0) x = 0;
                            if (y < 0) y = 0;

                            if (right >= 0 && bottom >= 0 && x < _width && y < _height)
                                _displayModule.Paint(_screen, x, y, right - x + 1, bottom - y + 1);
                        }
                    }
                }

                /// <summary>
                /// Displays a rectangle.
                /// </summary>
                /// <param name="outlineColor">The color for the outline of the rectangle.</param>
                /// <param name="thicknessOutline">The thickness of the outline.</param>
                /// <param name="fillColor">The color to fill the rectangle with.</param>
                /// <param name="x">The X coordinate for the top left corner of the rectangle.</param>
                /// <param name="y">The Y coordinate for the top left corner of the rectangle.</param>
                /// <param name="width">The width of the rectangle.</param>
                /// <param name="height">The height of the rectangle.</param>
                public void DisplayRectangle(Color outlineColor, int thicknessOutline, Color fillColor, int x, int y, int width, int height)
                {
                    DisplayRectangle(outlineColor, thicknessOutline, x, y, width, height, 0, 0, fillColor, 0, 0, fillColor, 0, 0, Bitmap.OpacityOpaque);
                }

                /// <summary>
                /// Displays a rectangle.
                /// </summary>
                /// <param name="outlineColor">The color for the outline of the rectangle.</param>
                /// <param name="thicknessOutline">The thickness of the outline.</param>
                /// <param name="fillColor">The color to fill the rectangle with.</param>
                /// <param name="x">The X coordinate for the top left corner of the rectangle.</param>
                /// <param name="y">The Y coordinate for the top left corner of the rectangle.</param>
                /// <param name="width">The width of the rectangle.</param>
                /// <param name="height">The height of the rectangle.</param>
                /// <param name="opacity">The opacity of the rectangle, 0 (transparent)..256 (opaque).</param>
                public void DisplayRectangle(Color outlineColor, int thicknessOutline, Color fillColor, int x, int y, int width, int height, ushort opacity)
                {
                    DisplayRectangle(outlineColor, thicknessOutline, x, y, width, height, 0, 0, fillColor, 0, 0, fillColor, 0, 0, opacity);
                }

                /// <summary>
                /// Displays a rectangle.
                /// </summary>
                /// <param name="outlineColor">The color for the outline of the rectangle.</param>
                /// <param name="thicknessOutline">The thickness of the outline.</param>
                /// <param name="fillColor">The color to fill the rectangle with.</param>
                /// <param name="x">The X coordinate for the top left corner of the rectangle.</param>
                /// <param name="y">The Y coordinate for the top left corner of the rectangle.</param>
                /// <param name="width">The width of the rectangle.</param>
                /// <param name="height">The height of the rectangle.</param>
                /// <param name="cornerRadius">The corner radius to be applied in both the X and Y dimensions.</param>
                /// <param name="opacity">The opacity of the rectangle, 0 (transparent)..256 (opaque).</param>
                public void DisplayRectangle(Color outlineColor, int thicknessOutline, Color fillColor, int x, int y, int width, int height, int cornerRadius, ushort opacity)
                {
                    DisplayRectangle(outlineColor, thicknessOutline, x, y, width, height, cornerRadius, cornerRadius, fillColor, 0, 0, fillColor, 0, 0, opacity);
                }

                /// <summary>
                /// Displays a line.
                /// </summary>
                /// <param name="color">The color of the line.</param>
                /// <param name="thickness">The thickness of the line.</param>
                /// <param name="x0">The X coordinate of the starting point of the line.</param>
                /// <param name="y0">The Y coordinate of the starting point of the line.</param>
                /// <param name="x1">The X coordinate of the ending point of the line.</param>
                /// <param name="y1">The Y coordinate of the ending point of the line.</param>
                public void DisplayLine(Color color, int thickness, int x0, int y0, int x1, int y1)
                {
                    _screen.DrawLine(color, thickness, x0, y0, x1, y1);

                    if (_autoRedraw)
                    {
                        if (_redrawAll)
                        {
                            _displayModule.Paint(_screen, 0, 0, _width, _height);
                            _redrawAll = false;
                        }
                        else
                        {
                            int top = y0;
                            int left = x0;
                            int right = x1;
                            int bottom = y1;

                            if (y0 > y1) { top = y1; bottom = y0; }
                            if (x0 > x1) { left = x1; right = x0; }

                            if (thickness > 1)
                            {
                                top -= thickness;
                                left -= thickness;
                                right += thickness - 1; // This is what works in testing!
                                bottom += thickness;
                            }

                            if (top < 0) top = 0;
                            if (left < 0) left = 0;
                            if (right >= _width) right = _width - 1;
                            if (bottom >= _height) bottom = _height - 1;

                            _displayModule.Paint(_screen, left, top, right - left + 1, bottom - top + 1);
                        }
                    }
                }

                /// <summary>
                /// Retrieves the pixel color at a specified location on the display.
                /// </summary>
                /// <param name="x">The x-coordinate of the location where pixel color is to be retrieved.</param>
                /// <param name="y">The y-coordinate of the location where pixel color is to be retrieved.</param>
                /// <returns>The pixel color at the specified location.</returns>
                public Color GetPixel(int x, int y)
                {
                    return _screen.GetPixel(x, y);
                }

                /// <summary>
                /// Sets a specified pixel to the specified color.
                /// </summary>
                /// <param name="color">The color to set the pixel to.</param>
                /// <param name="x">The X coordinate of the pixel.</param>
                /// <param name="y">The Y coordinate of the pixel.</param>
                public void SetPixel(Color color, int x, int y)
                {
                    _screen.SetPixel(x, y, color);

                    if (_autoRedraw)
                    {
                        if (_redrawAll)
                        {
                            _displayModule.Paint(_screen, 0, 0, _width, _height);
                            _redrawAll = false;
                        }
                        else
                        {
                            if (x >= 0 && x < _width && y >= 0 && y < _height)
                                _displayModule.Paint(_screen, x, y, 1, 1);
                        }
                    }
                }

                /// <summary>
                /// Displays a <see cref="T:Microsoft.SPOT.Bitmap"/> image on the screen.
                /// </summary>
                /// <param name="bitmap">The <see cref="Microsoft.SPOT.Bitmap"/> object to display.</param>
                /// <param name="x">Horizontal position of the left edge of the bitmap on the display.</param>
                /// <param name="y">Vertical position of the top edge of the bitmap on the display.</param>
                /// <param name="xSrc">Source X coordinate. Use this parameter to specify cropping from the left edge of the image. Use 0 to display full image.</param>
                /// <param name="ySrc">Source Y coordinate. Use this parameter to specify cropping from the top edge of the image. Use 0 to display full image.</param>
                /// <param name="width">Source width. Use this parameter to specify cropping to the right of the image. Use bitmap.Width to display full image.</param>
                /// <param name="height">Source height. Use this parameter to specify cropping to the right of the image. Use bitmap.Height to display full image.</param>
                public void DisplayImage(Bitmap bitmap, int x, int y, int xSrc, int ySrc, int width, int height)
                {
                    _screen.DrawImage(x, y, bitmap, xSrc, ySrc, width, height);

                    if (_autoRedraw)
                    {
                        if (_redrawAll)
                        {
                            _displayModule.Paint(_screen, 0, 0, _width, _height);
                            _redrawAll = false;
                        }
                        else
                        {
                            int right = x + width;
                            int bottom = y + height;

                            if (right >= _width) right = _width - 1;
                            if (bottom >= _height) bottom = _height - 1;

                            if (x < 0) x = 0;
                            if (y < 0) y = 0;

                            if (right >= 0 && bottom >= 0 && x < _width && y < _height)
                                _displayModule.Paint(_screen, x, y, right - x + 1, bottom - y + 1);
                        }
                    }
                }

                /// <summary>
                /// Displays a <see cref="T:Microsoft.SPOT.Bitmap"/> image on the screen.
                /// </summary>
                /// <param name="bitmap">The <see cref="Microsoft.SPOT.Bitmap"/> object to display</param>
                /// <param name="x">Horizontal position of the left edge of the bitmap on the display.</param>
                /// <param name="y">Vertical position of the top edge of the bitmap on the display.</param>   
                public void DisplayImage(Bitmap bitmap, int x, int y)
                {
                    DisplayImage(bitmap, x, y, 0, 0, bitmap.Width, bitmap.Height);
                }

                /// <summary>
                /// Displays a <see cref="T:Gadgeteer.Picture"/> image on the screen.
                /// </summary>
                /// <param name="picture">The <see cref="T:Gadgeteer.Picture"/> to display.</param>
                /// <param name="x">Horizontal position of the left edge of the image on the display.</param>
                /// <param name="y">Vertical position of the top edge of the image on the display.</param>
                /// <param name="xSrc">Source X coordinate. Use this parameter to specify cropping from the left edge of the image. Use 0 to display full image.</param>
                /// <param name="ySrc">Source Y coordinate. Use this parameter to specify cropping from the top edge of the image. Use 0 to display full image.</param>
                /// <param name="width">Source width. Use this parameter to specify cropping to the right of the image.</param>
                /// <param name="height">Source height. Use this parameter to specify cropping to the right of the image.</param>
                public void DisplayImage(Picture picture, int x, int y, int xSrc, int ySrc, int width, int height)
                {
                    DisplayImage((Bitmap)picture, x, y, xSrc, ySrc, width, height);
                }

                /// <summary>
                /// Displays a <see cref="T:Gadgeteer.Picture"/> image on the screen.
                /// </summary>
                /// <param name="picture">The <see cref="T:Gadgeteer.Picture"/> to display.</param>
                /// <param name="x">Horizontal position of the left edge of the bitmap on the display.</param>
                /// <param name="y">Vertical position of the top edge of the bitmap on the display.</param>
                public void DisplayImage(Picture picture, int x, int y)
                {
                    Bitmap bitmap = (Bitmap)picture;
                    DisplayImage(bitmap, x, y, 0, 0, bitmap.Width, bitmap.Height);
                }

                /// <summary>
                ///  Displays an image from a file on the screen.
                /// </summary>
                /// <param name="filePath">The path to the image file.</param>
                /// <param name="imageType">The type of image contained in <paramref name="filePath"/>.</param>
                /// <param name="x">Horizontal position of the left edge of the image on the display.</param>
                /// <param name="y">Vertical position of the top edge of the image on the display.</param>
                /// <param name="xSrc">Source X coordinate. Use this parameter to specify cropping from the left edge of the image. Use 0 to display full image.</param>
                /// <param name="ySrc">Source Y coordinate. Use this parameter to specify cropping from the top edge of the image. Use 0 to display full image.</param>
                /// <param name="width">Source width. Use this parameter to specify cropping to the right of the image. Use bitmap.Width to display full image.</param>
                /// <param name="height">Source height. Use this parameter to specify cropping to the right of the image. Use bitmap.Height to display full image.</param>
                public void DisplayImage(string filePath, Bitmap.BitmapImageType imageType, int x, int y, int xSrc, int ySrc, int width, int height)
                {
                    Bitmap bitmap = new Bitmap(File.ReadAllBytes(filePath), imageType);
                    DisplayImage(bitmap, x, y, xSrc, xSrc, width, height);
                }

                /// <summary>
                ///  Displays an image from a file on the screen.
                /// </summary>
                /// <param name="filePath">The path to the image file.</param>
                /// <param name="imageType">The type of image contained in <paramref name="filePath"/>.</param>
                /// <param name="x">Horizontal position of the left edge of the image on the display.</param>
                /// <param name="y">Vertical position of the top edge of the image on the display.</param>
                public void DisplayImage(string filePath, Bitmap.BitmapImageType imageType, int x, int y)
                {
                    Bitmap bitmap = new Bitmap(File.ReadAllBytes(filePath), imageType);
                    DisplayImage(bitmap, x, y, 0, 0, bitmap.Width, bitmap.Height);
                }

                /// <summary>
                /// Causes to refresh the cached members of the <see cref="DisplayModule" />.
                /// Does not invoke <see cref="Redraw" />.
                /// </summary>
                internal void OnScreenInvalidated()
                {
                    _width = _displayModule._width;
                    _height = _displayModule._height;
                    _screen = new Bitmap(_width, _height);
                }

                // This was needed for WPF Window, kept for not breaking IDisposable contract.

                /// <summary>
                /// Detaches this interface from the display.
                /// </summary>
                void IDisposable.Dispose()
                {
                    _displayModule._simpleGraphics = null;
                }
            }

            #endregion
        }
    }
}
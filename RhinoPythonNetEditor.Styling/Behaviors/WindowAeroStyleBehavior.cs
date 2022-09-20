using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace RhinoPythonNetEditor.Styling.Behaviors
{
    public class WindowAeroStyleBehavior : Behavior<Window>
    {
        private Window window;
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        public static DependencyProperty EnabledProperty = DependencyProperty.Register("Enabled", typeof(bool), typeof(WindowAeroStyleBehavior), new PropertyMetadata(true, OnEnabledPropertyChanged));


        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        private static void OnEnabledPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is WindowAeroStyleBehavior behavior) behavior.window?.EnableBlur((bool)e.NewValue, (bool)e.NewValue);
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (window != null)
            {
                window.LocationChanged -= Window_LocationChanged;
                window.PreviewMouseUp -= Window_PreviewMouseUp;
            }
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        private   void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            window = Window.GetWindow(AssociatedObject);
            if (window != null)
            {
                window.LocationChanged += Window_LocationChanged;
                window.PreviewMouseUp += Window_PreviewMouseUp;
            }
            window?.EnableBlur(Enabled, Enabled);
        }

        private  void Window_LocationChanged(object sender, EventArgs e)
        {
            window?.EnableBlur(false, false);
        }

        private void Window_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            window?.EnableBlur(true, true);
        }

    }
    public static class WindowEffect
    {
        public static void EnableWindowAnime(this Window wnd)
        {
            var myHWND = new WindowInteropHelper(wnd).Handle;
            IntPtr myStyle = new IntPtr(WS.WS_CAPTION | WS.WS_CLIPCHILDREN | WS.WS_MINIMIZEBOX | WS.WS_MAXIMIZEBOX | WS.WS_SYSMENU | WS.WS_SIZEBOX);
            SetWindowLongPtr(new HandleRef(null, myHWND), GWL_STYLE, myStyle);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        public const int GWL_STYLE = -16;

        public static class WS
        {
            public static readonly long
            WS_BORDER = 0x00800000L,
            WS_CAPTION = 0x00C00000L,
            WS_CHILD = 0x40000000L,
            WS_CHILDWINDOW = 0x40000000L,
            WS_CLIPCHILDREN = 0x02000000L,
            WS_CLIPSIBLINGS = 0x04000000L,
            WS_DISABLED = 0x08000000L,
            WS_DLGFRAME = 0x00400000L,
            WS_GROUP = 0x00020000L,
            WS_HSCROLL = 0x00100000L,
            WS_ICONIC = 0x20000000L,
            WS_MAXIMIZE = 0x01000000L,
            WS_MAXIMIZEBOX = 0x00010000L,
            WS_MINIMIZE = 0x20000000L,
            WS_MINIMIZEBOX = 0x00020000L,
            WS_OVERLAPPED = 0x00000000L,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUP = 0x80000000L,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_SIZEBOX = 0x00040000L,
            WS_SYSMENU = 0x00080000L,
            WS_TABSTOP = 0x00010000L,
            WS_THICKFRAME = 0x00040000L,
            WS_TILED = 0x00000000L,
            WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_VISIBLE = 0x10000000L,
            WS_VSCROLL = 0x00200000L;
        }

        public static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
            {
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            }
            else
            {
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
            }
        }


    }
    public static class BlurEffect
    {
        public static void EnableBlur(this Window wnd, bool enabled, bool enabled1)
        {
            var windowHelper = new WindowInteropHelper(wnd);

            var accent = new AccentPolicy();
            accent.AccentState = enabled ? AccentState.ACCENT_ENABLE_BLURBEHIND : AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
            if (enabled1) accent.AccentFlags = 0x20 | 0x40 | 0x80 | 0x100;
            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
        internal enum AccentState
        {
            ACCENT_DISABLED = 1,
            ACCENT_ENABLE_GRADIENT = 2,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 0,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
    }
}

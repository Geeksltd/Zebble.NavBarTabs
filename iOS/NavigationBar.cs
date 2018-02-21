namespace Zebble
{
    using Foundation;
    using UIKit;

    public partial class NavigationBar
    {
        float? OrginalHeight;

        void FixIPhoneXLayout()
        {
            Thread.UI.Run(() =>
            {
                if (Device.OS.HardwareModel != "iPhone X") return;

                OnSafeAreaChanged();

                NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidChangeStatusBarOrientationNotification, (NSNotification n) => OnSafeAreaChanged());

                NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidChangeStatusBarFrameNotification, (NSNotification n) => OnSafeAreaChanged());
            });
        }

        void OnSafeAreaChanged()
        {
            var insets = UIApplication.SharedApplication.KeyWindow.SafeAreaInsets;

            if (insets.Top <= 0) return;

            var top = (float)insets.Top;
            if (OrginalHeight == null) OrginalHeight = Height.CurrentValue;

            this.Height(OrginalHeight + top).Padding(top: top);
            UIRuntime.RenderRoot.Padding(
                left: (float)insets.Left,
                right: (float)insets.Right,
                bottom: (float)insets.Bottom);
        }
    }
}
namespace Zebble
{
    using System;
    using System.Threading.Tasks;
    using Services;

    public abstract class NavBarTabsPage<TTabs> : NavBarPage where TTabs : Tabs, new()
    {
        public static TTabs Tabs { get; private set; }

        static NavBarTabsPage()
        {
            Nav.NavigationAnimationStarted.FullEvent += HandleNavigationAnimationStarted;

            Nav.FullRefreshed.Event += async () =>
            {
                if (Tabs != null)
                {
                    await Root.Remove(Tabs);
                    Tabs = null;
                }
            };
        }

        protected override async Task InitializeFromMarkup()
        {
            await base.InitializeFromMarkup();

            if (Tabs == null)
            {
                Tabs = new TTabs().Absolute().Hide();
                await Root.Add(Tabs);
            }
        }

        static void HandleNavigationAnimationStarted(NavigationEventArgs args)
        {
            if (Tabs == null) return;

            if (args.From is PopUp || args.To is PopUp) return;

            var shouldFadeIn = args.To is NavBarTabsPage<TTabs> && !(args.From is NavBarTabsPage<TTabs>);
            var shouldFadeOut = args.From is NavBarTabsPage<TTabs> && !(args.To is NavBarTabsPage<TTabs>);

            if (shouldFadeIn)
                Tabs?.Animate(Animation.DefaultDuration, x => x.Opacity(0).Visible(), x => x.Opacity(1)).RunInParallel();
            else if (shouldFadeOut)
                Tabs?.Animate(Animation.DefaultDuration, x => x.Opacity(1).Visible(), x => x.Opacity(0)).RunInParallel();
            else Tabs?.Visible(args.To is NavBarTabsPage<TTabs>);
        }

        protected virtual bool ShowTabsAtTheTop() => CssEngine.Platform == DevicePlatform.Android;

        public override async Task OnPreRender()
        {
            await base.OnPreRender();

            if (ShowTabsAtTheTop())
            {
                Height.BindTo(Root.Height);
                Tabs.Y.BindTo(NavBarBackground.Height);
                Tabs.Height.Changed.Event += () => BodyScrollerWrapper.Margin(top: Tabs.ActualBottom);
                Tabs.Y.Changed.Event += () => BodyScrollerWrapper.Margin(top: Tabs.ActualBottom);
                BodyScrollerWrapper.Y.BindTo(Tabs.Y, Tabs.Height, (y, h) => y + h);
            }
            else
            {
                Height.BindTo(Root.Height, Tabs.Height, (x, y) => x - y);
                Tabs.Y.BindTo(Root.Height, Tabs.Height, (x, y) => x - y);
                BodyScrollerWrapper.Y.BindTo(NavBarBackground.Height);
            }

            BodyScrollerWrapper.Height.BindTo(View.Root.Height, NavBarBackground.Height, Tabs.Height, (x, y, z) => x - y - z);

            await Tabs.BringToFront();
        }
    }
}
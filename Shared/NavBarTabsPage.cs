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

            var showTabs = args.To is NavBarTabsPage<TTabs>;
            var shouldFadeIn = showTabs && !(args.From is NavBarTabsPage<TTabs>);
            var shouldFadeOut = args.From is NavBarTabsPage<TTabs> && !showTabs;

            if (shouldFadeIn) AnimateTabsIn();
            else if (shouldFadeOut) AnimateTabsOut();
            else UIWorkBatch.RunSync(() => Tabs?.Visible(showTabs).Opacity(showTabs ? 1 : 0));
        }

        static void AnimateTabsIn()
        {
            Tabs?.Animate(Animation.DefaultDuration, AnimationEasing.Linear,
                       x => { x.Opacity(0).Visible(); },
                       x => x.Opacity(1)).RunInParallel();
        }

        static void AnimateTabsOut()
        {
            if (Tabs == null) return;
            var ani = Animation.Create(Tabs, Animation.DefaultDuration, AnimationEasing.Linear, change: x => x.Opacity(0))
                .OnCompleted(() => Tabs?.Hide());

            Tabs?.Animate(ani).RunInParallel();
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

            BodyScrollerWrapper.Height.BindTo(Root.Height, NavBarBackground.Height, Tabs.Height, (x, y, z) => x - y - z);

            await Tabs.Visible().BringToFront();
        }
    }
}
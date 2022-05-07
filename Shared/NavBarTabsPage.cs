namespace Zebble
{
    using System;
    using System.Threading.Tasks;
    using Services;
    using System.Linq;

    public abstract class NavBarTabsPage<TTabs> : NavBarPage where TTabs : Tabs, new()
    {
        public TTabs Tabs { get; private set; }

        protected NavBarTabsPage()
        {
            Nav.NavigationAnimationStarted.FullEvent += HandleNavigationAnimationStarted;
            Nav.FullRefreshed.Event += HandleFullRefreshed;
        }

        public override void Dispose()
        {
            Nav.NavigationAnimationStarted.FullEvent -= HandleNavigationAnimationStarted;
            Nav.FullRefreshed.Event -= HandleFullRefreshed;
            base.Dispose();
        }

        protected override async Task InitializeFromMarkup()
        {
            await base.InitializeFromMarkup();

            if (Tabs == null)
            {
                Tabs = new TTabs().Absolute().Hide();
                await Wrapper.Add(Tabs);
            }
        }

        void HandleNavigationAnimationStarted(NavigationEventArgs args)
        {
            if (Tabs == null) return;

            if (args.From is PopUp || args.To is PopUp) return;

            // This seems to be a timing issue?
            // We're navigating between two pages and CurrentPage is none of them?
            if (!Nav.CurrentPage.IsAnyOf(args.From, args.To)) return;

            var showTabs = args.To is NavBarTabsPage<TTabs>;
            var shouldFadeIn = showTabs && !(args.From is NavBarTabsPage<TTabs>);
            var shouldFadeOut = args.From is NavBarTabsPage<TTabs> && !showTabs;

            if (shouldFadeIn) AnimateTabsIn();
            else if (shouldFadeOut) AnimateTabsOut();
            else UIWorkBatch.RunSync(() => Tabs?.Visible(showTabs).Opacity(showTabs ? 1 : 0));
        }

        async void HandleFullRefreshed()
        {
            if (Tabs is null) return;

            await Wrapper.Remove(Tabs);
            Tabs = null;
        }

        void AnimateTabsIn()
        {
            Tabs?.Animate(Animation.DefaultDuration, AnimationEasing.Linear,
                       x => { x.Opacity(0).Visible(); },
                       x => x.Opacity(1)).RunInParallel();
        }

        void AnimateTabsOut()
        {
            if (Tabs == null) return;
            var ani = Animation.Create(Tabs, Animation.DefaultDuration, AnimationEasing.Linear, change: x => x.Opacity(0))
                .OnCompleted(() => Tabs?.Hide());

            Tabs?.Animate(ani).RunInParallel();
        }

        protected virtual bool ShowTabsAtTheTop() => CssEngine.Platform == DevicePlatform.Android;

        protected virtual bool HasKeyboardLayout() => CssEngine.Platform == DevicePlatform.Android && AllDescendents().OfType<TextInput>().Any();

        public override async Task OnPreRender()
        {
            await base.OnPreRender();

            if (ShowTabsAtTheTop())
            {
                Height.BindTo(Root.Height);
                Tabs.Y.BindTo(NavBarBackground.Height);
                Tabs.Height.Changed.Event += () => BodyScroller.Margin(top: Tabs.ActualBottom);
                Tabs.Y.Changed.Event += () => BodyScroller.Margin(top: Tabs.ActualBottom);
                BodyScroller.Y.BindTo(Tabs.Y, Tabs.Height, (y, h) => y + h);
            }
            else
            {
                if (HasKeyboardLayout()) Height.BindTo(Root.Height);
                else Height.BindTo(Root.Height, Tabs.Height, (x, y) => x - y);

                Tabs.Y.BindTo(Root.Height, Tabs.Height, (x, y) => x - y);
                BodyScroller.Y.BindTo(NavBarBackground.Height);
            }

            BodyScroller.Height.BindTo(Root.Height, NavBarBackground.Height, Tabs.Height, (x, y, z) => x - y - z);

            await Tabs.Visible().BringToFront();
        }
    }
}
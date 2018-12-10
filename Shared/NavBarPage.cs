namespace Zebble
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract class NavBarPage : Page
    {
        const int TOP_ZINDEX = 100000;
        public static Canvas NavBarBackground { get; private set; }
        protected readonly NavigationBar NavBar = new NavigationBar().Absolute().ZIndex(TOP_ZINDEX);
        public readonly Canvas BodyScrollerWrapper = new Canvas().Id("BodyScrollerWrapper");
        public readonly ScrollView BodyScroller = new ScrollView().Id("BodyScroller");
        public readonly Stack Body = new Stack { Id = "Body" };

        static NavBarPage()
        {
            Nav.NavigationAnimationStarted.Handle(OnNavigationAnimationStarted);
        }

        protected override async Task InitializeFromMarkup()
        {
            await base.InitializeFromMarkup();
            await AddViews();
        }

        protected virtual async Task AddViews()
        {
            await Add(NavBar);
            await Add(BodyScrollerWrapper);
            await BodyScrollerWrapper.Add(BodyScroller);
            await BodyScroller.Add(Body);
        }

        public override async Task OnInitializing()
        {
            await CreateNavBarBackground();
            await base.OnInitializing();
        }

        protected virtual async Task CreateNavBarBackground()
        {
            var result = new Canvas().Id("NavBarBackground")
                .Absolute()
                .ZIndex(TOP_ZINDEX - 1)
                .CssClass("navbar-background");

            if (!(Nav.CurrentPage is NavBarPage))
                result.Hide();

            await Add(result, awaitNative: true);

            NavBarBackground = result;
        }

        static Task OnNavigationAnimationStarted(NavigationEventArgs args)
        {
            if (args.From is PopUp || args.To is PopUp) return Task.CompletedTask;

            NavBarBackground?.Visible(args.To is NavBarPage);

            var fade = args.To is NavBarPage && args.To.Transition != PageTransition.None;

            if (fade)
            {
                (args.To as NavBarPage)?.NavBar.Perform(x =>
                x.Animate(Animation.DefaultDuration, b => b.Opacity(1)).RunInParallel());

                (args.From as NavBarPage)?.NavBar
                    .Perform(x => x.Animate(Animation.DefaultDuration.Multiply(1.5), b => b.Opacity(0))
                     .RunInParallel());
            }
            else
            {
                (args.To as NavBarPage)?.NavBar.Perform(b => b.Opacity(1));
            }

            return Task.CompletedTask;
        }

        public override async Task OnPreRender()
        {
            await base.OnPreRender();
            await NavBar.ApplyStyles();
            await AddBackOrMenu();

            NavBarBackground.Height.BindTo(NavBar.Height);
            NavBarBackground.Visible();

            BodyScrollerWrapper.Height.BindTo(Root.Height, NavBar.Height, (x, y) => x - y);
            BodyScrollerWrapper.Margin(top: NavBarBackground.ActualBottom);

            NavBar.Height.Changed.Handle(() => BodyScrollerWrapper.Margin(top: NavBar.ActualHeight));
        }

        protected virtual View CreateBackButton() => new IconButton { CssClass = "navbar-button back", Text = "Back" };

        protected virtual View CreateMenuIcon() => new ImageView { CssClass = "menu-icon", AutoFlash = true };

        protected virtual ButtonLocation GetMenuLocation() => ButtonLocation.Left;

        protected virtual ButtonLocation GetBackButtonLocation() => ButtonLocation.Left;

        /// <summary>
        /// Determines whether the menu should be displayed even if a back button is displayed.
        /// </summary>
        protected virtual bool ShowMenuDespiteBackButton() => false;

        protected virtual async Task AddBackOrMenu()
        {
            if (Nav.Stack.Any())
            {
                var back = CreateBackButton();
                back.AutoFlash = true;
                back.On(x => x.Tapped, Nav.Back);
                await NavBar.AddButton(GetBackButtonLocation(), back);

                if (!ShowMenuDespiteBackButton()) return;
            }

            if (Data.ContainsKey("TopMenu"))
            {
                var menu = CreateMenuIcon()?.Id("MenuIcon");
                if (menu != null)
                {
                    menu.On(x => x.Tapped, OnMenuTapped);
                    await NavBar.AddButton(GetMenuLocation(), menu);
                }
            }
        }

        public string Title
        {
            get => NavBar.Title.Text;
            set => NavBar.Title.Text = value;
        }

        public virtual NavigationBar GetNavBar() => NavBar;

        protected virtual Task OnMenuTapped()
        {
            throw new NotImplementedException("NavBarPage.OnMenuTapped() should be overriden.");
        }
    }
}
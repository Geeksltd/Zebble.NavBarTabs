namespace Zebble
{
    using System;
    using System.Threading.Tasks;
    using Olive;

    public class MainMenuLauncher<TMenu> : Canvas where TMenu : View, new()
    {
        public static MainMenuLauncher<TMenu> Current { get; private set; }
        public static Overlay Overlay = new Overlay();
        public readonly AsyncEvent Toggled = new AsyncEvent();
        public static TMenu Menu;
        bool IsExpanded, Animating, IsToggling;

        public float MenuWidth = 250, OverlayOpacity = 0.35f;

        static MainMenuLauncher()
        {
            Nav.FullRefreshed.Handle(async () =>
            {
                await (Overlay?.RemoveSelf()).OrCompleted();
                Overlay = new Overlay();

                await (Menu?.RemoveSelf()).OrCompleted();
                Menu = null;

                await (Current?.RemoveSelf()).OrCompleted();
                Current = null;
            });
        }

        protected MainMenuLauncher()
        {
            if (Current == null)
            {
                this.Hide();
                Current = this;
                Nav.HardwareBack.InsertHandler(HardwareBackHandler, 0);
            }
        }

        async Task HardwareBackHandler(HardwareBackEventArgs args)
        {
#if ANDROID
            if (IsExpanded)
            {
                await HideMenu();
                args.Cancel = true;
            }
#endif
        }

        public virtual async Task Setup()
        {
            if (Current != this) return; // Only the first time it's done.

            await Task.Delay(Animation.OneFrame);
            await Root.Add(Current, awaitNative: true);

            await Current.BringToFront();
        }

        public virtual async Task Show()
        {
            while (!IsRendered())
                await Task.Delay(Animation.OneFrame);

            await BringToFront();
            await ToggleMenu();
        }

        public override async Task ApplyStyles()
        {
            await base.ApplyStyles();
            await SetInitialMenuPosition();
        }

        public override async Task OnInitializing()
        {
            await base.OnInitializing();

            await Add(Overlay.On(x => x.Tapped, HideMenu));

            Nav.Navigated.Event += () => HideMenu().RunInParallel();
            Swiped.FullEvent += a => { if (a.Direction == Direction.Left) HideMenu().RunInParallel(); };

            await Add(Menu = new TMenu());
        }

        public async Task ToggleMenu()
        {
            if (Animating) return;
            Animating = true;
            IsToggling = true;
            if (IsExpanded) await HideMenu();
            else await ShowMenu();
            IsToggling = false;
            Animating = false;
            await Toggled.Raise();
        }

        public async Task ShowMenu()
        {
            if (IsExpanded) return;
            IsExpanded = true;

            this.Visible();

            AnimateOverlayIn().RunInParallel();
            await AnimateMenuIn();

            if (!IsToggling) await Toggled.Raise();
        }

        public async Task HideMenu()
        {
            if (!IsExpanded) return;
            IsExpanded = false;

            AnimateOverlayOut().RunInParallel();
            await AnimateMenuOut();

            this.Hide();

            if (!IsToggling) await Toggled.Raise();
        }

        protected virtual HorizontalAlignment Alignment => HorizontalAlignment.Left;

        protected virtual Task AnimateMenuIn()
        {
            if (Alignment == HorizontalAlignment.Left)
                return Menu.Animate(m => m.X(0));
            else if (Alignment == HorizontalAlignment.Right)
                return Menu.Animate(m => m.X(ActualWidth - MenuWidth));
            else throw new NotImplementedException(Alignment + " is not implemented.");
        }

        protected virtual Task AnimateMenuOut()
        {
            if (Alignment == HorizontalAlignment.Left)
                return Menu.Animate(m => m.X(-MenuWidth));
            else if (Alignment == HorizontalAlignment.Right)
                return Menu.Animate(m => m.X(100.Percent()));
            else throw new NotImplementedException(Alignment + " is not implemented.");
        }

        protected virtual Task AnimateOverlayIn()
        {
            return Overlay.Animate(
                    Animation.FadeDuration,
                    initialState: x => x.Opacity(0),
                    change: x => x.Opacity(OverlayOpacity));
        }

        protected virtual Task AnimateOverlayOut()
        {
            return Overlay.Animate(
                    Animation.FadeDuration,
                    x => x.Opacity(0));
        }

        protected virtual Task SetInitialMenuPosition()
        {
            Menu.Size(MenuWidth, 100.Percent());

            if (Alignment == HorizontalAlignment.Left)
                Menu.X(-MenuWidth);
            else if (Alignment == HorizontalAlignment.Right)
                Menu.X(100.Percent());
            else throw new NotImplementedException(Alignment + " is not implemented.");

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            Nav.HardwareBack.RemoveHandler(HardwareBackHandler);
            base.Dispose();
        }
    }
}
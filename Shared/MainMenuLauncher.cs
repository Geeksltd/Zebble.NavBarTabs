namespace Zebble
{
    using System;
    using System.Threading.Tasks;

    public class MainMenuLauncher<TMenu> : Canvas where TMenu : View, new()
    {
        public static MainMenuLauncher<TMenu> Current { get; private set; }
        public static Overlay Overlay = new Overlay();
        public static TMenu Menu;
        bool IsExpanded, Animating;

        public float MenuWidth = 250;

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
            }
        }

        public virtual async Task Setup()
        {
            if (Current != this) return; // Only the first time it's done.

            await Task.Delay(Animation.OneFrame);
            await Root.Add(Current, awaitNative: true);
        }

        public virtual async Task Show()
        {
            while (!IsAlreadyRendered())
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
            Nav.Navigated.Handle(HideMenu);
            Swiped.Handle(a => a.Direction == Direction.Left ? HideMenu() : Task.CompletedTask);
            await Add(Menu = new TMenu());
        }

        public async Task ToggleMenu()
        {
            if (Animating) return;
            Animating = true;
            if (IsExpanded) await HideMenu(); else await ShowMenu();
            Animating = false;
        }

        public async Task ShowMenu()
        {
            if (IsExpanded) return;
            IsExpanded = true;

            using (Stylesheet.Preserve(Overlay, x => x.Opacity))
            {
                var overlayOpacity = Overlay.Opacity;
                Overlay.Opacity(0);

                this.Visible();

                var overlayAnimation = Overlay.Animate(Animation.FadeDuration, x => x.Opacity(overlayOpacity));
                await AnimateMenuIn();
                await overlayAnimation;
            }
        }

        public async Task HideMenu()
        {
            if (!IsExpanded) return;
            IsExpanded = false;

            using (Stylesheet.Preserve(Overlay, x => x.Opacity))
            {
                Overlay.Animate(Animation.FadeDuration, x => x.Opacity(0)).RunInParallel();
                await AnimateMenuOut();
                this.Hide();
            }
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
    }
}